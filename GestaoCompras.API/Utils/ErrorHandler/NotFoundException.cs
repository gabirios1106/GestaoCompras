namespace GestaoCompras.API.Utils.ErrorHandler
{
    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string message) : base(message) { }
    }
}
