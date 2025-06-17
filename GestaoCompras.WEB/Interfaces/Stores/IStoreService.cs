using GestaoCompras.DTO.Store;

namespace GestaoCompras.Web.Interfaces.Stores;

public interface IStoreService
{
    Task<List<StoreGetDTO>> GetStoresAsync(string requestUri);
}
