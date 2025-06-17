namespace GestaoCompras.API.Utils.ErrorHandler
{
    public class DbForeignKeyException : Exception
    {
        public DbForeignKeyException(string source, string message) : base(message)
        {
            base.Source = source;
        }
    }

}
