using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.DTO.Access
{
    public class UserLoginPostDTO
    {
        [Display(Name = "Login")]
        [Required(ErrorMessage = "O campo {0} precisa ser informado")]
        [EmailAddress(ErrorMessage = "Formato de {0} é inválido")]
        public string UserName { get; set; }

        [Display(Name = "Senha")]
        [Required(ErrorMessage = "O campo {0} precisa ser informado")]
        [DataType(DataType.Password)]
        public string PasswordHash { get; set; }

        public string ReCaptchaToken { get; set; }
        public DateTime AccessAttemptDate { get; set; }

        public UserLoginPostDTO() { }

    }
}
