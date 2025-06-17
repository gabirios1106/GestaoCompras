using Microsoft.EntityFrameworkCore;

namespace GestaoCompras.API.Utils.ErrorHandler
{
    public class DbConcurrencyException : DbUpdateConcurrencyException
    {
        public DbConcurrencyException(string message) : base(message) { }
    }

}
