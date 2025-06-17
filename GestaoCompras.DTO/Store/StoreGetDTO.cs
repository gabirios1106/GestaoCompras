using GestaoCompras.Enums.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.DTO.Store
{
    public class StoreGetDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Status Status { get; set; }

        public StoreGetDTO(){}

        public StoreGetDTO(int id, string name, Status status)
        {
            Id = id;
            Name = name;
            Status = status;
        }
    }
}
