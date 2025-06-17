using GestaoCompras.DTO.Common;
using GestaoCompras.Web.Interfaces.Access;
using GestaoCompras.Web.Interfaces.Common;
using GestaoCompras.Web.Services.Access;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace GestaoCompras.Web.Services.Common
{
    public class ApiService : IApiService
    {
        private readonly IErrorMessageService _errorMessageService;
        private readonly IHttpMessageService _httpMessageService;
        private readonly ISessionDataService _sessionDataService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TokenService _tokenService;
        private readonly ILogger<ApiService> _logger;

        private bool _isAuthenticated;
        private HttpClient _httpClient;

        public ApiService(IErrorMessageService errorMessageService, IHttpMessageService httpMessageService, ISessionDataService sessionDataService, IHttpClientFactory httpClientFactory, TokenService tokenService, ILogger<ApiService> logger)
        {
            _errorMessageService = errorMessageService;
            _httpMessageService = httpMessageService;
            _sessionDataService = sessionDataService;
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
            _logger = logger;

            _httpClient = _httpClientFactory.CreateClient("BackEndAPI");
        }

        public async Task<GetWithPaginationGetDTO<T>> GetAsync<T>(string requestUri)
        {
            _logger.LogInformation("Iniciando a requisição ao backend para obter lista com paginação");

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = _sessionDataService.GetBearerAuthorizationHeader();

                var httpResponse = await _httpClient.GetAsync(requestUri);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Requisição realizada e respondida com código HTTP de sucesso");
                    return JsonSerializer.Deserialize<GetWithPaginationGetDTO<T>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _logger.LogInformation("Requisição recusada por erro de autenticação");

                        _isAuthenticated = await _tokenService.TryToRefreshTokenAsync();

                        if (_isAuthenticated)
                        {
                            _logger.LogInformation("Nova requisição após atualização do token por RefreshToken");

                            _httpClient.DefaultRequestHeaders.Authorization = _sessionDataService.GetBearerAuthorizationHeader();

                            httpResponse = await _httpClient.GetAsync(requestUri);
                            responseContent = await httpResponse.Content.ReadAsStringAsync();

                            if (httpResponse.IsSuccessStatusCode)
                            {
                                _logger.LogInformation("Requisição realizada e respondida com código HTTP de sucesso após atualização do token por RefreshToken");
                                return JsonSerializer.Deserialize<GetWithPaginationGetDTO<T>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            }
                            else
                            {
                                _logger.LogInformation("Requisição realizada e respondida com código HTTP de erro após atualização do token por RefreshToken");

                                _httpMessageService.ShowMessage(httpResponse.StatusCode, null, 0, _httpClient.BaseAddress.ToString(), requestUri, httpResponse.ReasonPhrase, responseContent);
                                return null;
                            }
                        }
                        else
                            return null;
                    }
                    else
                    {
                        _logger.LogInformation("Requisição realizada e respondida com código HTTP de erro");

                        _httpMessageService.ShowMessage(httpResponse.StatusCode, null, 0, _httpClient.BaseAddress.ToString(), requestUri, httpResponse.ReasonPhrase, responseContent);
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                var routeAPI = _httpClient.BaseAddress.ToString() + requestUri;

                _logger.LogError("Erro ao acessar {RouteAPI} - {ErrorMessage}", routeAPI, e.Message);
                _errorMessageService.ShowErrorMessage($"{requestUri}", e.Message, _httpClient.BaseAddress.ToString());

                return null;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Clear();
            }
        }


        public async Task<List<T>> GetWithoutPaginationAsync<T>(string requestUri)
        {
            _logger.LogInformation("Iniciando a requisição ao backend para obter lista sem paginação");

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = _sessionDataService.GetBearerAuthorizationHeader();

                var httpResponse = await _httpClient.GetAsync(requestUri);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Requisição realizada e respondida com código HTTP de sucesso");
                    return JsonSerializer.Deserialize<List<T>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _logger.LogInformation("Requisição recusada por erro de autenticação");

                        _isAuthenticated = await _tokenService.TryToRefreshTokenAsync();

                        if (_isAuthenticated)
                        {
                            _logger.LogInformation("Nova requisição após atualização do token por RefreshToken");

                            _httpClient.DefaultRequestHeaders.Authorization = _sessionDataService.GetBearerAuthorizationHeader();

                            httpResponse = await _httpClient.GetAsync(requestUri);
                            responseContent = await httpResponse.Content.ReadAsStringAsync();

                            if (httpResponse.IsSuccessStatusCode)
                            {
                                _logger.LogInformation("Requisição realizada e respondida com código HTTP de sucesso após atualização do token por RefreshToken");
                                return JsonSerializer.Deserialize<List<T>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            }
                            else
                            {
                                _logger.LogInformation("Requisição realizada e respondida com código HTTP de erro após atualização do token por RefreshToken");

                                _httpMessageService.ShowMessage(httpResponse.StatusCode, null, 0, _httpClient.BaseAddress.ToString(), requestUri, httpResponse.ReasonPhrase, responseContent);
                                return null;
                            }
                        }
                        else
                            return null;
                    }
                    else
                    {
                        _logger.LogInformation("Requisição realizada e respondida com código HTTP de erro");

                        _httpMessageService.ShowMessage(httpResponse.StatusCode, null, 0, _httpClient.BaseAddress.ToString(), requestUri, httpResponse.ReasonPhrase, responseContent);
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                var routeAPI = _httpClient.BaseAddress.ToString() + requestUri;

                _logger.LogError("Erro ao acessar {RouteAPI} - {ErrorMessage}", routeAPI, e.Message);
                _errorMessageService.ShowErrorMessage($"{requestUri}", e.Message, _httpClient.BaseAddress.ToString());

                return null;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Clear();
            }
        }

        public async Task<T> GetByIdAsync<T>(string requestUri)
        {
            _logger.LogInformation("Iniciando a requisição ao backend para objeto");

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = _sessionDataService.GetBearerAuthorizationHeader();

                var httpResponse = await _httpClient.GetAsync(requestUri);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Requisição realizada e respondida com código HTTP de sucesso");
                    return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _logger.LogInformation("Requisição recusada por erro de autenticação");

                        _isAuthenticated = await _tokenService.TryToRefreshTokenAsync();

                        if (_isAuthenticated)
                        {
                            _logger.LogInformation("Nova requisição após atualização do token por RefreshToken");

                            _httpClient.DefaultRequestHeaders.Authorization = _sessionDataService.GetBearerAuthorizationHeader();

                            httpResponse = await _httpClient.GetAsync(requestUri);
                            responseContent = await httpResponse.Content.ReadAsStringAsync();

                            if (httpResponse.IsSuccessStatusCode)
                            {
                                _logger.LogInformation("Requisição realizada e respondida com código HTTP de sucesso após atualização do token por RefreshToken");
                                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            }
                            else
                            {
                                _logger.LogInformation("Requisição realizada e respondida com código HTTP de erro após atualização do token por RefreshToken");

                                _httpMessageService.ShowMessage(httpResponse.StatusCode, null, 0, _httpClient.BaseAddress.ToString(), requestUri, httpResponse.ReasonPhrase, responseContent);
                                return default;
                            }
                        }
                        else
                            return default;
                    }
                    else
                    {
                        _logger.LogInformation("Requisição realizada e respondida com código HTTP de erro");

                        _httpMessageService.ShowMessage(httpResponse.StatusCode, null, 0, _httpClient.BaseAddress.ToString(), requestUri, httpResponse.ReasonPhrase, responseContent);
                        return default;
                    }
                }
            }
            catch (Exception e)
            {
                var routeAPI = _httpClient.BaseAddress.ToString() + requestUri;

                _logger.LogError("Erro ao acessar {RouteAPI} - {ErrorMessage}", routeAPI, e.Message);
                _errorMessageService.ShowErrorMessage($"{requestUri}", e.Message, _httpClient.BaseAddress.ToString());

                return default;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Clear();
            }
        }

        public async Task<T> GetInfoAsync<T>(string requestUri)
        {
            _logger.LogInformation("Iniciando a requisição ao backend para objeto");

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = _sessionDataService.GetBearerAuthorizationHeader();

                var httpResponse = await _httpClient.GetAsync(requestUri);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Requisição realizada e respondida com código HTTP de sucesso");
                    return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _logger.LogInformation("Requisição recusada por erro de autenticação");

                        _isAuthenticated = await _tokenService.TryToRefreshTokenAsync();

                        if (_isAuthenticated)
                        {
                            _logger.LogInformation("Nova requisição após atualização do token por RefreshToken");

                            _httpClient.DefaultRequestHeaders.Authorization = _sessionDataService.GetBearerAuthorizationHeader();

                            httpResponse = await _httpClient.GetAsync(requestUri);
                            responseContent = await httpResponse.Content.ReadAsStringAsync();

                            if (httpResponse.IsSuccessStatusCode)
                            {
                                _logger.LogInformation("Requisição realizada e respondida com código HTTP de sucesso após atualização do token por RefreshToken");
                                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            }
                            else
                            {
                                _logger.LogInformation("Requisição realizada e respondida com código HTTP de erro após atualização do token por RefreshToken");

                                _httpMessageService.ShowMessage(httpResponse.StatusCode, null, 0, _httpClient.BaseAddress.ToString(), requestUri, httpResponse.ReasonPhrase, responseContent);
                                return default;
                            }
                        }
                        else
                            return default;
                    }
                    else
                    {
                        _logger.LogInformation("Requisição realizada e respondida com código HTTP de erro");

                        _httpMessageService.ShowMessage(httpResponse.StatusCode, null, 0, _httpClient.BaseAddress.ToString(), requestUri, httpResponse.ReasonPhrase, responseContent);
                        return default;
                    }
                }
            }
            catch (Exception e)
            {
                var routeAPI = _httpClient.BaseAddress.ToString() + requestUri;

                _logger.LogError("Erro ao acessar {RouteAPI} - {ErrorMessage}", routeAPI, e.Message);
                _errorMessageService.ShowErrorMessage($"{requestUri}", e.Message, _httpClient.BaseAddress.ToString());

                return default;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Clear();
            }
        }

        public async Task<bool> CreateAsync<T>(string requestUri, T value)
        {
            _logger.LogInformation("Iniciando a requisição para o backend para inserir dados");

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = _sessionDataService.GetBearerAuthorizationHeader();

                var httpResponse = await _httpClient.PostAsJsonAsync(requestUri, value);

                if (httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Requisição realizada e respondida com código HTTP de sucesso");
                    return true;
                }
                else
                {
                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _logger.LogInformation("Requisição recusada por erro de autenticação");

                        _isAuthenticated = await _tokenService.TryToRefreshTokenAsync();

                        if (_isAuthenticated)
                        {
                            _logger.LogInformation("Nova requisição após atualização do token por RefreshToken");

                            _httpClient.DefaultRequestHeaders.Authorization = _sessionDataService.GetBearerAuthorizationHeader();

                            httpResponse = await _httpClient.PostAsJsonAsync(requestUri, value);

                            if (httpResponse.IsSuccessStatusCode)
                            {
                                _logger.LogInformation("Requisição realizada e respondida com código HTTP de sucesso após atualização do token por RefreshToken");
                                return true;
                            }
                            else
                            {
                                _logger.LogInformation("Requisição realizada e respondida com código HTTP de erro após atualização do token por RefreshToken");

                                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                                _httpMessageService.ShowMessage(httpResponse.StatusCode, null, 0, _httpClient.BaseAddress.ToString(), requestUri, httpResponse.ReasonPhrase, responseContent);
                                return false;
                            }
                        }
                        else
                            return false;
                    }
                    else
                    {
                        _logger.LogInformation("Requisição realizada e respondida com código HTTP de erro");

                        var responseContent = await httpResponse.Content.ReadAsStringAsync();
                        _httpMessageService.ShowMessage(httpResponse.StatusCode, null, 0, _httpClient.BaseAddress.ToString(), requestUri, httpResponse.ReasonPhrase, responseContent);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                var routeAPI = _httpClient.BaseAddress.ToString() + requestUri;

                _logger.LogError("Erro ao acessar {RouteAPI} - {ErrorMessage}", routeAPI, e.Message);
                _errorMessageService.ShowErrorMessage($"{requestUri}", e.Message, _httpClient.BaseAddress.ToString());

                return false;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Clear();
            }
        }

        public async Task<G> CreateWithGetObjectAsync<G, P>(string requestUri, P value)
        {
            _logger.LogInformation("Iniciando a requisição para o backend para inserir dados");

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = _sessionDataService.GetBearerAuthorizationHeader();

                var httpResponse = await _httpClient.PostAsJsonAsync(requestUri, value);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Requisição realizada e respondida com código HTTP de sucesso");
                    return JsonSerializer.Deserialize<G>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _logger.LogInformation("Requisição recusada por erro de autenticação");

                        _isAuthenticated = await _tokenService.TryToRefreshTokenAsync();

                        if (_isAuthenticated)
                        {
                            _logger.LogInformation("Nova requisição após atualização do token por RefreshToken");

                            _httpClient.DefaultRequestHeaders.Authorization = _sessionDataService.GetBearerAuthorizationHeader();

                            httpResponse = await _httpClient.PostAsJsonAsync(requestUri, value);
                            responseContent = await httpResponse.Content.ReadAsStringAsync();

                            if (httpResponse.IsSuccessStatusCode)
                            {
                                _logger.LogInformation("Requisição realizada e respondida com código HTTP de sucesso após atualização do token por RefreshToken");
                                return JsonSerializer.Deserialize<G>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            }
                            else
                            {
                                _logger.LogInformation("Requisição realizada e respondida com código HTTP de erro após atualização do token por RefreshToken");

                                _httpMessageService.ShowMessage(httpResponse.StatusCode, null, 0, _httpClient.BaseAddress.ToString(), requestUri, httpResponse.ReasonPhrase, responseContent);
                                return default;
                            }
                        }
                        else
                            return default;
                    }
                    else
                    {
                        _logger.LogInformation("Requisição realizada e respondida com código HTTP de erro");

                        _httpMessageService.ShowMessage(httpResponse.StatusCode, null, 0, _httpClient.BaseAddress.ToString(), requestUri, httpResponse.ReasonPhrase, responseContent);
                        return default;
                    }
                }
            }
            catch (Exception e)
            {
                var routeAPI = _httpClient.BaseAddress.ToString() + requestUri;

                _logger.LogError("Erro ao acessar {RouteAPI} - {ErrorMessage}", routeAPI, e.Message);
                _errorMessageService.ShowErrorMessage($"{requestUri}", e.Message, _httpClient.BaseAddress.ToString());

                return default;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Clear();
            }
        }

        public async Task<bool> UpdateAsync<T>(string requestUri, T value)
        {
            _logger.LogInformation("Iniciando a requisição para o backend para atualizar dados");

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = _sessionDataService.GetBearerAuthorizationHeader();

                var httpResponse = await _httpClient.PutAsJsonAsync(requestUri, value);

                if (httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Requisição realizada e respondida com código HTTP de sucesso");
                    return true;
                }
                else
                {
                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _logger.LogInformation("Requisição recusada por erro de autenticação");

                        _isAuthenticated = await _tokenService.TryToRefreshTokenAsync();

                        if (_isAuthenticated)
                        {
                            _logger.LogInformation("Nova requisição após atualização do token por RefreshToken");

                            _httpClient.DefaultRequestHeaders.Authorization = _sessionDataService.GetBearerAuthorizationHeader();

                            httpResponse = await _httpClient.PutAsJsonAsync(requestUri, value);

                            if (httpResponse.IsSuccessStatusCode)
                            {
                                _logger.LogInformation("Requisição realizada e respondida com código HTTP de sucesso após atualização do token por RefreshToken");
                                return true;
                            }
                            else
                            {
                                _logger.LogInformation("Requisição realizada e respondida com código HTTP de erro após atualização do token por RefreshToken");

                                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                                _httpMessageService.ShowMessage(httpResponse.StatusCode, null, 0, _httpClient.BaseAddress.ToString(), requestUri, httpResponse.ReasonPhrase, responseContent);
                                return false;
                            }
                        }
                        else
                            return false;
                    }
                    else
                    {
                        _logger.LogInformation("Requisição realizada e respondida com código HTTP de erro");

                        var responseContent = await httpResponse.Content.ReadAsStringAsync();
                        _httpMessageService.ShowMessage(httpResponse.StatusCode, null, 0, _httpClient.BaseAddress.ToString(), requestUri, httpResponse.ReasonPhrase, responseContent);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                var routeAPI = _httpClient.BaseAddress.ToString() + requestUri;

                _logger.LogError("Erro ao acessar {RouteAPI} - {ErrorMessage}", routeAPI, e.Message);
                _errorMessageService.ShowErrorMessage($"{requestUri}", e.Message, _httpClient.BaseAddress.ToString());

                return false;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Clear();
            }
        }

        public async Task<bool> DeleteAsync(string requestUri)
        {
            _logger.LogInformation("Iniciando a requisição para o backend para excluir dados");

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = _sessionDataService.GetBearerAuthorizationHeader();

                var httpResponse = await _httpClient.DeleteAsync(requestUri);

                if (httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Requisição realizada e respondida com código HTTP de sucesso");
                    return true;
                }
                else
                {
                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _logger.LogInformation("Requisição recusada por erro de autenticação");

                        _isAuthenticated = await _tokenService.TryToRefreshTokenAsync();

                        if (_isAuthenticated)
                        {
                            _logger.LogInformation("Nova requisição após atualização do token por RefreshToken");

                            _httpClient.DefaultRequestHeaders.Authorization = _sessionDataService.GetBearerAuthorizationHeader();

                            httpResponse = await _httpClient.DeleteAsync(requestUri);

                            if (httpResponse.IsSuccessStatusCode)
                            {
                                _logger.LogInformation("Requisição realizada e respondida com código HTTP de sucesso após atualização do token por RefreshToken");
                                return true;
                            }
                            else
                            {
                                _logger.LogInformation("Requisição realizada e respondida com código HTTP de erro após atualização do token por RefreshToken");

                                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                                _httpMessageService.ShowMessage(httpResponse.StatusCode, null, 0, _httpClient.BaseAddress.ToString(), requestUri, httpResponse.ReasonPhrase, responseContent);
                                return false;
                            }
                        }
                        else
                            return false;
                    }
                    else
                    {
                        _logger.LogInformation("Requisição realizada e respondida com código HTTP de erro");

                        var responseContent = await httpResponse.Content.ReadAsStringAsync();
                        _httpMessageService.ShowMessage(httpResponse.StatusCode, null, 0, _httpClient.BaseAddress.ToString(), requestUri, httpResponse.ReasonPhrase, responseContent);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                var routeAPI = _httpClient.BaseAddress.ToString() + requestUri;

                _logger.LogError("Erro ao acessar {RouteAPI} - {ErrorMessage}", routeAPI, e.Message);
                _errorMessageService.ShowErrorMessage($"{requestUri}", e.Message, _httpClient.BaseAddress.ToString());

                return false;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Clear();
            }
        }
    }
}
