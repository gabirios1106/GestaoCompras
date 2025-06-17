using MudBlazor;

namespace GestaoCompras.Web.Interfaces.Common
{
    public interface ISnackbarService
    {
#nullable enable
        void ShowSnackbar(string title, string message, Severity severity, int timeout = 2500, string? tip = null);
    }
}
