using GestaoCompras.Enums.Users;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace GestaoCompras.Web.Interfaces.Access
{
    public interface ISessionDataService
    {
        AuthenticationHeaderValue GetBasicAuthorizationHeader();

        AuthenticationHeaderValue GetBearerAuthorizationHeader();

        Task<string> GetSavedUserIdAsync();

        Task<string> GetSavedUserNameAsync();

        Task<string> GetSavedNameAsync();

        Task<UserRole> GetSavedUserRoleAsync();

        Task<DateTime> GetSavedInstantLoginAsync();

        Task<DateTime> GetSavedExpirationAsync();

        Task<double> GetTokenLifeTimeAsync();

        Task<double> GetTimeSessionRemaining();

        Task<AuthenticationState> GetAuthenticationState();

        void SetAuthenticationState(ClaimsPrincipal claimsPrincipal);

        Task GetClaimsFromSessionStorageAsync();

        Task SaveClaimsInSessionStorageAsync();

        string GetUserNameToRevalidate();

        void SetUserNameToRevalidate(string userNameToRevalidate = "");

        int GetUserDataId();

        void SetUserDataId(int userDataId = 0);

        bool GetIsAuthenticated();

        void SetIsAuthenticated(bool isAuthenticated);

        string GetToken();

        void SetToken(string token);

        string GetRefreshToken();

        void SetRefreshToken(string refreshToken);

        Task<bool> GetRememberMeAsync();

        Task SetRememberMeAsync(bool rememberMe);

        Task<string> GetUserToRemember();

        Task SetUserToRememberAsync(string userToRemember = "");

        string GetRouteToRedirect();

        void SetRouteToRedirect(string routeToRedirect);
    }
}
