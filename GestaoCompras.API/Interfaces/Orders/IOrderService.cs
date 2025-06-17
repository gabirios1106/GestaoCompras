using GestaoCompras.DTO.Common;
using GestaoCompras.DTO.Order;
using GestaoCompras.Models.Orders;

namespace GestaoCompras.API.Interfaces.Orders;

public interface IOrderService
{
    Task<GetWithPaginationGetDTO<OrderGetDTO>> GetOrdersAsync(string orderBy, int skip, int take, bool ticket = false, bool employee = false, bool showInactive = false, string searchValue = null, string orderId = null, DateTime? initialDate = null, DateTime? finalDate = null);

    Task<dynamic> GetOrderByIdAsync<T>(int orderId);

    //Task<double> GetAllToPayAsync();

    Task<List<ActiveWeekGetDTO>> GetActiveWeeksAsync();

    Task InsertAsync(OrderPostDTO orderPostDTO);

    Task UpdateAsync(Order order);

    Task DeleteAsync(Order order);
}
