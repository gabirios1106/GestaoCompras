using GestaoCompras.DTO.Store;
using GestaoCompras.Models.Stores;

namespace GestaoCompras.API.Interfaces.Stores
{
    public interface IStoreService
    {
        Task<List<StoreGetDTO>> GetStoresAsync(string orderBy, bool showInactive);

        Task<dynamic> GetStoreByIdAsync<T>(int StoreId);

        Task InsertAsync(Store Store);

        Task UpdateAsync(Store Store);
    }
}
