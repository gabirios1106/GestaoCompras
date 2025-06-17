using GestaoCompras.DTO.Common;
using GestaoCompras.DTO.Order;

namespace GestaoCompras.Web.Interfaces.Orders
{
    public interface IOrderService
    {
        Task<GetWithPaginationGetDTO<OrderGetDTO>> GetOrdersAsync(string requestUri);

        Task<List<ActiveWeekGetDTO>> GetActiveWeeksAsync(string requestUri);

        Task<double> GetAllToPayAsync(string requestUri);

        Task<bool> CreateAsync(string requestUri, OrderPostDTO orderPostDTO);

        Task<bool> UpdateAsync(string requestUri, OrderPutDTO orderPutDTO);

        Task<bool> DeleteAsync(string requestUri);
    }
}
