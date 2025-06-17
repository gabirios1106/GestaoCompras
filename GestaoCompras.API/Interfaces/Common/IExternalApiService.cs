using GestaoCompras.Utils.Classes;

namespace GestaoCompras.API.Interfaces.Common
{
    public interface IExternalApiService
    {
        Task<GoogleReCaptchaResponse> GetGoogleReCaptchaVerify(string requestUri, string token, string secretKey);
    }
}
