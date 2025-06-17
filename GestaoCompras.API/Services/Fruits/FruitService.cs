using GestaoCompras.API.Data;
using GestaoCompras.API.Interfaces.Fruits;
using GestaoCompras.API.Utils.ErrorHandler;
using GestaoCompras.DTO.Fruit;
using GestaoCompras.Models.Fruits;
using GestaoCompras.Utils.Extentions;
using Microsoft.EntityFrameworkCore;

namespace GestaoCompras.API.Services.Fruits;

public class FruitService(ApplicationDbContext context, ILogger<FruitService> logger) : IFruitService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<FruitService> _logger = logger;

    public async Task<List<FruitGetDTO>> GetFruitsAsync(string orderBy)
    {
        var queryable = _context.Fruit.AsNoTracking().AsSplitQuery();

        if (string.IsNullOrEmpty(orderBy))
            queryable = queryable.OrderByDescending(f => f.Id);
        else
            queryable = queryable.OrderByField<Fruit>(orderBy);

        var fruitsGetDTO = await queryable.Select(f => new FruitGetDTO(f.Id, f.Name, f.Price)).ToListAsync();

        return fruitsGetDTO;
    }

    public async Task<dynamic> GetFruitByIdAsync<T>(int fruitId)
    {
        var fruit = await _context.Fruit.AsNoTracking().AsSplitQuery().Where(u => u.Id == fruitId).FirstOrDefaultAsync();

        if (fruit == null)
        {
            _logger.LogInformation("Não existe uma fruta com o id {FruitId}", fruitId);
            throw new NotFoundException($"Não existe uma fruta com o id {fruitId}");
        }
        else
        {
            if (typeof(T).IsAssignableFrom(typeof(Fruit)))
                return fruit;
            else if (typeof(T).IsAssignableFrom(typeof(FruitGetDTO)))
            {
                var fruitGetDTO = new FruitGetDTO(fruit.Id, fruit.Name, fruit.Price);
                return await Task.FromResult(fruitGetDTO);
            }
            else
            {
                _logger.LogInformation("O tipo {NameOfT} não é valido para o contexto", nameof(T));
                throw new ArgumentException($"O tipo {nameof(T)} não é válido para o contexto");
            }
        }
    }

    public async Task InsertAsync(Fruit fruit)
    {
        try
        {
            _context.Fruit.Add(fruit);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            _logger.LogInformation("Erro ao incluir nova fruta. Message: {Message}", e.Message);
            throw new DbUpdateException(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogInformation("Erro ao incluir nova fruta. Message: {Message}", e.Message);
            throw new Exception(e.Message);
        }
    }

    public async Task UpdateAsync(Fruit fruit)
    {
        try
        {
            _context.Entry(fruit).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            _logger.LogInformation("Erro ao atualizar fruta. Message: {Message}", e.Message);
            throw new DbUpdateConcurrencyException(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogInformation("Erro ao atualizar fruta. Message: {Message}", e.Message);
            throw new Exception(e.Message);
        }
    }

    public async Task<bool> ExistsByNameAsync(string name) => await _context.Fruit.AsNoTracking().AnyAsync(un => un.Name == name);
}

