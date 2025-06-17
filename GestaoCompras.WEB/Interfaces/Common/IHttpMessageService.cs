using System.Net;

namespace GestaoCompras.Web.Interfaces.Common
{
    public interface IHttpMessageService
    {
        void ShowMessage(HttpStatusCode httpStatusCode, string customMessage, int timeout, string baseAddress, string routeAPI = null, string reasonPhrase = null, string responseContent = null);
    }
}
