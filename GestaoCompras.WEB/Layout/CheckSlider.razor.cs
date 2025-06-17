using Microsoft.AspNetCore.Components;

namespace GestaoCompras.Web.Layout
{
    public class CheckSliderBase : ComponentBase
    {
        [Parameter] public bool Checked { get; set; }
        [Parameter] public bool Disabled { get; set; }
        [Parameter] public EventCallback ChangeInactiveOption { get; set; }

        protected async Task OnChangeCheckSliderAsync() => await ChangeInactiveOption.InvokeAsync(null);
    }
}
