using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.Utils.Validators;

public static class CustomValidator
{
    public static bool IsFkException(string innerException)
    {
        if (innerException.Contains("Npgsql.PostgresException", StringComparison.CurrentCultureIgnoreCase) && innerException.Contains("update or delete on table", StringComparison.CurrentCultureIgnoreCase) && innerException.Contains("violates foreign key constraint", StringComparison.CurrentCultureIgnoreCase))
            return true;
        else
            return false;
    }
}
