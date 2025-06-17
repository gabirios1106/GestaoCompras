using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace GestaoCompras.Web.Layout
{
    public class SearchByIdFieldBase : ComponentBase
    {
        [Parameter] public EventCallback<string> SearchAction { get; set; }
        [Parameter] public string PlaceHolderMessage { get; set; }
        [Parameter] public string HelperMessage { get; set; }
        [Parameter] public bool Disabled { get; set; }
        [Parameter] public string SearchIdValue { get; set; }
        [Parameter] public bool Autofocus { get; set; }

        protected MudNumericField<string> FieldSearchId { get; set; }

        private bool _previousAutoFocus { get; set; }

        protected override void OnInitialized() => _previousAutoFocus = Autofocus;

        protected override async Task OnParametersSetAsync()
        {
            if (_previousAutoFocus != Autofocus && Autofocus)
            {
                _previousAutoFocus = Autofocus;
                await FieldSearchId.FocusAsync();
            }
        }

        protected async Task OnClickSearchAsync() => await SearchAction.InvokeAsync(SearchIdValue);

        protected async Task OnKeyUpSearchAsync(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
                await OnClickSearchAsync();
        }

    }
}
