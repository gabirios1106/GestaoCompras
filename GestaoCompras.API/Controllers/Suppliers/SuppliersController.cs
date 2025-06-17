using AutoMapper;
using GestaoCompras.API.Interfaces.Access;
using GestaoCompras.API.Interfaces.Suppliers;
using GestaoCompras.API.Interfaces.Users;
using GestaoCompras.API.Utils.ErrorHandler;
using GestaoCompras.DTO.Supplier;
using GestaoCompras.Models.Suppliers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.Net;

namespace GestaoCompras.API.Controllers.Suppliers;

[Route("api/v1/[controller]")]
[ApiController]
public class SuppliersController(ISupplierService supplierService, IMapper mapper, ITokenService tokenService, IUserDataService userDataService, ILogger<SuppliersController> logger) : Controller
{
    private readonly ISupplierService _supplierService = supplierService;
    private readonly IMapper _mapper = mapper;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IUserDataService _userDataService = userDataService;
    private readonly ILogger<SuppliersController> _logger = logger;

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<SupplierGetDTO>>> GetSuppliersAsync([FromQuery] string orderBy)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            Guid userId = await _tokenService.ValidateTokenAsync(headerValue);

            _logger.LogInformation("Consulta de fornecedors feita pelo usuário {UserId}", userId);
            var suppliersGetDTO = await _supplierService.GetSuppliersAsync(orderBy);

            return Ok(suppliersGetDTO);
        }
        catch (TokenJwtException e)
        {
            _logger.LogError("Erro ao validar token JWT. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Unauthorized(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Falha ao consultar fornecedors. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpGet]
    [Route("GetSupplierById/{supplierId:int}")]
    [Authorize]
    public async Task<ActionResult<SupplierGetDTO>> GetSupplierByIdAsync([FromRoute] int supplierId)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            Guid userId = await _tokenService.ValidateTokenAsync(headerValue);

            _logger.LogInformation("Consulta da fornecedor {SupplierId} feita pelo usuário {UserId}", supplierId, userId);
            var supplierGetDTO = await _supplierService.GetSupplierByIdAsync<SupplierGetDTO>(supplierId);

            return Ok(supplierGetDTO);
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
            _logger.LogError("Falha ao consultar fornecedor. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<SupplierGetDTO>> PostSupplierAsync([FromBody] SupplierPostDTO supplierPostDTO)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            Guid userId = await _tokenService.ValidateTokenAsync(headerValue);

            supplierPostDTO.FormatName();
            var nameExists = await _supplierService.ExistsByNameAsync(supplierPostDTO.Name);

            if (nameExists)
            {
                _logger.LogInformation("Erro ao tentar cadastrar novo fornecedor. Já existe uma fornecedor com o nome {Name}", supplierPostDTO.Name);
                return BadRequest($"Já existe um fornecedor com o nome {supplierPostDTO.Name}");
            }

            var userDataId = await _userDataService.GetUserDataIdAsync(userId);

            supplierPostDTO.SetUserDataId(userDataId);

            var supplier = _mapper.Map<Supplier>(supplierPostDTO);

            await _supplierService.InsertAsync(supplier);
            _logger.LogInformation("Cadastrado o novo fornecedor {Name}", supplier.Name);

            var supplierGetDTO = _mapper.Map<SupplierGetDTO>(supplier);

            return CreatedAtAction("GetSupplierById", new { supplierId = supplierGetDTO.Id }, supplierGetDTO);
        }
        catch (TokenJwtException e)
        {
            _logger.LogError("Erro ao validar token JWT. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Unauthorized(e.Message);
        }
        catch (DbUpdateException e)
        {
            _logger.LogError("Falha ao incluir novo fornecedor. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception e)
        {
            _logger.LogError("Falha ao incluir novo fornecedor. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }
}
