using GestaoCompras.DTO.Fruit;

namespace GestaoCompras.Web.Interfaces.Fruits
{
    public interface IFruitService
    {
        Task<List<FruitGetDTO>> GetFruitsAsync(string requestUri);

        Task<FruitGetDTO> GetFruitByIdAsync(string requestUri);

        Task<FruitGetDTO> CreateWithGetObjectAsync(string requestUri, FruitPostDTO fruitPostDTO);
    }

}
