using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.DTO.Supplier
{
    public class SupplierPostDTO
    {
        public int UserDataId { get; set; }

        [Display(Name = "Nome do fornecedor")]
        [Required(ErrorMessage = "O campo {0} é obrigatorio")]
        [StringLength(250, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres", MinimumLength = 1)]
        [MaxLength(250)]
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }

        public SupplierPostDTO() => CreatedAt = DateTime.UtcNow;

        public void FormatName() => Name = Name.ToUpper();

        public void SetUserDataId(int userDataId) => UserDataId = userDataId;

    }
}
