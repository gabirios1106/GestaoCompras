using GestaoCompras.Enums.General;
using Microsoft.AspNetCore.Components;

namespace GestaoCompras.Web.Layout
{
    public class CrudOptionsBase<TItem> : ComponentBase
    {
        [Parameter] public string ItemStatus { get; set; }
        [Parameter] public TItem Item { get; set; }
        [Parameter] public EventCallback ShowUpdateDialog { get; set; }
        [Parameter] public EventCallback ShowConfirmationDelete { get; set; }
        [Parameter] public EventCallback Activate { get; set; }
        [Parameter] public EventCallback ShowItemDetail { get; set; }
        [Parameter] public bool DetailButton { get; set; }
        [Parameter] public string DetailButtonRoute { get; set; }
        [Parameter] public bool EditButton { get; set; } = true;
        [Parameter] public bool DeleteButton { get; set; } = true;
        [Parameter] public bool DisabledButtons { get; set; } = false;

        protected bool ItemActive(string statusItem) =>
            statusItem == Status.ATIVO.ToString();

        protected async Task OnClickShowUpdateDialog() => await ShowUpdateDialog.InvokeAsync();

        protected async Task OnClickShowConfirmationDelete() => await ShowConfirmationDelete.InvokeAsync();
    }
}
