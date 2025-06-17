using System.ComponentModel.DataAnnotations;

namespace GestaoCompras.DTO.Order
{
    public class OrderItemPostDTO
    {
        [Display(Name = "Carga de trás")]
        [Range(0, Double.MaxValue, ErrorMessage = "O campo {0} não pode ser preenchido com um valor negativo")]
        public int BackLoad { get; set; }

        [Display(Name = "Carga do meio")]
        [Range(0, Double.MaxValue, ErrorMessage = "O campo {0} não pode ser preenchido com um valor negativo")]
        public int MiddleLoad { get; set; }

        [Display(Name = "Carga da frente")]
        [Range(0, Double.MaxValue, ErrorMessage = "O campo {0} não pode ser preenchido com um valor negativo")]
        public int FrontLoad { get; set; }

        [Display(Name = "Loja")]
        [Range(1, Int32.MaxValue, ErrorMessage = "O campo {0} deve ser preenchido")]
        public int? StoreId { get; set; }

        public int TotalLoad { get; set; }

        public double TotalPrice { get; set; }

        public string? Observation { get; set; }

        public OrderItemPostDTO() { }

        public OrderItemPostDTO(int storeId) =>
            StoreId = storeId;

        public void CalculateTotalLoad(double unitPrice)
        {
            TotalLoad = BackLoad + MiddleLoad + FrontLoad;
            CalculateTotalPrice(unitPrice);
        }

        public void CalculateTotalPrice(double unitPrice)
        {
            TotalPrice = TotalLoad * unitPrice;
        }
    }
}
