using GestaoCompras.API.Data;
using GestaoCompras.API.Interfaces.Suppliers;
using GestaoCompras.API.Services.Suppliers;
using GestaoCompras.API.Utils.ErrorHandler;
using GestaoCompras.DTO.Supplier;
using GestaoCompras.Models.Suppliers;
using GestaoCompras.Utils.Extentions;
using Microsoft.EntityFrameworkCore;

namespace GestaoCompras.API.Services.Suppliers
{
    public class SupplierService(ApplicationDbContext context, ILogger<SupplierService> logger) : ISupplierService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<SupplierService> _logger = logger;

        public async Task<List<SupplierGetDTO>> GetSuppliersAsync(string orderBy)
        {
            var queryable = _context.Supplier.AsNoTracking().AsSplitQuery();

            if (string.IsNullOrEmpty(orderBy))
                queryable = queryable.OrderByDescending(f => f.Id);
            else
                queryable = queryable.OrderByField<Supplier>(orderBy);

            var fruitsGetDTO = await queryable.Select(f => new SupplierGetDTO(f.Id, f.Name)).ToListAsync();

            return fruitsGetDTO;
        }

        public async Task<dynamic> GetSupplierByIdAsync<T>(int fruitId)
        {
            var fruit = await _context.Supplier.AsNoTracking().AsSplitQuery().Where(u => u.Id == fruitId).FirstOrDefaultAsync();

            if (fruit == null)
            {
                _logger.LogInformation("Não existe uma fruta com o id {SupplierId}", fruitId);
                throw new NotFoundException($"Não existe uma fruta com o id {fruitId}");
            }
            else
            {
                if (typeof(T).IsAssignableFrom(typeof(Supplier)))
                    return fruit;
                else if (typeof(T).IsAssignableFrom(typeof(SupplierGetDTO)))
                {
                    var fruitGetDTO = new SupplierGetDTO(fruit.Id, fruit.Name);
                    return await Task.FromResult(fruitGetDTO);
                }
                else
                {
                    _logger.LogInformation("O tipo {NameOfT} não é valido para o contexto", nameof(T));
                    throw new ArgumentException($"O tipo {nameof(T)} não é válido para o contexto");
                }
            }
        }

        public async Task InsertAsync(Supplier fruit)
        {
            try
            {
                _context.Supplier.Add(fruit);
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
        public async Task<bool> ExistsByNameAsync(string name) => await _context.Supplier.AsNoTracking().AnyAsync(un => un.Name == name);
    }
}
