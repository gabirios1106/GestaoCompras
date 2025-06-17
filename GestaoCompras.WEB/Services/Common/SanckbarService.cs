using GestaoCompras.Web.Interfaces.Common;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GestaoCompras.Web.Services.Common;

public class SnackbarService(ISnackbar snackbar) : ISnackbarService
{
    private readonly ISnackbar _snackbar = snackbar;

#nullable enable
    public void ShowSnackbar(string title, string message, Severity severity, int timeout = 2500, string? tip = null)
    {
        var markupMsg = GetRenderFragment(title, message, tip);

        _snackbar.Add(markupMsg, severity, config => { config.VisibleStateDuration = timeout; });
    }

    private MarkupString GetRenderFragment(string title, string message, string? tip) =>
        new($"<strong>{title}</strong><br>{message}{(!string.IsNullOrEmpty(tip) ? $"<br><br><small>{tip}</small>" : string.Empty)}");
}



