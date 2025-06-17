using AutoMapper;
using GestaoCompras.API.Interfaces.Access;
using GestaoCompras.API.Interfaces.Fruits;
using GestaoCompras.API.Interfaces.Orders;
using GestaoCompras.API.Interfaces.Users;
using GestaoCompras.API.Utils.ErrorHandler;
using GestaoCompras.DTO.Fruit;
using GestaoCompras.DTO.Order;
using GestaoCompras.Enums.Orders;
using GestaoCompras.Models.Fruits;
using GestaoCompras.Models.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.Net;


namespace GestaoCompras.API.Controllers.Orders;

[Route("api/v1/[controller]")]
[ApiController]
public class OrdersController(IOrderService orderService, IFruitService fruitService, IMapper mapper, ITokenService tokenService, IUserDataService userDataService, ILogger<OrdersController> logger) : Controller
{
    private readonly IOrderService _orderService = orderService;
    private readonly IMapper _mapper = mapper;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IUserDataService _userDataService = userDataService;
    private readonly ILogger<OrdersController> _logger = logger;
    private readonly IFruitService _fruitService = fruitService;    

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<OrderGetDTO>>> GetOrdersAsync([FromQuery] string orderId, [FromQuery] string orderBy, [FromQuery] int skip, [FromQuery] int take, [FromQuery] bool showInactive = false, [FromQuery] bool ticket = false, [FromQuery] bool employee = false, [FromQuery] string searchValue = null, [FromQuery] DateTime? initialDate = null, [FromQuery] DateTime? finalDate = null)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            Guid userId = await _tokenService.ValidateTokenAsync(headerValue);

            _logger.LogInformation("Consulta de pedidos feita pelo usuário {UserId}", userId);
            var ordersGetDTO = await _orderService.GetOrdersAsync(orderBy, skip, take, showInactive, ticket, employee, searchValue, orderId, initialDate, finalDate);

            return Ok(ordersGetDTO);
        }
        catch (TokenJwtException e)
        {
            _logger.LogError("Erro ao validar token JWT. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Unauthorized(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Falha ao consultar pedidos. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpGet]
    [Route("GetAllToPay")]
    [Authorize]
    //public async Task<double> GetAllToPayAsync() => await _orderService.GetAllToPayAsync();

    [HttpGet]   
    [Route("GetOrderById/{orderId:int}")]
    [Authorize]
    public async Task<ActionResult<OrderGetDTO>> GetOrderByIdAsync([FromRoute] int orderId)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            Guid userId = await _tokenService.ValidateTokenAsync(headerValue);

            _logger.LogInformation("Consulta de pedido {OrderId} feita pelo usuário {UserId}", orderId, userId);
            var orderGetDTO = await _orderService.GetOrderByIdAsync<OrderGetDTO>(orderId);

            return Ok(orderGetDTO);
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

    [Route("GetActiveWeeks")]
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<ActiveWeekGetDTO>>> GetActiveWeeksAsync()
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            Guid userId = await _tokenService.ValidateTokenAsync(headerValue);

            _logger.LogInformation("Consulta de Semanas ativas feita pelo usuário {UserId}", userId);
            var activeWeeksGetDTO = await _orderService.GetActiveWeeksAsync();

            return Ok(activeWeeksGetDTO);
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
        catch (Exception e)
        {
            _logger.LogError("Falha ao consultar semanas. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<OrderGetDTO>> PostOrderAsync([FromBody] OrderPostDTO orderPostDTO)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            Guid userId = await _tokenService.ValidateTokenAsync(headerValue);

            var userDataId = await _userDataService.GetUserDataIdAsync(userId);

            orderPostDTO.SetUserDataId(userDataId);

            await _orderService.InsertAsync(orderPostDTO);

            _logger.LogInformation("Novo pedido cadastrado");

            await UpdateFruitPrice(orderPostDTO.FruitId, orderPostDTO.UnitPrice);

            return Ok();
        }
        catch (TokenJwtException e)
        {
            _logger.LogError("Erro ao validar token JWT. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Unauthorized(e.Message);
        }
        catch (DbUpdateException e)
        {
            _logger.LogError("Falha ao incluir novo pedido. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception e)
        {
            _logger.LogError("Falha ao incluir novo pedido. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> PutOrderAsync([FromBody] OrderPutDTO orderPutDTO)
    {
        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            Guid userId = await _tokenService.ValidateTokenAsync(headerValue);

            Order order = await _orderService.GetOrderByIdAsync<Order>(orderPutDTO.Id);

            order.PrepareForUpdate(orderPutDTO.StatusOrder, orderPutDTO.FruitId, orderPutDTO.SupplierId, orderPutDTO.BackLoad, orderPutDTO.MiddleLoad, orderPutDTO.FrontLoad, orderPutDTO.TotalLoad, orderPutDTO.UnitPrice, orderPutDTO.TotalPrice, orderPutDTO.WasAlreadyTicket, orderPutDTO.PaymentDay, orderPutDTO.Observation);

            await _orderService.UpdateAsync(order); 
            _logger.LogInformation("Realizada a atualização do pedido (ID:{Id})", order.Id.ToString("D6"));

            await UpdateFruitPrice(order.FruitId, order.UnitPrice);

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

    [HttpDelete]
    [Route("{orderId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteOrderAsync([FromRoute] int orderId)
    {
        Order order = new();

        try
        {
            Request.Headers.TryGetValue("Authorization", out StringValues headerValue);
            Guid userId = await _tokenService.ValidateTokenAsync(headerValue);

            order = await _orderService.GetOrderByIdAsync<Order>(orderId);

            if (order.StatusOrder == StatusOrder.PAGO)
            {
                _logger.LogInformation("Erro ao tentar excluir pedido (ID:{Id}). Não é possivel excluir um pedido que já foi pago", order.Id);
                return BadRequest("Não é possivel excluir um pedido que já foi pago");
            }

            await _orderService.DeleteAsync(order);
            _logger.LogInformation("Exclusão do pedido {Id} ", order.Id);
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
            _logger.LogError("Falha ao excluir pedido. Mensagem de erro: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
            return Problem(detail: e.StackTrace, title: e.Message, statusCode: (int)HttpStatusCode.InternalServerError);
        }

        return NoContent();
    }

    private async Task UpdateFruitPrice(int fruitId, double unitPrice)
    {
        Fruit fruit = await _fruitService.GetFruitByIdAsync<Fruit>(fruitId);
  
        if (fruit != null)
        {
            fruit.Price = unitPrice;
            await _fruitService.UpdateAsync(fruit);
        }
    }
}
