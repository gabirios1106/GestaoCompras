using GestaoCompras.Enums.Users;
using GestaoCompras.Utils.Converters;
using GestaoCompras.Web.Extensions;
using GestaoCompras.Web.Interfaces.Access;
using GestaoCompras.Web.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace GestaoCompras.Web.Services.Access;

public class SessionDataService(IConfiguration configuration, IJSRuntime jsRuntime, ILogger<SessionDataService> logger) : ISessionDataService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private readonly ILogger<SessionDataService> _logger = logger;

    private static AuthenticationState s_authenticationState;
    private static bool s_isAuthenticated;
    private static string s_userNameToRevalidate;
    private static int s_userDataId;
    private static string s_refreshToken;
    private static string s_token;
    private static string s_routeToRedirect = "Order";
    private static bool s_rememberMe;
    private static string s_userToRemember;
    private static UserRole s_UserRole;
    private static DateTime s_epoch = new DateTime(1970, 1, 1, 0, 0, 0);

    public AuthenticationHeaderValue GetBasicAuthorizationHeader()
    {
        _logger.LogInformation("Obter dados de configuração para gerar cabeçalho de autenticação básica");

        var envName = _configuration["EnvName"];

        var appId = AppSettings.GetAppSettings(envName, "AppCredential:AppId");
        var appSecret = AppSettings.GetAppSettings(envName, "AppCredential:AppSecret");

        var appCredentials = $"{appId}:{appSecret}";
        var authHeaderValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(appCredentials)));

        return authHeaderValue;
    }

    public AuthenticationHeaderValue GetBearerAuthorizationHeader()
    {
        _logger.LogInformation("Obter dados de configuração para gerar cabeçalho de autenticação bearer");

        var tokenJwt = GetToken();
        var authHeaderValue = new AuthenticationHeaderValue("Bearer", tokenJwt);

        return authHeaderValue;
    }

    public async Task<string> GetSavedUserIdAsync()
    {
        _logger.LogInformation("Obter o Id do usuário logado");

        var authenticationState = await GetAuthenticationState();
        var userId = Guid.NewGuid();

        var claimUserId = authenticationState.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        userId = claimUserId != null ? (Guid.TryParse(claimUserId.Value.ToString(), out Guid convertedUserId) ? convertedUserId : Guid.NewGuid()) : Guid.NewGuid();

        return userId.ToString();
    }

    public async Task<string> GetSavedUserNameAsync()
    {
        _logger.LogInformation("Obter o login do usuário logado");

        var authenticationState = await GetAuthenticationState();
        var claims = authenticationState.User.Claims;
        var claimEmail = authenticationState.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

        return claimEmail != null ? claimEmail.Value : string.Empty;
    }

    public async Task<string> GetSavedNameAsync()
    {
        _logger.LogInformation("Obter o nome do usuário logado");

        var authenticationState = await GetAuthenticationState();
        var claims = authenticationState.User.Claims;
        var claimName = authenticationState.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);

        return claimName != null ? claimName.Value : string.Empty;
    }

    public async Task<UserRole> GetSavedUserRoleAsync()
    {
        _logger.LogInformation("Obter o tipo de usuário logado");

        var tokenJWT = await _jsRuntime.GetItemFromSessionStorage("tokenJWT");

        UserRole userRole = UserRole.Funcionario;   

        if (!string.IsNullOrEmpty(tokenJWT))
        {
            var claim = CustomConverter.ParseClaimsFromJWT(tokenJWT).ToList().FirstOrDefault(c => c.Type == "UserRole");

            if (claim is not null)
            {
                if (Enum.TryParse<UserRole>(claim.Value.ToString(), out var userRoleValue))
                    userRole = userRoleValue;
            }
        }

        return userRole;
    }

    public async Task<DateTime> GetSavedInstantLoginAsync()
    {
        _logger.LogInformation("Obter o instante de login do usuário logado");

        var savedInstantLogin = s_epoch;
        var authenticationState = await GetAuthenticationState();

        var claimIat = authenticationState.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.AuthenticationInstant);

        if (long.TryParse(claimIat.Value, out long claimValue))
            savedInstantLogin = savedInstantLogin.AddSeconds(claimValue);

        return savedInstantLogin;
    }

    public async Task<DateTime> GetSavedExpirationAsync()
    {
        _logger.LogInformation("Obter a data e hora de expiração da sessão do usuário logado");

        var savedExpiration = s_epoch;
        var authenticationState = await GetAuthenticationState();

        var claimExp = authenticationState.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Expiration);

        if (long.TryParse(claimExp.Value, out long claimValue))
            savedExpiration = savedExpiration.AddSeconds(claimValue);

        return savedExpiration;
    }

    public async Task<double> GetTokenLifeTimeAsync()
    {
        _logger.LogInformation("Obter o tempo limite de sessão do usuário logado");

        var iat = await GetSavedInstantLoginAsync();
        var exp = await GetSavedExpirationAsync();

        var tokenLifeTime = exp.Subtract(iat).TotalSeconds;

        return (tokenLifeTime > 0.0) ? tokenLifeTime : 0.0;
    }

    public async Task<double> GetTimeSessionRemaining()
    {
        _logger.LogInformation("Obter o tempo restante da sessão do usuário logado");

        var now = DateTime.UtcNow;
        var exp = await GetSavedExpirationAsync();

        var timeSessionRemaining = exp.Subtract(now).TotalSeconds;

        return (timeSessionRemaining > 0.0) ? timeSessionRemaining : 0.0;
    }

    public async Task<AuthenticationState> GetAuthenticationState()
    {
        _logger.LogInformation("Obter o estado de autenticação da aplicação");

        var authenticationState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        if (s_authenticationState == null)
            await GetClaimsFromSessionStorageAsync();

        if (s_authenticationState != null)
            authenticationState = s_authenticationState;

        _logger.LogInformation("Estado de autenticação obtido: {AuthenticationState.User.Identity.IsAuthenticated}", authenticationState.User.Identity.IsAuthenticated);

        return await Task.FromResult(authenticationState);
    }

    public async void SetAuthenticationState(ClaimsPrincipal claimsPrincipal)
    {
        _logger.LogInformation("Definindo o estado de autenticação da aplicação");

        if (claimsPrincipal is null)
            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

        s_authenticationState = new AuthenticationState(claimsPrincipal);
        await SaveClaimsInSessionStorageAsync();

        _logger.LogInformation("Estado de autenticação definido: {AuthenticationState.User.Identity.IsAuthenticated}", s_authenticationState.User.Identity.IsAuthenticated);
    }

    public async Task GetClaimsFromSessionStorageAsync()
    {
        _logger.LogInformation("Obter claims, tokenJWT e refreshToken na Session Storage");

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

        var tokenJWT = await _jsRuntime.GetItemFromSessionStorage("tokenJWT");
        var refreshToken = await _jsRuntime.GetItemFromSessionStorage("refreshToken");

        if (!string.IsNullOrEmpty(tokenJWT) && !string.IsNullOrEmpty(refreshToken))
        {
            var sub = await _jsRuntime.GetItemFromSessionStorage("sub");
            var email = await _jsRuntime.GetItemFromSessionStorage("email");
            var name = await _jsRuntime.GetItemFromSessionStorage("name");
            var exp = await _jsRuntime.GetItemFromSessionStorage("exp");
            var iat = await _jsRuntime.GetItemFromSessionStorage("iat");

            if (!string.IsNullOrEmpty(sub) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(exp) && !string.IsNullOrEmpty(iat))
            {
                s_token = tokenJWT;
                s_refreshToken = refreshToken;

                var claimsIdentity = new ClaimsIdentity("JWT");

                var claimSub = new Claim(ClaimTypes.NameIdentifier, sub);
                var claimEmail = new Claim(ClaimTypes.Email, email);
                var claimName = new Claim(ClaimTypes.Name, name);
                var claimExp = new Claim(ClaimTypes.Expiration, exp);
                var claimIat = new Claim(ClaimTypes.AuthenticationInstant, iat);

                claimsIdentity.AddClaim(claimSub);
                claimsIdentity.AddClaim(claimEmail);
                claimsIdentity.AddClaim(claimName);
                claimsIdentity.AddClaim(claimExp);
                claimsIdentity.AddClaim(claimIat);

                claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                s_authenticationState = new AuthenticationState(claimsPrincipal);
            }
        }
    }

    public async Task SaveClaimsInSessionStorageAsync()
    {
        _logger.LogInformation("Salvar claims, tokenJWT e refreshToken na Session Storage");

        var authenticationState = await GetAuthenticationState();

        if (authenticationState.User.Claims.Any() && !string.IsNullOrEmpty(s_token) && !string.IsNullOrEmpty(s_refreshToken))
        {
            await _jsRuntime.SetItemInSessionStorage("tokenJWT", s_token);
            await _jsRuntime.SetItemInSessionStorage("refreshToken", s_refreshToken);
            await _jsRuntime.SetItemInSessionStorage("sub", authenticationState.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
            await _jsRuntime.SetItemInSessionStorage("email", authenticationState.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value);
            await _jsRuntime.SetItemInSessionStorage("name", authenticationState.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value);
            await _jsRuntime.SetItemInSessionStorage("exp", authenticationState.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Expiration).Value);
            await _jsRuntime.SetItemInSessionStorage("iat", authenticationState.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.AuthenticationInstant).Value);
        }
        else
            await _jsRuntime.ClearSessionStorage();
    }

    public string GetUserNameToRevalidate()
    {
        _logger.LogInformation("Obter login do usuário para revalidação: {UserNameToRevalidate}", s_userNameToRevalidate);

        return s_userNameToRevalidate;
    }

    public void SetUserNameToRevalidate(string userNameToRevalidate = "")
    {
        _logger.LogInformation("Definir login do usuário para revalidação: {UserNameToRevalidate}", userNameToRevalidate);

        s_userNameToRevalidate = userNameToRevalidate;
    }

    public int GetUserDataId()
    {
        _logger.LogInformation("Obter Id do usuário: {UserDataId}", s_userNameToRevalidate);

        return s_userDataId;
    }

    public void SetUserDataId(int userDataId = 0)
    {
        _logger.LogInformation("Definir Id do usuário: {UserDataId}", userDataId);

        s_userDataId = userDataId;
    }

    public bool GetIsAuthenticated()
    {
        _logger.LogInformation("Obter flag de autenticação do usuário: {IsAuthenticated}", s_isAuthenticated);
        return s_isAuthenticated;
    }

    public void SetIsAuthenticated(bool isAuthenticated)
    {
        _logger.LogInformation("Definir flag de autenticação do usuário: {IsAuthenticated}", isAuthenticated);
        s_isAuthenticated = isAuthenticated;
    }

    public string GetToken()
    {
        _logger.LogInformation("Obter token JWT da sessão logada");

        return s_token;
    }

    public void SetToken(string token)
    {
        _logger.LogInformation("Definir token JWT da sessão logada");

        s_token = token;
    }

    public string GetRefreshToken()
    {
        _logger.LogInformation("Obter refresh token JWT da sessão logada");

        return s_refreshToken;
    }

    public void SetRefreshToken(string refreshToken)
    {
        _logger.LogInformation("Definir refresh token JWT da sessão logada");

        s_refreshToken = refreshToken;
    }

    public async Task<bool> GetRememberMeAsync()
    {
        _logger.LogInformation("Obter opção de lembrar usuário");
        var rememberMe = await _jsRuntime.GetItemFromLocalStorage("rememberMe");

        if (!string.IsNullOrEmpty(rememberMe))
        {
            if (bool.TryParse(rememberMe, out bool rememberMeOption))
                s_rememberMe = rememberMeOption;
        }

        return s_rememberMe;
    }

    public async Task SetRememberMeAsync(bool rememberMe)
    {
        _logger.LogInformation("Definir opção de lembrar usuário");
        await _jsRuntime.SetItemInLocalStorage("rememberMe", rememberMe.ToString());

        s_rememberMe = rememberMe;
    }

    public async Task<string> GetUserToRemember()
    {
        _logger.LogInformation("Obter usuário a ser lembrado");
        s_userToRemember = await _jsRuntime.GetItemFromLocalStorage("userToRemember");

        return s_userToRemember;
    }

    public async Task SetUserToRememberAsync(string userToRemember = "")
    {
        _logger.LogInformation("Definir usuário a ser lembrado");
        await _jsRuntime.SetItemInLocalStorage("userToRemember", userToRemember);

        s_userToRemember = userToRemember;
    }

    public string GetRouteToRedirect()
    {
        _logger.LogInformation("Obter rota para redirecionamento da navegação: {RouteToRedirect}", s_routeToRedirect);
        return s_routeToRedirect;
    }

    public void SetRouteToRedirect(string routeToRedirect = "")
    {
        _logger.LogInformation("Definir rota para redirecionamento da navegação: {RouteToRedirect}", routeToRedirect);
        s_routeToRedirect = routeToRedirect;
    }
}
