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
    public class OrderPostDTO
    {
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

        public int UserDataId { get; set; }


        [Display(Name = "Preço unitário")]
        [Range(0, Double.MaxValue, ErrorMessage = "O campo {0} não pode ser preenchido com um valor negativo")]
        public double UnitPrice { get; set; }
        public DateTime? PaymentDay { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime OrderDate { get; set; }

        public virtual ICollection<OrderItemPostDTO> OrderItemsPostDTO { get; set; } = [];

        public OrderPostDTO()
        {
            CreatedAt = DateTime.UtcNow;
            OrderDate = DateTime.Parse(DateTime.Now.ToShortDateString());

            StatusOrder = StatusOrder.A_PAGAR;
        }

        public void RecalculateTotalPrice()
        {
            foreach (var item in OrderItemsPostDTO)
            {
                item.CalculateTotalLoad(UnitPrice);
            }
        }

        public void SetUserDataId(int userDataId) => UserDataId = userDataId;
    }
}
