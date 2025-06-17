using GestaoCompras.API.Interfaces.Access;
using GestaoCompras.API.Interfaces.Common;
using GestaoCompras.Utils.Classes;
using System.Security.Cryptography;
using System.Text;

namespace GestaoCompras.API.Services.Access
{
    public class GoogleReCaptchaService(IExternalApiService externalApiService, ILogger<GoogleReCaptchaService> logger) : IGoogleReCaptchaService
    {
        private readonly IExternalApiService _externalApiService = externalApiService;
        private readonly ILogger<GoogleReCaptchaService> _logger = logger;

        public async Task<GoogleReCaptchaResponse> GetGoogleReCaptchaVerify(string requestUri, string token, string secretKey)
        {
            try
            {
                return await _externalApiService.GetGoogleReCaptchaVerify(requestUri, token, secretKey);
            }
            catch (Exception e)
            {
                _logger.LogError("Erro na chamada da API de verificação do ReCaptcha. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
                throw new Exception(e.Message);
            }
        }
    }
}
