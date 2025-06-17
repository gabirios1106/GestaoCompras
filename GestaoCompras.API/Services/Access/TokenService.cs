using GestaoCompras.API.Data;
using GestaoCompras.API.Interfaces.Access;
using GestaoCompras.API.Utils.ErrorHandler;
using GestaoCompras.Models.Access;
using GestaoCompras.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace GestaoCompras.API.Services.Access
{
    public class TokenService : ITokenService
    {
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenService> _logger;

        private static string s_secretKey;
        private static double s_tokenLifeTime;
        private static double s_refreshTokenLifeTime;
        private static string s_issuer;
        private static string s_audienceWeb;
        private const string JwtExceptionMessage = "Token expirado";

        public TokenService(IUserService userService, ApplicationDbContext context, IConfiguration configuration, ILogger<TokenService> logger)
        {
            _userService = userService;
            _context = context;
            _configuration = configuration;
            _logger = logger;

            s_tokenLifeTime = _configuration.GetValue<double>("Jwt:Exp");
            s_refreshTokenLifeTime = _configuration.GetValue<double>("Jwt:RefreshTokenExp");
            s_secretKey = _configuration.GetValue<string>("Jwt:Key");
            s_issuer = _configuration.GetValue<string>("Jwt:Issuer");
            s_audienceWeb = _configuration.GetValue<string>("Jwt:AudienceWeb");
        }

        public async Task<Guid> ValidateTokenAsync(string token, bool returnUserId = true)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    throw new TokenJwtException(JwtExceptionMessage);

                token = token.Replace("Bearer ", string.Empty);

                var handler = new JsonWebTokenHandler();

                if (!handler.CanReadToken(token))
                    throw new TokenJwtException(JwtExceptionMessage);

                var tokenJwt = handler.ReadJsonWebToken(token);
                var tokenJwtId = Guid.TryParse(tokenJwt.Id, out Guid jit) ? jit : Guid.Empty;

                var userData = await _context.UserData.AsNoTracking().FirstOrDefaultAsync(ud => ud.TokenJwtId == tokenJwtId);

                if (userData == null)
                    throw new TokenJwtException(JwtExceptionMessage);

                return returnUserId ? userData.UserId : Guid.Empty;
            }
            catch (Exception e)
            {
                _logger.LogError("Erro ao validar token JWT. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
                throw new TokenJwtException(JwtExceptionMessage);
            }
        }

        public async Task<JsonWebToken> ValidateRefreshTokenAsync(string refreshToken)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshToken))
                    throw new TokenJwtException(JwtExceptionMessage);

                refreshToken = refreshToken.Replace("Bearer ", string.Empty);

                var handler = new JsonWebTokenHandler();

                if (!handler.CanReadToken(refreshToken))
                    throw new TokenJwtException(JwtExceptionMessage);

                var refreshTokenJwt = handler.ReadJsonWebToken(refreshToken);
                var refreshTokenJwtId = Guid.TryParse(refreshTokenJwt.Id, out Guid jit) ? jit : Guid.Empty;

                var refreshTokenIsActive = await _context.UserData.AnyAsync(ud => ud.RefreshTokenJwtId == refreshTokenJwtId);

                if (!refreshTokenIsActive)
                    throw new TokenJwtException(JwtExceptionMessage);

                var validateResult = await handler.ValidateTokenAsync(refreshToken, new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(s_secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = s_issuer,
                    ValidateAudience = true,
                    ValidAudience = s_audienceWeb,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                });

                if (!validateResult.IsValid)
                    throw new TokenJwtException(JwtExceptionMessage);

                var userName = refreshTokenJwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email).Value;
                var userId = Guid.TryParse(refreshTokenJwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub).Value, out Guid subject) ? subject : Guid.Empty;

                if (string.IsNullOrEmpty(userName) || userId == Guid.Empty)
                    throw new TokenJwtException(JwtExceptionMessage);

                return refreshTokenJwt;
            }
            catch (Exception e)
            {
                _logger.LogError("Erro ao gerar token JWT. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
                throw new TokenJwtException(JwtExceptionMessage);
            }
        }

        public string GenerateToken(User user, UserData userData, DateTime utcNow)
        {
            try
            {
                var tokenExpiration = utcNow.AddMinutes(s_tokenLifeTime);
                var tokenJwtId = Guid.NewGuid();
                var userRole = (int)userData.UserRole;

                var claims = new List<Claim>()
                {
                    new(JwtRegisteredClaimNames.Jti, tokenJwtId.ToString()),
                    new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new(JwtRegisteredClaimNames.Email, user.Email.ToString()),
                    new(JwtRegisteredClaimNames.Name, userData.Name),
                    new("UserRole", userRole.ToString()),
                    new(JwtRegisteredClaimNames.Amr, "pwd"),
                    new(JwtRegisteredClaimNames.AuthTime, utcNow.ToJToken().ToString())
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = s_issuer,
                    Audience = s_audienceWeb,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(s_secretKey)), SecurityAlgorithms.HmacSha256Signature),
                    NotBefore = utcNow,
                    Expires = tokenExpiration,
                    IssuedAt = utcNow,
                    TokenType = "JWT",
                    Subject = new ClaimsIdentity(claims),
                    AdditionalHeaderClaims = new Dictionary<string, object>() { { "fnc", "at" } }
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);

                return tokenHandler.WriteToken(securityToken);
            }
            catch (Exception e)
            {
                _logger.LogError("Erro ao gerar token JWT. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
                throw new Exception(e.Message);
            }
        }

        public string GenerateRefreshToken(User user, UserData userData, DateTime utcNow, string token)
        {
            try
            {
                var handler = new JsonWebTokenHandler();
                var tokenJwt = handler.ReadJsonWebToken(token);

                var refreshTokenExpiration = tokenJwt.ValidTo.AddMinutes(s_refreshTokenLifeTime);
                var refreshTokenJwtId = Guid.NewGuid();

                var claims = new List<Claim>()
        {
            new(JwtRegisteredClaimNames.Jti, refreshTokenJwtId.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email.ToString()),
            new(JwtRegisteredClaimNames.AuthTime, utcNow.ToJToken().ToString())
        };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = s_issuer,
                    Audience = s_audienceWeb,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(s_secretKey)), SecurityAlgorithms.HmacSha256Signature),
                    NotBefore = tokenJwt.ValidTo,
                    Expires = refreshTokenExpiration,
                    IssuedAt = utcNow,
                    TokenType = "JWT",
                    Subject = new ClaimsIdentity(claims),
                    AdditionalHeaderClaims = new Dictionary<string, object>() { { "fnc", "rt" } }
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);

                return tokenHandler.WriteToken(securityToken);
            }
            catch (Exception e)
            {
                _logger.LogError("Erro ao gerar refresh token JWT. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
                throw new Exception(e.Message);
            }
        }

        public async Task SaveRefreshTokenAsync(string token, string refreshToken, int userDataId)
        {
            try
            {
                UserData userData = await _context.UserData.FirstOrDefaultAsync(u => u.Id == userDataId);

                if (userData != null)
                {
                    var handler = new JsonWebTokenHandler();

                    var tokenJwt = handler.ReadJsonWebToken(token);
                    var refreshTokenJwt = handler.ReadJsonWebToken(refreshToken);

                    userData.TokenJwtId = Guid.TryParse(tokenJwt.Id, out Guid tokenJwtId) ? tokenJwtId : Guid.Empty;
                    userData.RefreshTokenJwtId = Guid.TryParse(refreshTokenJwt.Id, out Guid refreshTokenJwtId) ? refreshTokenJwtId : Guid.Empty;

                    _context.Entry(userData).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException e)
            {
                _logger.LogError("Erro ao salvar ID do refresh token JWT. Mensagem de erro: {Message}. Source: {Source}. InnerException: {InnerExceptionMessage}. StackTrace: {StackTrace}", e.Message, e.Source, e.InnerException.Message, e.StackTrace);
                throw new DbForeignKeyException(e.Source, e.InnerException.Message);
            }
            catch (Exception e)
            {
                _logger.LogError("Erro ao salvar ID do refresh token JWT. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
                throw new Exception(e.Message);
            }
        }

        public async Task DisconnectUserAsync(Guid tokenJwtId)
        {
            try
            {
                var userData = await _context.UserData.FirstOrDefaultAsync(ud => ud.TokenJwtId == tokenJwtId) ?? throw new NotFoundException($"Usuário não encontrado");

                userData.TokenJwtId = Guid.Empty;
                userData.RefreshTokenJwtId = Guid.Empty;

                _context.Entry(userData).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                _logger.LogError("Erro ao limpar ID do token e refresh token JWT. Mensagem de erro: {Message}. StackTrace: {StackTrace}. InnerException.Message: {InnerExceptionMessage}", e.Message, e.StackTrace, e.InnerException.Message);
                throw new DbForeignKeyException(e.Source, e.InnerException.Message);
            }
            catch (Exception e)
            {
                _logger.LogError("Erro ao limpar ID do token e refresh token JWT. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
                throw new Exception(e.Message);
            }
        }
    }
}
