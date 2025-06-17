using AutoMapper;
using GestaoCompras.API.Interfaces.Access;
using GestaoCompras.API.Interfaces.Fruits;
using GestaoCompras.API.Interfaces.Users;
using GestaoCompras.API.Utils.ErrorHandler;
using GestaoCompras.DTO.Fruit;
using GestaoCompras.Models.Fruits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.Net;

namespace GestaoCompras.API.Controllers.Fruits;

[Route("api/v1/[controller]")]
[ApiController]
public class FruitsController(IFruitService fruitService, IMapper mapper, ITokenService tokenService, IUserDataService userDataService, ILogger<FruitsController> logger) : Controller
{
    private readonly IFruitService _fruitService = fruitService;
    private readonly IMapper _mapper = mapper;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IUserDataService _userDataService = userDataService;
    private readonly ILogger<FruitsController> _logger = logger;

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<FruitGetDTO>>> GetFruitsAsync([FromQuery] string orderBy)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            Guid userId = await _tokenService.ValidateTokenAsync(headerValue);

            _logger.LogInformation("Consulta de frutas feita pelo usuário {UserId}", userId);
            var fruitsGetDTO = await _fruitService.GetFruitsAsync(orderBy);

            return Ok(fruitsGetDTO);
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
    [Route("GetFruitById/{fruitId:int}")]
    [Authorize]
    public async Task<ActionResult<FruitGetDTO>> GetFruitByIdAsync([FromRoute] int fruitId)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            Guid userId = await _tokenService.ValidateTokenAsync(headerValue);

            _logger.LogInformation("Consulta da fruta {FruitId} feita pelo usuário {UserId}", fruitId, userId);
            var fruitGetDTO = await _fruitService.GetFruitByIdAsync<FruitGetDTO>(fruitId);

            return Ok(fruitGetDTO);
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
    public async Task<ActionResult<FruitGetDTO>> PostFruitAsync([FromBody] FruitPostDTO fruitPostDTO)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            Guid userId = await _tokenService.ValidateTokenAsync(headerValue);

            fruitPostDTO.FormatName();
            var nameExists = await _fruitService.ExistsByNameAsync(fruitPostDTO.Name);

            if (nameExists)
            {
                _logger.LogInformation("Erro ao tentar cadastrar nova fruta. Já existe uma fruta com o nome {Name}", fruitPostDTO.Name);
                return BadRequest($"Já existe uma fruta com o nome {fruitPostDTO.Name}");
            }

            var userDataId = await _userDataService.GetUserDataIdAsync(userId);

            fruitPostDTO.SetUserDataId(userDataId);

            var fruit = _mapper.Map<Fruit>(fruitPostDTO);

            await _fruitService.InsertAsync(fruit);
            _logger.LogInformation("Cadastrada a nova fruta {Name}", fruit.Name);

            var fruitGetDTO = _mapper.Map<FruitGetDTO>(fruit);

            return CreatedAtAction("GetFruitById", new { fruitId = fruitGetDTO.Id }, fruitGetDTO);
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
    public async Task<IActionResult> PutFruitAsync([FromBody] FruitPutDTO fruitPutDTO)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            Guid userId = await _tokenService.ValidateTokenAsync(headerValue);

            Fruit fruit = await _fruitService.GetFruitByIdAsync<Fruit>(fruitPutDTO.Id);

            fruit.PrepareForUpdate(fruitPutDTO.Price);

            await _fruitService.UpdateAsync(fruit);
            _logger.LogInformation("Realizada a atualização do preço da fruta {Name}", fruit.Name);

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

