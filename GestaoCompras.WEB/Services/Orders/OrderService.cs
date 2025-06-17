using GestaoCompras.DTO.Common;
using GestaoCompras.DTO.Order;
using GestaoCompras.Web.Interfaces.Common;
using GestaoCompras.Web.Interfaces.Orders;
using System.Reflection.Metadata.Ecma335;

namespace GestaoCompras.Web.Services.Orders
{
    public class OrderService(IApiService apiService) : IOrderService
    {
        private readonly IApiService _apiService = apiService;
        private readonly string _apiVersion = "v1";

        public async Task<GetWithPaginationGetDTO<OrderGetDTO>> GetOrdersAsync(string requestUri) =>
       await _apiService.GetAsync<OrderGetDTO>($"{_apiVersion}/{requestUri}");

        public async Task<List<ActiveWeekGetDTO>> GetActiveWeeksAsync(string requestUri) =>
            await _apiService.GetWithoutPaginationAsync<ActiveWeekGetDTO>($"{_apiVersion}/{requestUri}");

        public async Task<double> GetAllToPayAsync(string requestUri) =>
            await _apiService.GetInfoAsync<double>($"{_apiVersion}/{requestUri}");

        public async Task<bool> CreateAsync(string requestUri, OrderPostDTO orderPostDTO) =>
            await _apiService.CreateAsync($"{_apiVersion}/{requestUri}", orderPostDTO);

        public async Task<bool> UpdateAsync(string requestUri, OrderPutDTO orderPutDTO) =>
            await _apiService.UpdateAsync($"{_apiVersion}/{requestUri}", orderPutDTO);

        public async Task<bool> DeleteAsync(string requestUri) =>
            await _apiService.DeleteAsync($"{_apiVersion}/{requestUri}");

    }
}
