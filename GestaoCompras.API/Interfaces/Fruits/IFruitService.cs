using GestaoCompras.DTO.Fruit;
using GestaoCompras.Models.Fruits;

namespace GestaoCompras.API.Interfaces.Fruits
{
    public interface IFruitService
    {
        Task<List<FruitGetDTO>> GetFruitsAsync(string orderBy);

        Task<dynamic> GetFruitByIdAsync<T>(int fruitId);

        Task InsertAsync(Fruit fruit);

        Task UpdateAsync(Fruit fruit);

        Task<bool> ExistsByNameAsync(string name);
    }
}
