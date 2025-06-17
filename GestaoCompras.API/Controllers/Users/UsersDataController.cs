using AutoMapper;
using GestaoCompras.API.Interfaces.Access;
using GestaoCompras.API.Interfaces.Users;
using GestaoCompras.API.Utils.ErrorHandler;
using GestaoCompras.DTO.Order;
using GestaoCompras.DTO.Users;
using GestaoCompras.Models.Access;
using GestaoCompras.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.Net;

namespace GestaoCompras.API.Controllers.Users;

[Route("api/v1/[controller]")]
[ApiController]
public class UsersDataController(IUserDataService userDataService, IUserService userService, ITokenService tokenService, ILogger<UsersDataController> logger) : Controller
{
    private readonly IUserDataService _userDataService = userDataService;
    private readonly IUserService _userService = userService;
    private readonly ITokenService _tokenService = tokenService;
    private readonly ILogger<UsersDataController> _logger = logger;

    [HttpPost]
    [Route("RegisterWeb")]
    [Authorize]
    public async Task<IActionResult> RegisterWebUserAsync([FromBody] RegisterUserPostDTO registerUserPostDTO)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            var userId = await _tokenService.ValidateTokenAsync(headerValue);

            if (await _userService.ExistsByEmailAsync(registerUserPostDTO.Email))
            {
                _logger.LogError("Erro ao tentar cadastrar usuário. Já existe um usuário cadastrado com o e-mail {Email}", registerUserPostDTO.Email);
                return BadRequest($"Já existe um usuário cadastrado com o e-mail {registerUserPostDTO.Email}");
            }

            if (registerUserPostDTO.PasswordHash != registerUserPostDTO.ConfirmPassword)
            {
                _logger.LogError("Erro ao tentar cadastrar usuário. As senhas informadas estão divergentes.");
                return BadRequest("As senhas devem ser iguais");
            }

            var registerUserDTO = registerUserPostDTO;
            var user = new User(registerUserDTO.Email, registerUserDTO.PasswordHash);

            var result = await _userService.CreateAsync(user);

            if (result.Succeeded)
            {
                var userData = new UserData()
                {
                    Name = registerUserDTO.Name,
                    UserId = user.Id
                };

                await _userDataService.InsertAsync(userData);
                _logger.LogInformation("Criação do {UserName}", user.UserName);

                return Ok();
            }
            else
            {
                await _userService.DeleteAsync(user);

                string errorReturn = string.Empty;

                foreach (var error in result.Errors)
                    errorReturn += $"{error.Code};";

                _logger.LogError("Erro ao tentar cadastrar usuário: {ErrorReturn}", errorReturn);
                return BadRequest(errorReturn);
            }
        }
        catch (TokenJwtException e)
        {
            _logger.LogError("Erro ao validar token JWT. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Unauthorized(e.Message);
        }
        catch (NotFoundException e)
        {
            _logger.LogError("Falha na tentativa de cadastrar o usuário {UserName}. Mensagem de erro: {Message}", registerUserPostDTO.Email, e.Message);
            return NotFound(e.Message);
        }
        catch (DbUpdateConcurrencyException e)
        {
            _logger.LogError("Falha na tentativa de cadastrar o usuário {UserName}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", registerUserPostDTO.Email, e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception e)
        {
            _logger.LogError("Falha na tentativa de cadastrar o usuário {UserName}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", registerUserPostDTO.Email, e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }
}
