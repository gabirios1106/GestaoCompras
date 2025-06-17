namespace GestaoCompras.Web.Interfaces.Common
{
    public interface IErrorMessageService
    {
        void ShowErrorMessage(string routeAPI, string errorMessage, string baseAddress, int timeout = 3500);
    }
}
