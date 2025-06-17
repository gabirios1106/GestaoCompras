using GestaoCompras.Models.Users;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.Models.Access;

public class User : IdentityUser<Guid>
{
    public UserData UserData { get; set; }
    public User() : base(){ }
    public User(string email, string passwordHash) : base(email)
    {
        Email = email;
        UserName = email;
        PasswordHash = passwordHash;  
    }
}
