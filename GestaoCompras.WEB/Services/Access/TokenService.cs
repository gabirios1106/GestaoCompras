using GestaoCompras.DTO.Access;
using GestaoCompras.Utils.Converters;
using GestaoCompras.Web.Interfaces.Access;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace GestaoCompras.Web.Services.Access;

public class TokenService(IJSRuntime jsRuntime, NavigationManager navigationManager, IAccessService accessService, ISessionDataService sessionDataService, ILogger<TokenService> logger) : AuthenticationStateProvider, ITokenService
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private readonly NavigationManager _navigationManager = navigationManager;
    private readonly IAccessService _accessService = accessService;
    private readonly ISessionDataService _sessionDataService = sessionDataService;
    private readonly ILogger<TokenService> _logger = logger;

    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var authenticationState = await _sessionDataService.GetAuthenticationState();
        _logger.LogInformation("Obtenção do estado de autenticação da aplicação pelo provedor de autenticação: {AuthenticationState.User.Identity.IsAuthenticated}", authenticationState.User.Identity.IsAuthenticated);

        return authenticationState;
    }

    public async Task<bool> TryToRefreshTokenAsync()
    {
        _logger.LogInformation("Iniciando o processo de RefreshToken");

        #region RefreshToken
        var result = false;

        var refreshTokenDTO = new RefreshTokenGetDTO()
        {
            PasswordHash = string.Empty,
            RefreshToken = _sessionDataService.GetRefreshToken(),
            Revalidate = false
        };

        var refreshedTokenDTO = await _accessService.RefreshTokenAsync(refreshTokenDTO);

        if (refreshedTokenDTO != null)
        {
            await UserRefreshTokenAsync(refreshedTokenDTO);
            result = true;

            _logger.LogInformation("Token atualizado utilizando RefreshToken");
        }
        #endregion RefreshToken

        #region Logout
        if (!result)
        {
            result = false;

            UserLogoutAsync(false);
            _sessionDataService.SetAuthenticationState(null);

            _sessionDataService.SetRouteToRedirect(_navigationManager.ToBaseRelativePath(_navigationManager.Uri));
            _navigationManager.NavigateTo("/");

            _logger.LogInformation("O token não foi atualizado com o RefreshToken disponível");

        }
        #endregion Logout

        _sessionDataService.SetIsAuthenticated(result);

        return result;
    }

    public async Task UserLoginAsync(AuthenticatedUserGetDTO authenticatedUserGetDTO)
    {
        _logger.LogInformation("Iniciando o processo de login no provedor de autenticação da aplicação");

        try
        {
            var authenticationState = await UpdateAuthenticationState(authenticatedUserGetDTO: authenticatedUserGetDTO);
            var userName = await _sessionDataService.GetSavedUserNameAsync();

            _sessionDataService.SetUserNameToRevalidate(userName);
            _sessionDataService.SetUserDataId(authenticatedUserGetDTO.UserDataId);

            NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));

            _logger.LogInformation("Estado de autenticação notificado para a aplicação: {AuthenticationState.User.Identity.IsAuthenticated}", authenticationState.User.Identity.IsAuthenticated);
        }
        catch (Exception e)
        {
            _logger.LogError("Erro ao notificar estado de autenticação para a aplicação. Message: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            throw new Exception(e.Message);
        }
    }

    public async Task UserRefreshTokenAsync(RefreshTokenGetDTO refreshTokenDTO)
    {
        _logger.LogInformation("Atualizando login no provedor de autenticação da aplicação através de RefreshToken");

        try
        {
            var authenticationState = await UpdateAuthenticationState(refreshTokenDTO: refreshTokenDTO);
            var userName = await _sessionDataService.GetSavedUserNameAsync();

            _sessionDataService.SetUserNameToRevalidate(userName);

            NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));

            _logger.LogInformation("Estado de autenticação notificado para a aplicação: {AuthenticationState.User.Identity.IsAuthenticated}", authenticationState.User.Identity.IsAuthenticated);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async void UserLogoutAsync(bool clearUserName = true)
    {
        _logger.LogInformation("Iniciando o processo de logout no provedor de autenticação da aplicação");

        try
        {
            if (clearUserName)
            {
                _sessionDataService.SetUserNameToRevalidate();
                _sessionDataService.SetUserDataId();
            }

            _sessionDataService.SetAuthenticationState(null);

            var authenticationState = await _sessionDataService.GetAuthenticationState();
            NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));

            _logger.LogInformation("Estado de autenticação notificado para a aplicação: {AuthenticationState.User.Identity.IsAuthenticated}", authenticationState.User.Identity.IsAuthenticated);
        }
        catch (Exception e)
        {
            _logger.LogError("Erro ao notificar estado de autenticação para a aplicação. Message: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            throw new Exception(e.Message);
        }
    }

    public async Task<AuthenticationState> UpdateAuthenticationState(AuthenticatedUserGetDTO authenticatedUserGetDTO = null, RefreshTokenGetDTO refreshTokenDTO = null)
    {
        _logger.LogInformation("Iniciando a extração de informações do token JWT para autenticar o usuário na aplicação");

        var tokenJWT = (authenticatedUserGetDTO is null) ? refreshTokenDTO.Token : authenticatedUserGetDTO.Token;
        var refreshTokenJWT = (authenticatedUserGetDTO is null) ? refreshTokenDTO.RefreshToken : authenticatedUserGetDTO.RefreshToken;

        _sessionDataService.SetToken(tokenJWT);
        _sessionDataService.SetRefreshToken(refreshTokenJWT);

        var claims = CustomConverter.ParseClaimsFromJWT(tokenJWT);

        var claimsIdentity = new ClaimsIdentity("JWT");

        foreach (var claim in claims)
        {
            var item = claim.Type switch
            {
                ("sub") => ClaimTypes.NameIdentifier,
                ("email") => ClaimTypes.Email,
                ("name") => ClaimTypes.Name,
                ("UserRole") => ClaimTypes.Role,
                ("exp") => ClaimTypes.Expiration,
                ("iat") => ClaimTypes.AuthenticationInstant,
                _ => claim.Type,
            };

            if (!string.IsNullOrEmpty(item))
            {
                var customClaim = new Claim(item, claim.Value);
                claimsIdentity.AddClaim(customClaim);

                _logger.LogInformation("Claim {ClaimType} com o valor {ClaimValue} extraída do token JWT", item, claim.Value);
            }
        }

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        _sessionDataService.SetAuthenticationState(claimsPrincipal);

        var authenticationState = await _sessionDataService.GetAuthenticationState();

        return authenticationState;
    }
}
