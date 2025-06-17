using GestaoCompras.Models.Access;
using GestaoCompras.Models.Users;
using Microsoft.IdentityModel.JsonWebTokens;

namespace GestaoCompras.API.Interfaces.Access
{
    public interface ITokenService
    {
        Task<Guid> ValidateTokenAsync(string token, bool returnUserId = true);

        Task<JsonWebToken> ValidateRefreshTokenAsync(string refreshToken);

        string GenerateToken(User user, UserData userData, DateTime utcNow);

        string GenerateRefreshToken(User user, UserData userData, DateTime utcNow, string token);

        Task SaveRefreshTokenAsync(string token, string refreshToken, int userDataId);

        Task DisconnectUserAsync(Guid tokenJwtId);

    }
}
