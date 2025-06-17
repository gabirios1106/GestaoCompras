using GestaoCompras.Enums.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.DTO.Users
{
    public class RegisterUserPostDTO
    {
        [Display(Name = "Código")]
        [Required(ErrorMessage = "O campo {0} deve ser preenchido")]
        public int Id { get; set; }

        [Display(Name = "Cargo do funcionario")]
        [Required(ErrorMessage = "O campo {0} deve ser preenchido")]
        public UserRole UserRole { get; set; }

        [Display(Name = "Nome")]
        [Required(ErrorMessage = "O campo {0} deve ser preenchido")]
        [MaxLength(300)]
        [StringLength(300, ErrorMessage = "O campo {0} deve ter {1} caracteres", MinimumLength = 4)]
        public string Name { get; set; }

        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "O campo {0} deve ser preenchido")]
        [EmailAddress]
        [StringLength(150, ErrorMessage = "O campo {0} deve ter no mínimo {2} e no máximo {1} caracteres", MinimumLength = 8)]
        public string Email { get; set; }

        [Display(Name = "Senha")]
        [Required(ErrorMessage = "O campo {0} deve ser preenchido")]
        [DataType(DataType.Password)]
        [StringLength(14, ErrorMessage = "O campo {0} deve ter no mínimo {2} e no máximo {1} caracteres", MinimumLength = 8)]
        public string PasswordHash { get; set; }

        [Display(Name = "Confirmar senha")]
        [Required(ErrorMessage = "O campo {0} deve ser preenchido")]
        [DataType(DataType.Password)]
        [Compare("PasswordHash", ErrorMessage = "As senhas devem ser iguais")]
        public string ConfirmPassword { get; set; }

    }
}
