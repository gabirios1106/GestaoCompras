using GestaoCompras.Utils.Classes;

namespace GestaoCompras.API.Interfaces.Access
{
    public interface IGoogleReCaptchaService
    {
        Task<GoogleReCaptchaResponse> GetGoogleReCaptchaVerify(string requestUri, string token, string secretKey);
    }
}
