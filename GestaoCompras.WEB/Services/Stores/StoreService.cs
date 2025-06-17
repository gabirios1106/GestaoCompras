using GestaoCompras.DTO.Store;
using GestaoCompras.Web.Interfaces.Common;
using GestaoCompras.Web.Interfaces.Stores;
using GestaoCompras.Web.Services.Common;

namespace GestaoCompras.Web.Services.Stores
{
    public class StoreService(IApiService apiService) : IStoreService
    {
        private readonly IApiService _apiService = apiService;
        private readonly string _apiVersion = "v1";

        public async Task<List<StoreGetDTO>> GetStoresAsync(string requestUri) =>
            await _apiService.GetWithoutPaginationAsync<StoreGetDTO>($"{_apiVersion}/{requestUri}");
    }
}
