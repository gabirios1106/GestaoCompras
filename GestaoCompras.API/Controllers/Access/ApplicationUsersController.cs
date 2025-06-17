using AutoMapper;
using GestaoCompras.API.Interfaces.Access;
using GestaoCompras.API.Utils.ErrorHandler;
using GestaoCompras.DTO.Access;
using GestaoCompras.Enums.General;
using GestaoCompras.Models.Access;
using GestaoCompras.Utils.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.Net;

namespace GestaoCompras.API.Controllers.Access;

[Route("api/v1/[controller]")]
[ApiController]
public class ApplicationUsersController(IApplicationUserService applicationUserService, IMapper mapper, ITokenService tokenService, ILogger<ApplicationUsersController> logger) : Controller
{
    private readonly IApplicationUserService _applicationUserService = applicationUserService;
    private readonly IMapper _mapper = mapper;
    private readonly ITokenService _tokenService = tokenService;
    private readonly ILogger<ApplicationUsersController> _logger = logger;

    [HttpGet]
    [Route("GetAplicationUserById/{appId}")]
    [Authorize]
    public async Task<ActionResult<ApplicationUserGetDTO>> GetAplicationUserByIdAsync([FromRoute] Guid appId, [FromQuery] bool showInactive)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            var userId = await _tokenService.ValidateTokenAsync(headerValue);

            _logger.LogInformation("Consulta ao usuário de aplicação {AppId}", appId);

            var applicationUserGetDTO = await _applicationUserService.GetApplicationUserByAppIdAsync<ApplicationUserGetDTO>(appId, showInactive);

            if (applicationUserGetDTO == null)
                return NotFound("Nenhum registro encontrado");

            return Ok(applicationUserGetDTO);
        }
        catch (TokenJwtException e)
        {
            _logger.LogError("Erro ao validar token JWT. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Unauthorized(e.Message);
        }
        catch (NotFoundException e)
        {
            _logger.LogError("Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return NotFound(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError("Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception e)
        {
            _logger.LogError("Falha ao consultar o usuário de aplicação {AppId}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", appId, e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApplicationUserGetDTO>> PostApplicationUserAsync([FromBody] ApplicationUserPostDTO applicationUserPostDTO)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            var userId = await _tokenService.ValidateTokenAsync(headerValue);

            var exists = false;

            exists = await _applicationUserService.ExistsByNameAsync(applicationUserPostDTO.AppName);

            if (exists)
            {
                _logger.LogInformation("Erro ao criar usuário de aplicação. Já existe um usuário de aplicação chamado {AppName}", applicationUserPostDTO.AppName);
                return BadRequest($"Já existe um usuário de aplicação chamado {applicationUserPostDTO.AppName}");
            }

            var appPassword = Guid.NewGuid();

            var applicationUser = _mapper.Map<ApplicationUser>(applicationUserPostDTO);
            applicationUser.AppPasswordHash = appPassword.ToString();

            await _applicationUserService.CreateAsync(applicationUser);

            _logger.LogInformation("Cadastro do usuário de aplicação -> #{AppId} - {AppName}", applicationUser.AppId, applicationUser.AppName);

            var applicationUserGetDTO = _mapper.Map<ApplicationUserGetDTO>(applicationUser);
            applicationUserGetDTO.AppPasswordHash = appPassword.ToString();

            return CreatedAtAction("GetAplicationUserById", new { appId = applicationUserGetDTO.AppId }, applicationUserGetDTO);
        }
        catch (TokenJwtException e)
        {
            _logger.LogError("Erro ao validar token JWT. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Unauthorized(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Falha na tentativa de cadastrar o usuário de aplicação {AppName}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", applicationUserPostDTO.AppName, e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPut]
    [Route("ChangeApplicationPassword/{appId}")]
    [Authorize]
    public async Task<IActionResult> PutApplicationUserAsync([FromRoute] Guid appId)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            var userId = await _tokenService.ValidateTokenAsync(headerValue);

            var applicationUser = await _applicationUserService.GetApplicationUserByAppIdAsync<ApplicationUser>(appId);

            applicationUser.AppPasswordHash = Guid.NewGuid().ToString();

            string appName = applicationUser.AppName;

            await _applicationUserService.UpdateAsync(applicationUser);
            _logger.LogInformation("Alteração de senha do usuário de aplicação -> #{AppId} - {AppName}", appId, appName);

            return NoContent();
        }
        catch (TokenJwtException e)
        {
            _logger.LogError("Erro ao validar token JWT. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Unauthorized(e.Message);
        }
        catch (NotFoundException e)
        {
            _logger.LogError("Falha na tentativa de alterar a senha do usuário de aplicação {AppId}. O usuário não existe", appId);
            return NotFound(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError("Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
        catch (DbUpdateConcurrencyException e)
        {
            _logger.LogError("Falha na tentativa de alterar a senha do usuário de aplicação {AppId}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", appId, e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception e)
        {
            _logger.LogError("Falha na tentativa de alterar a senha do usuário de aplicação {AppId}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", appId, e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpDelete]
    [Route("{appId}")]
    [Authorize]
    public async Task<IActionResult> DeleteApplicationUserAsync(Guid appId)
    {
        var applicationUser = new ApplicationUser();
        var userId = Guid.Empty;

        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            userId = await _tokenService.ValidateTokenAsync(headerValue);

            applicationUser = await _applicationUserService.GetApplicationUserByAppIdAsync<ApplicationUser>(appId);

            await _applicationUserService.DeleteAsync(applicationUser);
            _logger.LogInformation("Exclusão do usuário de aplicação -> #{UserId} - {AppName}", userId, applicationUser.AppName);

            return NoContent();
        }
        catch (TokenJwtException e)
        {
            _logger.LogError("Erro ao validar token JWT. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Unauthorized(e.Message);
        }
        catch (NotFoundException e)
        {
            _logger.LogError("Falha na tentativa de alterar a senha do usuário de aplicação {AppId}. O usuário não existe", appId);
            return NotFound(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogError("Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
        catch (DbConcurrencyException e)
        {
            _logger.LogError("Falha na tentativa de excluir o usuário de aplicação {AppId}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", appId, e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
        catch (DbUpdateException e)
        {
            if (CustomValidator.IsFkException(e.InnerException.InnerException.ToString()))
            {
                applicationUser.Status = Status.INATIVO;
                await _applicationUserService.UpdateAsync(applicationUser);
                _logger.LogInformation("Desativação do usuário de aplicação -> #{UserId} - {AppName}", userId, applicationUser.AppName);

                return NoContent();
            }
            else
            {
                _logger.LogError("Falha na tentativa de excluir o usuário de aplicação {AppId}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", appId, e.Message, e.StackTrace);
                return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Falha na tentativa de excluir o usuário de aplicação {AppId}. Mensagem de erro: {Message}. StackTrace: {StackTrace}", appId, e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }
}
