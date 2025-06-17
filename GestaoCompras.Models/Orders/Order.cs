using GestaoCompras.Enums.General;
using GestaoCompras.Enums.Orders;
using GestaoCompras.Models.Fruits;
using GestaoCompras.Models.Stores;
using GestaoCompras.Models.Suppliers;
using GestaoCompras.Models.Users;

namespace GestaoCompras.Models.Orders
{
    public class Order
    {
        public int Id { get; set; }

        public StatusOrder StatusOrder { get; set; }

        public Fruit Fruit { get; set; }
        public int FruitId { get; set; }

        public Supplier Supplier { get; set; }
        public int SupplierId { get; set; }

        public Store Store { get; set; }
        public int? StoreId { get; set; }

        public UserData UserData { get; set; }
        public int UserDataId { get; set; }

        public int BackLoad { get; set; }
        public int MiddleLoad { get; set; }
        public int FrontLoad { get; set; }
        public int TotalLoad { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice { get; set; }
        public DateTime? PaymentDay { get; set; }
        public bool WasAlreadyTicket { get; set; }
        public string? Observation { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime OrderDate { get; set; }

        public Order() { }

        public Order(StatusOrder statusOrder, bool wasAlreadyTicket, int fruitId, int supplierId, int storeId, int userDataId, int backLoad, int middleLoad, int frontLoad, int totalLoad, double unitPrice, double totalPrice, DateTime? paymentDay, DateTime createdAt, DateTime orderDate, string? observation)
        {
            StatusOrder = statusOrder;
            WasAlreadyTicket = wasAlreadyTicket;
            FruitId = fruitId;
            SupplierId = supplierId;
            StoreId = storeId;
            UserDataId = userDataId;
            BackLoad = backLoad;
            MiddleLoad = middleLoad;
            FrontLoad = frontLoad;
            TotalLoad = totalLoad;
            UnitPrice = unitPrice;
            TotalPrice = totalPrice;
            CreatedAt = createdAt;
            OrderDate = orderDate;
            PaymentDay = paymentDay;
            Observation = observation;
        }

        public void PrepareForUpdate(StatusOrder statusOrder, int fruitId, int supplierId, int backLoad, int middleLoad, int frontLoad, int totalLoad, double unitPrice, double totalPrice, bool wasAlreadyTicket, DateTime? paymentDay, string? observation)
        {
            StatusOrder = statusOrder;
            FruitId = fruitId;
            SupplierId = supplierId;
            BackLoad = backLoad;
            MiddleLoad = middleLoad;
            FrontLoad = frontLoad;
            TotalLoad = totalLoad;
            UnitPrice = unitPrice;
            TotalPrice = totalPrice;
            PaymentDay = paymentDay;
            WasAlreadyTicket = wasAlreadyTicket;
            Observation = observation;
        }
    }
}
