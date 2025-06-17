using Microsoft.JSInterop;

namespace GestaoCompras.Web.Extensions;

public static class IJSRuntimeExtension
{
    public static ValueTask<string> GetGoogleReCaptchaToken(this IJSRuntime js) =>
        js.InvokeAsync<string>("runCaptcha");

    public static ValueTask<string> SaveAsFile(this IJSRuntime js, string fileName, string base64) =>
        js.InvokeAsync<string>("saveAsFile", fileName, base64);

    public static ValueTask<string> PDFViewer(this IJSRuntime js, string base64) =>
        js.InvokeAsync<string>("pdfViewer", base64);

    #region SessionStorage
    public static ValueTask<string> SetItemInSessionStorage(this IJSRuntime js, string key, string value) =>
        js.InvokeAsync<string>("sessionStorage.setItem", key, value);

    public static ValueTask<string> GetItemFromSessionStorage(this IJSRuntime js, string key) =>
        js.InvokeAsync<string>("sessionStorage.getItem", key);

    public static ValueTask<string> ClearSessionStorage(this IJSRuntime js) =>
        js.InvokeAsync<string>("sessionStorage.clear");
    #endregion SessionStorage

    #region LocalStorage
    public static ValueTask<string> SetItemInLocalStorage(this IJSRuntime js, string key, string value) =>
        js.InvokeAsync<string>("localStorage.setItem", key, value);

    public static ValueTask<string> GetItemFromLocalStorage(this IJSRuntime js, string key) =>
        js.InvokeAsync<string>("localStorage.getItem", key);

    public static ValueTask<string> ClearLocalStorage(this IJSRuntime js) =>
        js.InvokeAsync<string>("localStorage.clear");
    #endregion LocalStorage
}
