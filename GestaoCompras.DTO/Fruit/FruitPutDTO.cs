using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.DTO.Fruit
{
    public class FruitPutDTO
    {
        [Display(Name = "Id")]
        [Range(1, Int32.MaxValue, ErrorMessage = "O campo {0} deve ser preenchido")]
        public int Id { get; set; }

        [Display(Name = "Preço unitario")]
        [Range(0.01, Double.MaxValue, ErrorMessage = "O campo {0} deve ser preenchido com um valor maior que zero")]
        public double Price { get; set; }

        public FruitPutDTO(){}
    }
}
