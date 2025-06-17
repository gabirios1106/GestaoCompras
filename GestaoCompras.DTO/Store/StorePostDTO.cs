using GestaoCompras.Enums.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.DTO.Store
{
    public class StorePostDTO
    {
        public int UserDataId { get; set; }

        [Display(Name = "Nome da Loja")]
        [Required(ErrorMessage = "O campo {0} é obrigatorio")]
        [StringLength(250, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres", MinimumLength = 1)]
        [MaxLength(250)]
        public string Name { get; set; }

        public Status Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public StorePostDTO()
        {
            CreatedAt = DateTime.UtcNow;
            Status = Status.ATIVO;
        }

        public void SetUserDataId(int userDataId) => UserDataId = userDataId;   
    }
}
