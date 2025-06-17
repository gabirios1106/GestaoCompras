using AutoMapper;
using GestaoCompras.API.Interfaces.Access;
using GestaoCompras.API.Interfaces.Stores;
using GestaoCompras.API.Interfaces.Users;
using GestaoCompras.API.Utils.ErrorHandler;
using GestaoCompras.DTO.Store;
using GestaoCompras.Models.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.Net;

namespace GestaoCompras.API.Controllers.Stores;

[Route("api/v1/[controller]")]
[ApiController]
public class StoresController(IStoreService storeService, IMapper mapper, ITokenService tokenService, IUserDataService userDataService, ILogger<StoresController> logger) : Controller
{
    private readonly IStoreService _storeService = storeService;
    private readonly IMapper _mapper = mapper;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IUserDataService _userDataService = userDataService;
    private readonly ILogger<StoresController> _logger = logger;

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<StoreGetDTO>>> GetStoresAsync([FromQuery] string orderBy, [FromQuery] bool showInactive = false)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            Guid userId = await _tokenService.ValidateTokenAsync(headerValue);

            _logger.LogInformation("Consulta de frutas feita pelo usuário {UserId}", userId);
            var StoresGetDTO = await _storeService.GetStoresAsync(orderBy, showInactive);

            return Ok(StoresGetDTO);
        }
        catch (TokenJwtException e)
        {
            _logger.LogError("Erro ao validar token JWT. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Unauthorized(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Falha ao consultar frutas. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpGet]
    [Route("GetStoreById/{StoreId:int}")]
    [Authorize]
    public async Task<ActionResult<StoreGetDTO>> GetStoreByIdAsync([FromRoute] int StoreId)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            Guid userId = await _tokenService.ValidateTokenAsync(headerValue);

            _logger.LogInformation("Consulta da fruta {StoreId} feita pelo usuário {UserId}", StoreId, userId);
            var StoreGetDTO = await _storeService.GetStoreByIdAsync<StoreGetDTO>(StoreId);

            return Ok(StoreGetDTO);
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
            _logger.LogError("Falha ao consultar fruta. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<StoreGetDTO>> PostStoreAsync([FromBody] StorePostDTO StorePostDTO)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            Guid userId = await _tokenService.ValidateTokenAsync(headerValue);

            var userDataId = await _userDataService.GetUserDataIdAsync(userId);

            StorePostDTO.SetUserDataId(userDataId);

            var Store = _mapper.Map<Store>(StorePostDTO);

            await _storeService.InsertAsync(Store);
            _logger.LogInformation("Cadastrada a nova fruta {Name}", Store.Name);

            var StoreGetDTO = _mapper.Map<StoreGetDTO>(Store);

            return CreatedAtAction("GetStoreById", new { StoreId = StoreGetDTO.Id }, StoreGetDTO);
        }
        catch (TokenJwtException e)
        {
            _logger.LogError("Erro ao validar token JWT. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Unauthorized(e.Message);
        }
        catch (DbUpdateException e)
        {
            _logger.LogError("Falha ao incluir nova fruta. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception e)
        {
            _logger.LogError("Falha ao incluir nova fruta. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> PutStoreAsync([FromBody] StorePutDTO StorePutDTO)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            Guid userId = await _tokenService.ValidateTokenAsync(headerValue);

            Store Store = await _storeService.GetStoreByIdAsync<Store>(StorePutDTO.Id);

            //Store.PrepareForUpdate(StorePutDTO.Price);

            await _storeService.UpdateAsync(Store);
            _logger.LogInformation("Realizada a atualização do preço da fruta {Name}", Store.Name);

            return NoContent();
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
        catch (DbConcurrencyException e)
        {
            _logger.LogError("Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception e)
        {
            _logger.LogError("Falha ao ativar fruta. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }
}
