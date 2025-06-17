using GestaoCompras.Enums.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.DTO.Access
{
    public class ApplicationUserPostDTO
    {
        public Guid AppId { get; set; }

        public string AppName { get; set; }

        public DateTime CreatedAt { get; set; }

        public Status Status { get; set; }

        public ApplicationUserPostDTO()
        {
            AppId = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            Status = Status.ATIVO;
        }

        public ApplicationUserPostDTO(string appName)
        {
            AppName = appName;
        }

    }
}
