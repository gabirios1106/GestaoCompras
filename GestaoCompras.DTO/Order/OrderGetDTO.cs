using GestaoCompras.Enums.General;
using GestaoCompras.Enums.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.DTO.Order
{
    public class OrderGetDTO
    {
        public int Id { get; set; }

        public StatusOrder StatusOrder { get; set; }
        public bool WasAlreadyTicket { get; set; }

        public string FruitName { get; set; }
        public int FruitId { get; set; }

        public string SupplierName { get; set; }
        public int SupplierId { get; set; }

        public string StoreName { get; set; }
        public int? StoreId { get; set; }

        public string UserDataName { get; set; }


        public int BackLoad { get; set; }
        public int MiddleLoad { get; set; }
        public int FrontLoad { get; set; }
        public string? Observation { get; set; }

        public int TotalLoad { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice { get; set; }
        public DateTime? PaymentDay { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime OrderData {  get; set; }

        public OrderGetDTO(){}

        public OrderGetDTO(int id, StatusOrder statusOrder, bool wasAlreadyTicket, string fruitName, int fruitId, string supplierName, int supplierId, string userDataName, int backLoad, int middleLoad, int frontLoad, int totalLoad, double unitPrice, double totalPrice, DateTime? paymentDay, DateTime createdAt, DateTime orderData, string storeName, string? observation)
        {
            Id = id;
            StatusOrder = statusOrder;
            FruitName = fruitName;
            FruitId = fruitId;
            SupplierName = supplierName;
            SupplierId = supplierId;
            UserDataName = userDataName;
            BackLoad = backLoad;
            MiddleLoad = middleLoad;
            FrontLoad = frontLoad;
            TotalLoad = totalLoad;
            UnitPrice = unitPrice;
            TotalPrice = totalPrice;
            PaymentDay = paymentDay;
            CreatedAt = createdAt;
            OrderData = orderData;
            StoreName = storeName;
            WasAlreadyTicket = wasAlreadyTicket;
            Observation = observation;
        }
    }
}
