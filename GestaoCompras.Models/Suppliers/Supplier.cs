using GestaoCompras.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.Models.Suppliers
{
    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.Now;


        public UserData UserData { get; set; }
        public int UserDataId { get; set; }

        public Supplier(){}

        public Supplier(int id, string name, DateTime createdAt, UserData userData, int userDataId)
        {
            Id = id;
            Name = name;
            CreatedAt = createdAt;
            UserData = userData;
            UserDataId = userDataId;
        }
    }
}
