using Microsoft.AspNetCore.Components;

namespace GestaoCompras.Web.Layout
{
    public class ButtonNewBase : ComponentBase
    {
        [Parameter] public string Text { get; set; }
        [Parameter] public EventCallback OnClick { get; set; }

        protected async Task ButtonNewClick() => await OnClick.InvokeAsync();
    }
}
