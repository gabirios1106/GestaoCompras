using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.DTO.Access
{
    public class AuthenticatedUserGetDTO
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public int UserDataId { get; set; }

        public AuthenticatedUserGetDTO() { }

    }
}
