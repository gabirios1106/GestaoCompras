using GestaoCompras.DTO.Access;
using GestaoCompras.Web.Interfaces.Access;
using GestaoCompras.Web.Interfaces.Common;
using MudBlazor;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using System.Text;

namespace GestaoCompras.Web.Services.Access;

public class AccessService : IAccessService
{
    private readonly ISnackbarService _snackbarService;
    private readonly IHttpMessageService _httpMessageService;
    private readonly ISessionDataService _sessionDataService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SessionDataService> _logger;

    private HttpClient _httpClient;

    public AccessService(ISnackbarService snackbarService, IHttpMessageService httpMessageService, ISessionDataService sessionDataService, IHttpClientFactory httpClientFactory, HttpClient httpClient, ILogger<SessionDataService> logger)
    {
        _snackbarService = snackbarService;
        _httpMessageService = httpMessageService;
        _sessionDataService = sessionDataService;
        _httpClientFactory = httpClientFactory;
        _httpClient = httpClient;
        _logger = logger;

        _httpClient = _httpClientFactory.CreateClient("BackEndAPI");
    }

    #region BasicAuthorization
    public async Task<AuthenticatedUserGetDTO> LoginAsync(UserLoginPostDTO userLoginPostDTO)
    {
        _logger.LogInformation("Iniciando chamada de API para login do usuário {UserName}", userLoginPostDTO.UserName);

        try
        {
            _logger.LogInformation("Obter cabeçalho de autorização da aplicação");
            _httpClient.DefaultRequestHeaders.Authorization = _sessionDataService.GetBasicAuthorizationHeader();

            var loginAsJSon = JsonSerializer.Serialize(userLoginPostDTO);
            var httpResponse = await _httpClient.PostAsync("v1/Access/UserLogin", new StringContent(loginAsJSon, Encoding.UTF8, "application/json"));
            var responseContent = await httpResponse.Content.ReadAsStringAsync();

            _logger.LogInformation("Realizada a chamada da API no endpoint 'v1/Access/UserLogin");

            if (httpResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Sucesso na chamada da API no endpoint 'v1/Access/UserLogin. Código da resposta HTTP: {HttpStatusCode}", httpResponse.StatusCode);
                return JsonSerializer.Deserialize<AuthenticatedUserGetDTO>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                _logger.LogWarning("Falha na chamada da API no endpoint 'v1/Access/UserLogin. Código da resposta HTTP: {HttpStatusCode}", httpResponse.StatusCode);
                _httpMessageService.ShowMessage(httpResponse.StatusCode, null, 0, _httpClient.BaseAddress.ToString(), "v1/Access/UserLogin", httpResponse.ReasonPhrase, responseContent);

                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Erro ao acionar a API no endpoint 'v1/Access/UserLogin. Message: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            _snackbarService.ShowSnackbar("Erro", "Houve uma falha ao tentar realizar o login", Severity.Error, 3500, "Pressione F12 para maiores detalhes");

            return null;
        }
        finally
        {
            _logger.LogInformation("Cabeçalhos HTTP excluídos");
            _httpClient.DefaultRequestHeaders.Clear();
        }
    }

    public async Task<RefreshTokenGetDTO> RefreshTokenAsync(RefreshTokenGetDTO refreshTokenDTO)
    {
        _logger.LogInformation("Iniciando chamada de API para atualização de token JWT");

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = _sessionDataService.GetBasicAuthorizationHeader();

            var refreshTokenAsJson = JsonSerializer.Serialize(refreshTokenDTO);
            var httpResponse = await _httpClient.PostAsync("v1/Access/RefreshToken", new StringContent(refreshTokenAsJson, Encoding.UTF8, "application/json"));
            var responseContent = await httpResponse.Content.ReadAsStringAsync();

            if (httpResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Sucesso na chamada da API no endpoint 'v1/Access/RefreshToken. Código da resposta HTTP: {HttpStatusCode}", httpResponse.StatusCode);
                return JsonSerializer.Deserialize<RefreshTokenGetDTO>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                _logger.LogWarning("Falha na chamada da API no endpoint 'v1/Access/RefreshToken. Código da resposta HTTP: {HttpStatusCode}", httpResponse.StatusCode);
                _httpMessageService.ShowMessage(httpResponse.StatusCode, null, 0, _httpClient.BaseAddress.ToString(), "v1/Access/RefreshToken", httpResponse.ReasonPhrase, responseContent);

                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Erro ao acionar a API no endpoint 'v1/Access/RefreshToken. Message: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            _snackbarService.ShowSnackbar("Erro", "Houve uma falha ao tentar atualizar o token", Severity.Error, 3500, "Pressione F12 para maiores detalhes");

            return null;
        }
        finally
        {
            _logger.LogInformation("Cabeçalhos HTTP excluídos");
            _httpClient.DefaultRequestHeaders.Clear();
        }
    }
    #endregion BasicAuthorization

    #region Anonymous
    public async Task<bool> LogoutAsync()
    {
        _logger.LogInformation("Iniciando chamada de API para desconectar usuário");

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = _sessionDataService.GetBearerAuthorizationHeader();

            var httpResponse = await _httpClient.PostAsJsonAsync("v1/Access/UserLogout", string.Empty);
            var responseContent = await httpResponse.Content.ReadAsStringAsync();

            if (httpResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Sucesso na chamada da API no endpoint 'v1/Access/UserLogout. Código da resposta HTTP: {HttpStatusCode}", httpResponse.StatusCode);
                return true;
            }
            else
            {
                _logger.LogWarning("Falha na chamada da API no endpoint 'v1/Access/UserLogout. Código da resposta HTTP: {HttpStatusCode}", httpResponse.StatusCode);
                _httpMessageService.ShowMessage(httpResponse.StatusCode, null, 0, _httpClient.BaseAddress.ToString(), "v1/Access/UserLogout", httpResponse.ReasonPhrase, responseContent);

                return (httpResponse.StatusCode == HttpStatusCode.BadRequest);
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Erro ao acionar a API no endpoint 'v1/Access/UserLogout. Message: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            _snackbarService.ShowSnackbar("Erro", "Houve uma falha ao tentar realizar o logout", Severity.Error, 3500, "Pressione F12 para maiores detalhes");

            return false;
        }
        finally
        {
            _httpClient.DefaultRequestHeaders.Clear();
        }

    }
    #endregion Anonymous
}
