using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.DTO.Supplier
{
    public class SupplierGetDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public SupplierGetDTO()
        {
            
        }

        public SupplierGetDTO(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
