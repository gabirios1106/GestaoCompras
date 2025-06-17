using GestaoCompras.Enums.Users;
using GestaoCompras.Web.Interfaces.Access;
using GestaoCompras.Web.Services.Access;
using GestaoCompras.Web.Utils.Classes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace GestaoCompras.Web.Layout;

public class ConfirmationDialogBase : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }

    [Parameter] public string ConfirmMessage { get; set; } = "Tem certeza que deseja excluir?";
    [Parameter] public string AlertMessage { get; set; } = "Essa ação será irreversível!";
    [Parameter] public string CancelButtonText { get; set; } = "Cancelar";
    [Parameter] public Color CancelButtonColor { get; set; } = Color.Dark;
    [Parameter] public string ConfirmButtonText { get; set; } = "Confirmar";
    [Parameter] public Color ConfirmButtonColor { get; set; } = Color.Error;
    [Parameter] public EventCallback ConfirmAsync { get; set; }
    
    [Inject] ISessionDataService SessionDataService { get; set; }
    protected UserRole UserRole { get; set; }

    protected override async void OnInitialized()
    {
        UserRole = await SessionDataService.GetSavedUserRoleAsync();
    }

    protected async Task OnClickConfirmAsync()
    {
        await ConfirmAsync.InvokeAsync();
        MudDialog.Close();
    }

    protected void Cancel() => MudDialog.Cancel();
}