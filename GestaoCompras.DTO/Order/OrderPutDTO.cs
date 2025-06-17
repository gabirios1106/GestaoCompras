using GestaoCompras.Enums.Orders;
using System.ComponentModel.DataAnnotations;

namespace GestaoCompras.DTO.Order
{
    public class OrderPutDTO
    {
        [Display(Name = "ID")]
        [Range(1, Int32.MaxValue, ErrorMessage = "O campo {0} deve ser preenchido")]
        public int Id { get; set; }

        [Display(Name = "StatusOrder")]
        [Required(ErrorMessage = "O campo {0} deve ser preenchido")]
        [EnumDataType(typeof(StatusOrder), ErrorMessage = "Valor inválido para o campo {0}")]
        public StatusOrder StatusOrder { get; set; }
        public bool WasAlreadyTicket { get; set; }

        [Display(Name = "Fruta")]
        [Range(1, Int32.MaxValue, ErrorMessage = "O campo {0} deve ser preenchido")]
        public int FruitId { get; set; }

        [Display(Name = "Fornecedor")]
        [Range(1, Int32.MaxValue, ErrorMessage = "O campo {0} deve ser preenchido")]
        public int SupplierId { get; set; }

        [Display(Name = "Preço unitário")]
        [Range(0, Double.MaxValue, ErrorMessage = "O campo {0} não pode ser preenchido com um valor negativo")]
        public double UnitPrice { get; set; }

        [Display(Name = "Carga de trás")]
        [Range(0, Double.MaxValue, ErrorMessage = "O campo {0} não pode ser preenchido com um valor negativo")]
        public int BackLoad { get; set; }

        [Display(Name = "Carga do meio")]
        [Range(0, Double.MaxValue, ErrorMessage = "O campo {0} não pode ser preenchido com um valor negativo")]
        public int MiddleLoad { get; set; }

        [Display(Name = "Carga da frente")]
        [Range(0, Double.MaxValue, ErrorMessage = "O campo {0} não pode ser preenchido com um valor negativo")]
        public int FrontLoad { get; set; }

        public int TotalLoad { get; set; }

        public string? Observation { get; set; }
        public double TotalPrice { get; set; }

        public DateTime? PaymentDay { get; set; }

        public OrderPutDTO() { }

        public OrderPutDTO(OrderGetDTO orderGetDTO)
        {
            Id = orderGetDTO.Id;
            StatusOrder = orderGetDTO.StatusOrder;
            WasAlreadyTicket = orderGetDTO.WasAlreadyTicket;
            FruitId = orderGetDTO.FruitId;
            SupplierId = orderGetDTO.SupplierId;
            UnitPrice = orderGetDTO.UnitPrice;
            BackLoad = orderGetDTO.BackLoad;
            MiddleLoad = orderGetDTO.MiddleLoad;
            FrontLoad = orderGetDTO.FrontLoad;
            TotalLoad = orderGetDTO.TotalLoad;
            TotalPrice = orderGetDTO.TotalPrice;
            PaymentDay = orderGetDTO.PaymentDay;
            Observation = orderGetDTO.Observation;
        }
    }
}
