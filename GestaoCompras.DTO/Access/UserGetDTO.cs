using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.DTO.Access;

public class UserGetDTO
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public UserGetDTO()
    {
        
    }
    public UserGetDTO(Guid id, string userName)
    {
        Id = id;
        UserName = userName;
    }
}
