using GestaoCompras.DTO.Access;
using Microsoft.AspNetCore.Components.Authorization;

namespace GestaoCompras.Web.Interfaces.Access;

public interface ITokenService
{
    Task<bool> TryToRefreshTokenAsync();

    Task UserLoginAsync(AuthenticatedUserGetDTO authenticatedUserDTO);

    Task UserRefreshTokenAsync(RefreshTokenGetDTO refreshTokenDTO);

    void UserLogoutAsync(bool clearUserName = true);

    Task<AuthenticationState> UpdateAuthenticationState(AuthenticatedUserGetDTO authenticatedUserGetDTO = null, RefreshTokenGetDTO refreshTokenDTO = null);

}
