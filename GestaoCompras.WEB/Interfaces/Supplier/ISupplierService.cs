using GestaoCompras.DTO.Fruit;
using GestaoCompras.DTO.Supplier;

namespace GestaoCompras.Web.Interfaces.Supplier
{
    public interface ISupplierService
    {
        Task<List<SupplierGetDTO>> GetSuppliersAsync(string requestUri);

        Task<SupplierGetDTO> GetSupplierByIdAsync(string requestUri);

        Task<SupplierGetDTO> CreateWithGetObjectAsync(string requestUri, SupplierPostDTO supplierPostDTO);
    }
}
