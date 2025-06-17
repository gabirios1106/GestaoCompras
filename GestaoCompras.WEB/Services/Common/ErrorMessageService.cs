using GestaoCompras.Web.Interfaces.Common;
using MudBlazor;

namespace GestaoCompras.Web.Services.Common;

public class ErrorMessageService(ISnackbarService snackbarService) : IErrorMessageService
{
    private readonly ISnackbarService _snackbarService = snackbarService;

    public void ShowErrorMessage(string routeAPI, string errorMessage, string baseAddress, int timeout = 3500)
    {
        routeAPI = baseAddress + routeAPI;
        _snackbarService.ShowSnackbar("Erro", "A operação falhou", Severity.Error, timeout, "Pressione F12 para maiores detalhes");
    }
}


