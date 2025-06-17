using GestaoCompras.Models.Users;

namespace GestaoCompras.Models.Fruits
{
    public class Fruit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }

        public DateTime CreatedAt { get; set; }

        public UserData UserData { get; set; }
        public int UserDataId { get; set; }

        public Fruit() { }

        public Fruit(int id, string fruitName, double price, DateTime createdAt, UserData userData, int userDataId)
        {
            Id = id;
            Name = fruitName;
            Price = price;
            CreatedAt = createdAt;
            UserData = userData;
            UserDataId = userDataId;
        }

        public void PrepareForUpdate(double price) => Price = price;
    }
}
