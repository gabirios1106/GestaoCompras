using GestaoCompras.DTO.Fruit;
using GestaoCompras.Web.Interfaces.Common;
using GestaoCompras.Web.Interfaces.Fruits;

namespace GestaoCompras.Web.Services.Fruits
{
    public class FruitService(IApiService apiService) : IFruitService
    {
        private readonly IApiService _apiService = apiService;
        private readonly string _apiVersion = "v1";

        public async Task<List<FruitGetDTO>> GetFruitsAsync(string requestUri) =>
            await _apiService.GetWithoutPaginationAsync<FruitGetDTO>($"{_apiVersion}/{requestUri}");

        public async Task<FruitGetDTO> GetFruitByIdAsync(string requestUri) =>
            await _apiService.GetByIdAsync<FruitGetDTO>($"{_apiVersion}/{requestUri}");

        public async Task<FruitGetDTO> CreateWithGetObjectAsync(string requestUri, FruitPostDTO fruitPostDTO) =>
            await _apiService.CreateWithGetObjectAsync<FruitGetDTO, FruitPostDTO>($"{_apiVersion}/{requestUri}", fruitPostDTO);
    }
}
