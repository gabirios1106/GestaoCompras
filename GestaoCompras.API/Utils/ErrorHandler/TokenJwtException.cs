namespace GestaoCompras.API.Utils.ErrorHandler
{
    public class TokenJwtException : ApplicationException
    {
        public TokenJwtException(string message) : base(message) { }
    }
}
