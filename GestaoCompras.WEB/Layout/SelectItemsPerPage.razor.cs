using Microsoft.AspNetCore.Components;

namespace GestaoCompras.Web.Layout
{
    public class SelectItemsPerPageBase : ComponentBase
    {
        [Parameter] public bool SmallScale { get; set; } = false;
        [Parameter] public bool Disabled { get; set; }
        [Parameter] public EventCallback<int> ChangeItemsPerPage { get; set; }

        protected int[] SelectScale;
        protected int SelectedValue;

        protected override void OnInitialized()
        {
            SelectScale = SmallScale ? [5, 10, 20] : [25, 50, 100];
            SelectedValue = SelectScale[2];

            base.OnInitialized();
        }

        protected async Task OnChangeItemsPerPageAsync(int selectedValue)
        {
            if (ChangeItemsPerPage.HasDelegate)
            {
                SelectedValue = selectedValue;
                await ChangeItemsPerPage.InvokeAsync(selectedValue);
            }
        }
    }
}
