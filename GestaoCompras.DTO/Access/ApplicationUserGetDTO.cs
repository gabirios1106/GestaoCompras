using GestaoCompras.Enums.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.DTO.Access
{
    public class ApplicationUserGetDTO
    {
        public Guid AppId { get; set; }

        public string AppName { get; set; }

        public string AppPasswordHash { get; set; }

        public DateTime CreatedAt { get; set; }

        public Status Status { get; set; }

        public ApplicationUserGetDTO() { }

        public ApplicationUserGetDTO(Guid appId, string appName, DateTime createdAt, Status status)
        {
            AppId = appId;
            AppName = appName;
            CreatedAt = createdAt;
            Status = status;
        }

        public ApplicationUserGetDTO(string appName, string appPasswordHash)
        {
            AppId = Guid.NewGuid();
            AppName = appName;
            AppPasswordHash = appPasswordHash;
            CreatedAt = DateTime.UtcNow;
            Status = Status.ATIVO;
        }

    }
}
