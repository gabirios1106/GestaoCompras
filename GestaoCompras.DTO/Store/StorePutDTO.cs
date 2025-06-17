using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.DTO.Store
{
    public class StorePutDTO
    {

        [Display(Name = "Id")]
        [Range(1, Int32.MaxValue, ErrorMessage = "O campo {0} deve ser preenchido")]
        public int Id { get; set; }

        [Display(Name = "Nome da fruta")]
        [Required(ErrorMessage = "O campo {0} é obrigatorio")]
        [StringLength(250, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres", MinimumLength = 1)]
        [MaxLength(250)]
        public string Name { get; set; }

        public StorePutDTO(){}
    }
}
