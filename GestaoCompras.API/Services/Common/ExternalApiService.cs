using GestaoCompras.API.Interfaces.Common;
using GestaoCompras.Utils.Classes;
using System.Text.Json;

namespace GestaoCompras.API.Services.Common
{
    public class ExternalApiService : IExternalApiService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ExternalApiService> _logger;
        private static readonly double s_externalApiTimeout = 60.0;
        private static string s_googleReCaptchaUri;
        public static HttpClient s_httpGoogleReCaptchaClient;

        public ExternalApiService(IConfiguration configuration, ILogger<ExternalApiService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            s_googleReCaptchaUri = _configuration.GetValue<string>("GoogleReCaptcha:GoogleReCaptchaUri");
            s_httpGoogleReCaptchaClient = new()
            {
                BaseAddress = new Uri(s_googleReCaptchaUri),
                Timeout = TimeSpan.FromSeconds(s_externalApiTimeout)
            };
        }

        public async Task<GoogleReCaptchaResponse> GetGoogleReCaptchaVerify(string requestUri, string token, string secretKey)
        {
            var apiName = "Google ReCaptcha";

            try
            {
                var googleReCaptchaData = new GoogleReCaptchaData(token, secretKey);
                var googleReCaptchaResponse = new GoogleReCaptchaResponse();

                var objectContent = new[]
                {
                new KeyValuePair<string, string>("secret", secretKey),
                new KeyValuePair<string, string>("response", token)
            };

                var postContent = new FormUrlEncodedContent(objectContent);

                var httpResponse = await s_httpGoogleReCaptchaClient.PostAsync(requestUri, postContent);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                    return JsonSerializer.Deserialize<GoogleReCaptchaResponse>(responseContent);
                else
                    throw new Exception($"{httpResponse.ReasonPhrase} - {httpResponse.RequestMessage}");
            }
            catch (TaskCanceledException e)
            {
                _logger.LogError("Falha na chamada da API GoogleReCaptchaVerify. A tarefa foi cancelada porque a espera pelo retorno da API {ApiName} na chamada do endpoint {RequestUri} superou tempo limite de {ExternalApiTimeout} segundos. Por favor, tente novamente mais tarde. ({Message})", apiName, requestUri, s_externalApiTimeout, e.Message);
                throw new Exception($"Falha na chamada da API GoogleReCaptchaVerify. A operação foi cancelada porque a espera pelo retorno da API {apiName} na chamada do endpoint {requestUri} superou tempo limite de {s_externalApiTimeout} segundos. Por favor, tente novamente mais tarde. ({e.Message})");
            }
            catch (OperationCanceledException e)
            {
                _logger.LogError("Falha na chamada da API GoogleReCaptchaVerify. A tarefa foi cancelada porque a espera pelo retorno da API {ApiName} na chamada do endpoint {RequestUri} superou tempo limite de {ExternalApiTimeout} segundos. Por favor, tente novamente mais tarde. ({Message})", apiName, requestUri, s_externalApiTimeout, e.Message);
                throw new Exception($"Falha na chamada da API GoogleReCaptchaVerify. A operação foi cancelada porque a espera pelo retorno da API {apiName} na chamada do endpoint {requestUri} superou tempo limite de {s_externalApiTimeout} segundos. Por favor, tente novamente mais tarde. ({e.Message})");
            }
            catch (Exception e)
            {
                _logger.LogError("Falha na chamada da API GoogleReCaptchaVerify. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
                throw new Exception(e.Message);
            }
        }
    }

}
