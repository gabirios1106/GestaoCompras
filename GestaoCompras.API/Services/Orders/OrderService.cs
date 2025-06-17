using GestaoCompras.API.Data;
using GestaoCompras.API.Interfaces.Orders;
using GestaoCompras.API.Utils.ErrorHandler;
using GestaoCompras.DTO.Common;
using GestaoCompras.DTO.Order;
using GestaoCompras.Enums.Orders;
using GestaoCompras.Models.Orders;
using GestaoCompras.Models.Stores;
using GestaoCompras.Utils.Converters;
using GestaoCompras.Utils.Extentions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GestaoCompras.API.Services.Orders
{
    public class OrderService(ApplicationDbContext context, ILogger<OrderService> logger) : IOrderService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<OrderService> _logger = logger;
        private readonly CultureInfo _ptBR = new("pt-BR");

        public async Task<GetWithPaginationGetDTO<OrderGetDTO>> GetOrdersAsync(string orderBy, int skip, int take, bool showInactive = false, bool ticket = false, bool employee = false, string searchValue = null, string orderId = null, DateTime? initialDate = null, DateTime? finalDate = null)
        {
            var queryable = _context.Order
                                    .AsNoTracking()
                                    .Include(o => o.Fruit).AsSplitQuery()
                                    .Include(o => o.Supplier).AsSplitQuery()
                                    .Include(o => o.UserData).AsSplitQuery()
                                    .Include(o => o.Store).AsSplitQuery();

            if (!employee)
            {
                if (!showInactive && !ticket)
                {
                    queryable = queryable.Where(o => o.StatusOrder == StatusOrder.A_PAGAR);
                }
                if (showInactive && !ticket)
                {
                    queryable = queryable.Where(o => o.StatusOrder == StatusOrder.A_PAGAR || o.StatusOrder == StatusOrder.PAGO);
                }
                if (ticket && !showInactive)
                {
                    queryable = queryable.Where(o => o.StatusOrder == StatusOrder.BOLETO_CHEQUE && o.WasAlreadyTicket == true);
                }
                if (ticket && showInactive)
                {
                    queryable = queryable.Where(o => o.StatusOrder == StatusOrder.BOLETO_CHEQUE || o.StatusOrder == StatusOrder.PAGO && o.WasAlreadyTicket == true);
                }
            }
            if (employee)
            {
                queryable = queryable.Where(o => o.StatusOrder == StatusOrder.BOLETO_CHEQUE || o.StatusOrder == StatusOrder.A_PAGAR);
            }



            if (!string.IsNullOrEmpty(orderId))
            {
                if (int.TryParse(orderId, out var id))
                    queryable = queryable.Where(o => o.Id == id);
                else
                    throw new ArgumentException("Id inválido");
            }

            if (initialDate.HasValue && !ticket)
                queryable = queryable.Where(o => o.OrderDate >= initialDate.Value);

            if (finalDate.HasValue && !ticket)
                queryable = queryable.Where(o => o.OrderDate <= finalDate.Value);

            if (initialDate.HasValue && ticket)
                queryable = queryable.Where(o => o.PaymentDay >= initialDate.Value);

            if (finalDate.HasValue && ticket)
                queryable = queryable.Where(o => o.PaymentDay <= finalDate.Value);

            if (!string.IsNullOrEmpty(searchValue))
            {
                searchValue = searchValue.Trim().ToUpper();

                queryable = queryable.Where(o =>
                    o.Fruit.Name.ToUpper().Contains(searchValue) ||
                    o.Supplier.Name.ToUpper().Contains(searchValue) ||
                    o.UserData.Name.ToUpper().Contains(searchValue)
                );
            }

            var queryableCount = queryable;

            if (string.IsNullOrEmpty(orderBy))
                queryable = queryable.OrderByDescending(o => o.Id);
            else
                queryable = queryable.OrderByField<Order>(orderBy);

            queryable = queryable.Skip(skip).Take(take);

            var ordersGetDTO = await queryable.Select(o =>
                new OrderGetDTO(
                    o.Id,
                    o.StatusOrder,
                    o.WasAlreadyTicket,
                    o.Fruit.Name,
                    o.FruitId,
                    o.Supplier.Name,
                    o.SupplierId,
                    o.UserData.Name,
                    o.BackLoad,
                    o.MiddleLoad,
                    o.FrontLoad,
                    o.TotalLoad,
                    o.UnitPrice,
                    o.TotalPrice,
                    o.PaymentDay,
                    o.CreatedAt,
                    o.OrderDate,
                    o.Store.Name,
                    o.Observation
                )
            ).ToListAsync();

            double totalRegs = await queryableCount.CountAsync();
            double totalPages = totalRegs > 0 ? Math.Ceiling(totalRegs / take) : 0;
            double allToPay;

            if (searchValue != null)
            {
                allToPay = await queryableCount.Where(o => o.StatusOrder != StatusOrder.PAGO).SumAsync(o => o.TotalPrice);
                double sumAllToPay = await _context.Order.Where(o => o.StatusOrder == StatusOrder.BOLETO_CHEQUE && o.Supplier.Name == searchValue).SumAsync(o => o.TotalPrice);
                allToPay += sumAllToPay;
            }
            else
            {
                allToPay = await _context.Order.Where(o => o.StatusOrder != StatusOrder.PAGO).SumAsync(o => o.TotalPrice);
            }

            double AllToPayMoney = await queryableCount.Where(o => o.StatusOrder == StatusOrder.A_PAGAR).SumAsync(o => o.TotalPrice);
            double allToPayTicket = await queryableCount.Where(o => o.StatusOrder == StatusOrder.BOLETO_CHEQUE).SumAsync(o => o.TotalPrice);

            if (!ticket)
            {
                if (searchValue != null)
                {
                    allToPayTicket = await _context.Order.Where(o => o.StatusOrder == StatusOrder.BOLETO_CHEQUE && o.Supplier.Name == searchValue).SumAsync(o => o.TotalPrice);
                }
                else
                {
                    allToPayTicket = await _context.Order.Where(o => o.StatusOrder == StatusOrder.BOLETO_CHEQUE).SumAsync(o => o.TotalPrice);
                }
            }

            double allToPayStore1 = await queryableCount.Where(o => o.Store.Name == "Loja 1" && o.StatusOrder != StatusOrder.PAGO).SumAsync(o => o.TotalPrice);
            double allToPayStore2 = await queryableCount.Where(o => o.Store.Name == "Loja 2" && o.StatusOrder != StatusOrder.PAGO).SumAsync(o => o.TotalPrice);

            var ticketNotifications = await _context.Order.Where(o => o.StatusOrder == StatusOrder.BOLETO_CHEQUE && o.PaymentDay <= DateTime.Today.AddDays(3)).ToListAsync();

            var getWithPaginationGetDTO = new GetWithPaginationGetDTO<OrderGetDTO>(ordersGetDTO, (int)totalRegs, (int)totalPages, Math.Round(allToPay, 2), Math.Round(allToPayTicket, 2), Math.Round(allToPayStore1, 2), Math.Round(allToPayStore2, 2), Math.Round(AllToPayMoney, 2));

            return getWithPaginationGetDTO;
        }

        public async Task<dynamic> GetOrderByIdAsync<T>(int orderId)
        {
            var queryable = _context.Order.AsNoTracking().Where(u => u.Id == orderId);

            if (typeof(T).IsAssignableFrom(typeof(OrderGetDTO)))
            {
                queryable = queryable.Include(o => o.Fruit).AsSplitQuery()
                                    .Include(o => o.Supplier).AsSplitQuery()
                                    .Include(o => o.UserData).AsSplitQuery()
                                    .Include(o => o.Store).AsSplitQuery();
            }

            var order = await queryable.FirstOrDefaultAsync();

            if (order == null)
            {
                _logger.LogInformation("Não existe um pedido com o id {OrderId}", orderId);
                throw new NotFoundException($"Não existe um pedido com o id {orderId}");
            }
            else
            {
                if (typeof(T).IsAssignableFrom(typeof(Order)))
                    return order;
                else if (typeof(T).IsAssignableFrom(typeof(OrderGetDTO)))
                {
                    var orderGetDTO = new OrderGetDTO(order.Id, order.StatusOrder, order.WasAlreadyTicket, order.Fruit.Name, order.FruitId, order.Supplier.Name, order.SupplierId, order.UserData.Name, order.BackLoad, order.MiddleLoad, order.FrontLoad, order.TotalLoad, order.UnitPrice, order.TotalPrice, order.PaymentDay, order.CreatedAt, order.OrderDate, order.Store.Name, order.Observation);
                    return await Task.FromResult(orderGetDTO);
                }
                else
                {
                    _logger.LogInformation("O tipo {NameOfT} não é valido para o contexto", nameof(T));
                    throw new ArgumentException($"O tipo {nameof(T)} não é válido para o contexto");
                }
            }
        }

        //public async Task<double> GetAllToPayAsync() =>
        //    await _context.Order.Where(o => o.StatusOrder == StatusOrder.A_PAGAR).SumAsync(o => o.TotalPrice);

        public async Task<List<ActiveWeekGetDTO>> GetActiveWeeksAsync()
        {
            var activeWeeksGetDTO = new List<ActiveWeekGetDTO>();
            var countOrders = await _context.Order.AsNoTracking().CountAsync();

            if (countOrders > 0)
            {
                #region DateRange
                var initialDate = await _context.Order.AsNoTracking().MinAsync(o => o.OrderDate);
                var endDate = await _context.Order.AsNoTracking().MaxAsync(o => o.OrderDate);

                initialDate = DateTime.Parse(initialDate.ToShortDateString()).AddDays(-2);
                endDate = DateTime.Parse(endDate.ToShortDateString());

                List<DateTime> dates = [];

                for (var date = initialDate; date < endDate.AddDays(1); date = date.AddDays(1))
                {
                    dates.Add(date);
                }
                #endregion DateRange

                #region WeekRange
                var startDay = (initialDate.DayOfWeek == DayOfWeek.Monday) ? initialDate : initialDate.AddDays(((int)initialDate.DayOfWeek - (int)DayOfWeek.Monday) * -1);
                var endDay = (endDate.DayOfWeek == DayOfWeek.Sunday) ? endDate : endDate.AddDays(6 - ((int)endDate.DayOfWeek));

                for (var date = startDay; date < endDay.AddDays(1); date = date.AddDays(7))
                {
                    var startDayWeek = date;
                    var endDayWeek = date.AddDays(6);

                    activeWeeksGetDTO.Add(new ActiveWeekGetDTO()
                    {
                        Id = Guid.NewGuid(),
                        OptionText = (startDayWeek == endDayWeek) ? startDayWeek.ToString("dd/MM/yyyy", _ptBR.DateTimeFormat) : CustomConverter.GetWeekDescription(startDayWeek, endDayWeek, _ptBR.DateTimeFormat),
                        InitialDate = startDayWeek,
                        EndDate = endDayWeek
                    });
                }
                #endregion WeekRange
            }

            return activeWeeksGetDTO.OrderByDescending(a => a.EndDate).ToList();
        }

        public async Task InsertAsync(OrderPostDTO orderPostDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in orderPostDTO.OrderItemsPostDTO)
                {
                    if (item.TotalPrice > 0)
                    {
                        if (orderPostDTO.StatusOrder == StatusOrder.BOLETO_CHEQUE)
                        {
                            orderPostDTO.WasAlreadyTicket = true;
                        }
                        var order = new Order(orderPostDTO.StatusOrder, orderPostDTO.WasAlreadyTicket, orderPostDTO.FruitId, orderPostDTO.SupplierId, item.StoreId.Value, orderPostDTO.UserDataId, item.BackLoad, item.MiddleLoad, item.FrontLoad, item.TotalLoad, orderPostDTO.UnitPrice, item.TotalPrice, orderPostDTO.PaymentDay, orderPostDTO.CreatedAt, orderPostDTO.OrderDate, item.Observation);
                        _context.Order.Add(order);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException e)
            {
                await transaction.RollbackAsync();

                _logger.LogInformation("Erro ao incluir novo pedido. Message: {Message}", e.Message);
                throw new DbUpdateException(e.Message);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();

                _logger.LogInformation("Erro ao incluir novo pedido. Message: {Message}", e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task UpdateAsync(Order order)
        {
            try
            {
                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                _logger.LogInformation("Erro ao atualizar pedido. Message: {Message}", e.Message);
                throw new DbUpdateConcurrencyException(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogInformation("Erro ao atualizar pedido. Message: {Message}", e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task DeleteAsync(Order order)
        {
            try
            {
                _context.Order.Remove(order);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                _logger.LogInformation("Erro ao excluir pedido. Message: {Message}", e.Message);
                throw new DbUpdateException(e.Message, e);
            }
            catch (Exception e)
            {
                _logger.LogInformation("Erro ao excluir pedido. Message: {Message}", e.Message);
                throw new Exception(e.Message);
            }
        }
    }
}
