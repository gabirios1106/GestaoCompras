using GestaoCompras.Enums.Users;
using GestaoCompras.Models.Access;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.Models.Users;

public class UserData
{
    public int Id { get; set; }
    public UserRole UserRole { get; set; }
    public string Name { get; set; }
    public Guid RefreshTokenJwtId { get; set; }
    public Guid TokenJwtId { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public DateTime CreatedAt { get; set; }

    public UserData() { }

    public UserData(int id, string name, Guid refreshTokenJwtId, Guid tokenJwtId, Guid userId, User user, DateTime createdAt)
    {
        Id = id;
        Name = name;
        RefreshTokenJwtId = refreshTokenJwtId;
        TokenJwtId = tokenJwtId;
        UserId = userId;
        User = user;
        CreatedAt = createdAt;
    }

    public UserData(string name, Guid userId)
    {
        Name = name;   
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }
}
