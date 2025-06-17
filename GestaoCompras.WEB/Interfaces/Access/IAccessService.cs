using GestaoCompras.DTO.Access;

namespace GestaoCompras.Web.Interfaces.Access
{
    public interface IAccessService
    {
        Task<AuthenticatedUserGetDTO> LoginAsync(UserLoginPostDTO userLoginPostDTO);

        Task<RefreshTokenGetDTO> RefreshTokenAsync(RefreshTokenGetDTO refreshTokenDTO);

        Task<bool> LogoutAsync();
    }
}
