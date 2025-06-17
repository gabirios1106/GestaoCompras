using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.DTO.Fruit
{
    public class FruitPostDTO
    {
        public int UserDataId { get; set; }

        [Display(Name = "Nome da fruta")]
        [Required(ErrorMessage = "O campo {0} é obrigatorio")]
        [StringLength(250, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres", MinimumLength = 1)]
        [MaxLength(250)]
        public string Name { get; set; }

        [Display(Name = "Preço unitario")]
        [Range(0.01, Double.MaxValue, ErrorMessage = "O campo {0} deve ser preenchido com um valor maior que zero")]
        public double Price { get; set; }

        public DateTime CreatedAt { get; set; }

        public FruitPostDTO() => CreatedAt = DateTime.UtcNow;

        public void FormatName() => Name = Name.ToUpper();

        public void SetUserDataId(int userDataId) => UserDataId = userDataId;
    }
}
