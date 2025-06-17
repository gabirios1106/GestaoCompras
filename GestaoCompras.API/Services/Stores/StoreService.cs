using GestaoCompras.API.Data;
using GestaoCompras.API.Interfaces.Stores;
using GestaoCompras.API.Utils.ErrorHandler;
using GestaoCompras.DTO.Store;
using GestaoCompras.Enums.General;
using GestaoCompras.Models.Fruits;
using GestaoCompras.Models.Stores;
using GestaoCompras.Utils.Extentions;
using Microsoft.EntityFrameworkCore;

namespace GestaoCompras.API.Services.Stores;

public class StoreService(ApplicationDbContext context, ILogger<StoreService> logger) : IStoreService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<StoreService> _logger = logger;

    public async Task<List<StoreGetDTO>> GetStoresAsync(string orderBy, bool showInactive)
    {   
        var queryable = _context.Store.AsNoTracking().AsSplitQuery();

        if (!showInactive)
        {
            queryable = queryable.Where(s => s.Status == Status.ATIVO);
        }

        if (string.IsNullOrEmpty(orderBy))
            queryable = queryable.OrderByDescending(f => f.Id);
        else
            queryable = queryable.OrderByField<Store>(orderBy);

        var StoresGetDTO = await queryable.Select(s => new StoreGetDTO(s.Id, s.Name, s.Status)).ToListAsync();

        return StoresGetDTO;
    }

    public async Task<dynamic> GetStoreByIdAsync<T>(int StoreId)
    {
        var Store = await _context.Store.AsNoTracking().AsSplitQuery().Where(u => u.Id == StoreId).FirstOrDefaultAsync();

        if (Store == null)
        {
            _logger.LogInformation("Não existe uma loja com o id {StoreId}", StoreId);
            throw new NotFoundException($"Não existe uma loja com o id {StoreId}");
        }
        else
        {
            if (typeof(T).IsAssignableFrom(typeof(Store)))
                return Store;
            else if (typeof(T).IsAssignableFrom(typeof(StoreGetDTO)))
            {
                var StoreGetDTO = new StoreGetDTO(Store.Id, Store.Name, Store.Status);
                return await Task.FromResult(StoreGetDTO);
            }
            else
            {
                _logger.LogInformation("O tipo {NameOfT} não é valido para o contexto", nameof(T));
                throw new ArgumentException($"O tipo {nameof(T)} não é válido para o contexto");
            }
        }
    }

    public async Task InsertAsync(Store Store)
    {
        try
        {
            _context.Store.Add(Store);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            _logger.LogInformation("Erro ao incluir nova loja. Message: {Message}", e.Message);
            throw new DbUpdateException(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogInformation("Erro ao incluir nova loja. Message: {Message}", e.Message);
            throw new Exception(e.Message);
        }
    }

    public async Task UpdateAsync(Store Store)
    {
        try
        {
            _context.Entry(Store).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            _logger.LogInformation("Erro ao atualizar loja. Message: {Message}", e.Message);
            throw new DbUpdateConcurrencyException(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogInformation("Erro ao atualizar loja. Message: {Message}", e.Message);
            throw new Exception(e.Message);
        }
    }
}
