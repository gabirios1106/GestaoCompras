using GestaoCompras.DTO.Supplier;
using GestaoCompras.Web.Interfaces.Common;
using GestaoCompras.Web.Interfaces.Supplier;

namespace GestaoCompras.Web.Services.Supplier
{
    public class SupplierService(IApiService apiService) : ISupplierService
    {
        private readonly IApiService _apiService = apiService;
        private readonly string _apiVersion = "v1";

        public async Task<List<SupplierGetDTO>> GetSuppliersAsync(string requestUri) =>
            await _apiService.GetWithoutPaginationAsync<SupplierGetDTO>($"{_apiVersion}/{requestUri}");

        public async Task<SupplierGetDTO> GetSupplierByIdAsync(string requestUri) =>
            await _apiService.GetByIdAsync<SupplierGetDTO>($"{_apiVersion}/{requestUri}");

        public async Task<SupplierGetDTO> CreateWithGetObjectAsync(string requestUri, SupplierPostDTO supplierPostDTO) =>
            await _apiService.CreateWithGetObjectAsync<SupplierGetDTO, SupplierPostDTO>($"{_apiVersion}/{requestUri}", supplierPostDTO);
    }
}
