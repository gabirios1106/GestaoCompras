using GestaoCompras.DTO.Supplier;
using GestaoCompras.Models.Suppliers;

namespace GestaoCompras.API.Interfaces.Suppliers
{
    public interface ISupplierService
    {
        Task<List<SupplierGetDTO>> GetSuppliersAsync(string orderBy);

        Task<dynamic> GetSupplierByIdAsync<T>(int supplierId);

        Task InsertAsync(Supplier supplier);

        Task<bool> ExistsByNameAsync(string name);
    }
}
