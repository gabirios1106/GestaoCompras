using GestaoCompras.DTO.Common;

namespace GestaoCompras.Web.Interfaces.Common
{
    public interface IApiService
    {
        Task<List<T>> GetWithoutPaginationAsync<T>(string requestUri);

        Task<T> GetByIdAsync<T>(string requestUri);

        Task<GetWithPaginationGetDTO<T>> GetAsync<T>(string requestUri);

        Task<T> GetInfoAsync<T>(string requestUri);

        Task<bool> CreateAsync<T>(string requestUri, T value);

        Task<G> CreateWithGetObjectAsync<G, P>(string requestUri, P value);

        Task<bool> UpdateAsync<T>(string requestUri, T value);

        Task<bool> DeleteAsync(string requestUri);
    }
}
