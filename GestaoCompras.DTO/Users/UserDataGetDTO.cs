using GestaoCompras.Enums.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.DTO.Users;

public class UserDataGetDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Guid UserId { get; set; }
    public UserRole UserRole { get; set; }

    public UserDataGetDTO() { }

    public UserDataGetDTO(int id, string name, Guid userId)
    {
        Id = id;
        Name = name;
        UserId = userId;
    }

}
