using GestaoCompras.Enums.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.Models.Access
{
    public class ApplicationUser
    {
        public Guid AppId { get; set; }
        public string AppName { get; set; }
        public string AppPasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public Status Status { get; set; }
        public ApplicationUser() { }

        public ApplicationUser(string appName, string appPasswordHash)
        {
            AppId = Guid.NewGuid();
            AppName = appName;
            AppPasswordHash = appPasswordHash;
            CreatedAt = DateTime.UtcNow;
            Status = Status.ATIVO;
        }

    }
}
