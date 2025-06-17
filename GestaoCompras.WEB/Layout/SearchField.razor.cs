using GestaoCompras.Web.Utils.Classes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace GestaoCompras.Web.Layout
{
    public class SearchFieldBase : ComponentBase
    {
        [Parameter] public EventCallback<string> SearchAction { get; set; }
        [Parameter] public EventCallback ReloadPage { get; set; }
        [Parameter] public string PlaceHolderMessage { get; set; }
        [Parameter] public bool Disabled { get; set; }
        [Parameter] public string SearchValue { get; set; }
        [Parameter] public bool Autofocus { get; set; }

        protected MudTextField<string> FieldSearch { get; set; }

        private bool _previousAutoFocus { get; set; }

        protected override void OnInitialized() => _previousAutoFocus = Autofocus;

        protected override async Task OnParametersSetAsync()
        {
            if (_previousAutoFocus != Autofocus && Autofocus)
            {
                _previousAutoFocus = Autofocus;
                await FieldSearch.FocusAsync();
            }
        }

        protected async Task OnChangeSearchValue(string newSearchValue)
        {
            if (string.IsNullOrEmpty(newSearchValue)) {
                SearchValue = string.Empty;

                await ReloadPage.InvokeAsync();
            }
        }

        protected async Task OnClickSearchAsync() {
            await SearchAction.InvokeAsync(SearchValue);
        }

        protected async Task OnKeyUpSearchAsync(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
                await OnClickSearchAsync();
        }
    }
}
