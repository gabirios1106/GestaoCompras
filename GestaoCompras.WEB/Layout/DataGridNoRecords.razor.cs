using Microsoft.AspNetCore.Components;

namespace GestaoCompras.Web.Layout
{
    public class DataGridNoRecordsBase : ComponentBase
    {
        [Parameter] public EventCallback ClearSearch { get; set; } = EventCallback.Empty;

        protected async Task OnClearSearchAsync() => await ClearSearch.InvokeAsync();
    }
}
