using AutoMapper;
using GestaoCompras.API.Interfaces.Access;
using GestaoCompras.API.Interfaces.Users;
using GestaoCompras.API.Utils.ErrorHandler;
using GestaoCompras.DTO.Access;
using GestaoCompras.Models.Access;
using GestaoCompras.Models.Users;
using GestaoCompras.Utils.Classes;
using GestaoCompras.Utils.Converters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace GestaoCompras.API.Controllers.Access;

[Route("api/v1/[controller]")]
[ApiController]
public class AccessController(IUserService userService, IUserDataService userDataService, IApplicationUserService applicationUserService, ITokenService tokenService, IConfiguration configuration, IMapper mapper, IGoogleReCaptchaService googleReCaptchaService, ILogger<AccessController> logger) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly IUserDataService _userDataService = userDataService;
    private readonly IApplicationUserService _applicationUserService = applicationUserService;
    private readonly IGoogleReCaptchaService _googleReCaptchaService = googleReCaptchaService;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IConfiguration _configuration = configuration;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<AccessController> _logger = logger;

    [HttpPost]
    [Route("UserLogin")]
    [AllowAnonymous]
    public async Task<ActionResult<dynamic>> UserLoginAsync([FromBody] UserLoginPostDTO userLoginPostDTO)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            var appUserAuthenticated = await _applicationUserService.VerifyHashedPassword(headerValue);

            if (!appUserAuthenticated)
            {
                _logger.LogWarning("Acesso de aplicação negado. HeaderValue: {HeaderValue}", headerValue);
                return Unauthorized("Não autorizado");
            }

            var googleReCaptchaScore = 1.0;

            if (userLoginPostDTO == null)
            {
                _logger.LogInformation("Solicitação de login inválida. Corpo da solicitação nulo");
                return BadRequest("Dados inválidos");
            }

            var user = await _userService.FindAsync<User>(userLoginPostDTO.UserName);

            if (user == null)
            {
                _logger.LogInformation("Login não autorizado. O usuário {UserName} não existe", userLoginPostDTO.UserName);
                return BadRequest("Usuário e/ou senha inválidos");
            }

            #region GoogleReCaptcha
            if (userLoginPostDTO.ReCaptchaToken != "ByPass")
            {
                var secretKey = _configuration.GetValue<string>("GoogleReCaptcha:SecretKey");
                var googleReCaptchaResponse = new GoogleReCaptchaResponse()
                {
                    score = 1,
                    success = true
                };

                googleReCaptchaResponse = await _googleReCaptchaService.GetGoogleReCaptchaVerify("siteverify", userLoginPostDTO.ReCaptchaToken, secretKey);

                if (googleReCaptchaResponse != null)
                {
                    googleReCaptchaScore = googleReCaptchaResponse.score;

                    if (!googleReCaptchaResponse.success)
                    {
                        _logger.LogWarning("Login não autorizado para o usuário {UserName}. Verificação do GoogleReCaptcha reprovada. GoogleReCaptchaScore: {GoogleReCaptchaScore}", userLoginPostDTO.UserName, googleReCaptchaResponse.score);
                        return Unauthorized("Problemas na verificação da autenticação. Por favor, tente novamente. Caso o problema persista entre em contato com o administrador do sistema");
                    }

                    if (googleReCaptchaResponse.score < 0.5)
                    {
                        _logger.LogWarning("Login não autorizado para o usuário {UserName}. Score menor que 0,5. GoogleReCaptchaScore: {GoogleReCaptchaScore}", userLoginPostDTO.UserName, googleReCaptchaResponse.score);
                        return Unauthorized("Acesso Negado");
                    }
                }
            }
            else
            {
                _logger.LogInformation("Aplicado ByPass no fluxo de verificação do Google ReCaptcha");
            }
            #endregion GoogleReCaptcha

            var passwordHasher = new PasswordHasher<User>();

            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userLoginPostDTO.PasswordHash) == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Login não autorizado para o usuário {UserName}. Senha incorreta", userLoginPostDTO.UserName);
                return Unauthorized("Usuário e/ou senha inválidos");
            }

            var userData = await _userDataService.FindByUserIdAsync<UserData>(user.Id);

            if (userData == null)
            {
                _logger.LogWarning("Login não autorizado para o usuário {UserName}. Não existem dados do usuário na tabela UserData", userLoginPostDTO.UserName);
                return BadRequest("Não foi possível concluir o login");
            }

            var tokenJwt = string.Empty;
            var refreshTokenJwt = string.Empty;
            var authenticatorUri = string.Empty;
            var authenticatorKey = string.Empty;

            var utcNow = DateTime.UtcNow;

            tokenJwt = _tokenService.GenerateToken(user, userData, utcNow);
            refreshTokenJwt = _tokenService.GenerateRefreshToken(user, userData, utcNow, tokenJwt);

            await _tokenService.SaveRefreshTokenAsync(tokenJwt, refreshTokenJwt, userData.Id);

            _logger.LogInformation("Login realizado pelo usuário {UserName}. Score Google ReCaptcha: {GoogleReCaptchaScore}", userLoginPostDTO.UserName, googleReCaptchaScore);

            var authenticatedUserDTO = new AuthenticatedUserGetDTO()
            {
                Token = tokenJwt,
                RefreshToken = refreshTokenJwt,
                UserDataId = userData.Id
            };

            return Ok(authenticatedUserDTO);
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("Login não autorizado para o usuário {UserName}. O status do usuário não existe", userLoginPostDTO.UserName);
            return Unauthorized("Usuário e/ou senha inválidos");
        }
        catch (ArgumentException e)
        {
            _logger.LogError("Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
        catch (DbUpdateConcurrencyException e)
        {
            _logger.LogError("Falha na tentativa de login do usuário {UserName}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", userLoginPostDTO.UserName, e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception e)
        {
            _logger.LogError("Falha na tentativa de login do usuário {UserName}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", userLoginPostDTO.UserName, e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPost]
    [Route("RefreshToken")]
    [AllowAnonymous]
    public async Task<ActionResult<dynamic>> RefreshTokenAsync([FromBody] RefreshTokenGetDTO refreshTokenGetDTO)
    {

        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            var appUserAuthenticated = await _applicationUserService.VerifyHashedPassword(headerValue);

            if (!appUserAuthenticated)
            {
                _logger.LogWarning("Acesso de aplicação negado. HeaderValue: {HeaderValue}", headerValue);
                return Unauthorized("Não autorizado");
            }

            if (refreshTokenGetDTO == null)
            {
                _logger.LogInformation("Solicitação de refresh token inválida. Corpo da solicitação nulo");
                return Unauthorized("Token expirado");
            }

            var jsonWebRefreshToken = await _tokenService.ValidateRefreshTokenAsync(refreshTokenGetDTO.RefreshToken);

            if (jsonWebRefreshToken == null)
            {
                _logger.LogInformation("Solicitação de refresh token inválida. Refresh token não enviado");
                return Unauthorized("Token expirado");
            }

            var userName = jsonWebRefreshToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email).Value;
            var userId = Guid.TryParse(jsonWebRefreshToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub).Value, out Guid subject) ? subject : Guid.Empty;

            var user = await _userService.FindAsync<User>(userName);

            if (user == null)
            {
                _logger.LogWarning("Dados de usuário não localizados nas credenciais de autorização");
                return Unauthorized("Token expirado");
            }

            var userData = await _userDataService.FindByUserIdAsync<UserData>(user.Id);

            if (userData == null)
            {
                _logger.LogWarning("Login não autorizado para o usuário {UserName}. Não existem dados do usuário na tabela UserData", userName);
                return Unauthorized("Dados inválidos");
            }

            if (refreshTokenGetDTO.Revalidate)
            {
                var passwordHasher = new PasswordHasher<User>();

                if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, refreshTokenGetDTO.PasswordHash) == PasswordVerificationResult.Failed)
                {
                    _logger.LogWarning("Refresh token não autorizado para o usuário {UserName}. Senha incorreta", userName);
                    return Unauthorized("Usuário e/ou senha inválidos");
                }
            }

            var utcNow = DateTime.UtcNow;

            var tokenJwt = _tokenService.GenerateToken(user, userData, utcNow);
            var refreshTokenJwt = _tokenService.GenerateRefreshToken(user, userData, utcNow, tokenJwt);

            await _tokenService.SaveRefreshTokenAsync(tokenJwt, refreshTokenJwt, userData.Id);

            _logger.LogInformation("Atualização de token para o usuário {UserId}", userId);

            var refreshedTokenDTO = new RefreshTokenGetDTO("", tokenJwt, refreshTokenJwt);

            return Ok(refreshedTokenDTO);
        }
        catch (TokenJwtException e)
        {
            _logger.LogError("Falha na tentativa de atualização de token. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Unauthorized(e.Message);
        }
        catch (NotFoundException e)
        {
            _logger.LogError("Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Unauthorized("Usuário e/ou senha inválidos");
        }
        catch (ArgumentException e)
        {
            _logger.LogError("Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception e)
        {
            _logger.LogError("Falha na tentativa de atualização de token. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPost]
    [Route("UserLogout")]
    [AllowAnonymous]
    public async Task<IActionResult> UserLogoutAsync()
    {
        Guid userId = Guid.Empty;
        Guid tokenJwtId = Guid.Empty;

        try
        {
            var authorization = Request.Headers.TryGetValue("Authorization", out StringValues headerValue) ? headerValue.ToString().Replace("Bearer ", string.Empty) : string.Empty;

            if (string.IsNullOrEmpty(authorization))
            {
                _logger.LogWarning("Logout de usuário solicitado sem credenciais de autenticação");
                return BadRequest("Solicitação inválida. Credenciais de autorização ausentes.");
            }

            var claimsIdentity = GetClaimsIdentity(authorization);

            if (claimsIdentity.Claims.Count() < 2)
            {
                _logger.LogWarning("Não foi possível identificar os dados da sessão nas credenciais de autenticação");
                return BadRequest("Solicitação inválida. Dados da sessão não localizados nas credenciais de autenticação.");
            }

            userId = Guid.TryParse(claimsIdentity.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub).Value, out Guid subject) ? subject : Guid.Empty;

            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Dados de usuário não localizados nas credenciais de autorização");
                return BadRequest("Solicitação inválida. Dados de usuário não localizados nas credenciais de autorização");
            }

            tokenJwtId = Guid.TryParse(claimsIdentity.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti).Value, out Guid jit) ? jit : Guid.Empty;

            if (tokenJwtId == Guid.Empty)
            {
                _logger.LogWarning("Dados de token não localizados nas credenciais de autorização");
                return BadRequest("Solicitação inválida. Dados de token não localizados nas credenciais de autorização");
            }

            await _tokenService.DisconnectUserAsync(tokenJwtId);
            _logger.LogInformation("Realizado o logou do usuário {UserId}", userId);

            return Ok($"O usuário foi {userId} desconectado!");
        }
        catch (NotFoundException e)
        {
            _logger.LogError("Falha na tentativa de logout do usuário {UserId}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", userId, e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
        catch (DbUpdateException e)
        {
            _logger.LogError("Falha na tentativa de logout do usuário {UserId}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", userId, e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception e)
        {
            _logger.LogError("Falha na tentativa de logout do usuário {UserId}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", userId, e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }

    private static ClaimsIdentity GetClaimsIdentity(string token)
    {
        var claims = CustomConverter.ParseClaimsFromJWT(token);

        var claimsIdentity = new ClaimsIdentity("JWT");

        foreach (var claim in claims)
        {
            if (claim.Type == JwtRegisteredClaimNames.Sub || claim.Type == JwtRegisteredClaimNames.Jti)
            {
                var customClaim = new Claim(claim.Type, claim.Value);
                claimsIdentity.AddClaim(customClaim);
            }
        }

        return claimsIdentity;
    }
}
