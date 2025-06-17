using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.Enums.Users;

public enum UserRole
{
    [Description("Funcionário")]
    Funcionario = 0,

    [Description("Administrador")]
    Administrador = 1
}
