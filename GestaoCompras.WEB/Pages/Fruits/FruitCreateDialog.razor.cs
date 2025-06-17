using GestaoCompras.DTO.Fruit;
using GestaoCompras.Web.Interfaces.Common;
using GestaoCompras.Web.Interfaces.Fruits;
using GestaoCompras.Web.Utils.Classes;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GestaoCompras.Web.Pages.Fruits
{
    public class FruitCreateDialogBase : ComponentBase
    {
        [Inject] ISnackbarService SnackbarService { get; set; }
        [Inject] IFruitService FruitService { get; set; }
        [Inject] ILogger<FruitCreateDialogBase> Logger { get; set; }

        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public EventCallback<int> GetFruitsAsync { get; set; }

        protected FruitPostDTO FruitPostDTO { get; set; } = new();
        protected CrudControl<FruitPostDTO> CrudControl { get; set; } = new CrudControl<FruitPostDTO>();

        protected override void OnInitialized()
        {
            FruitPostDTO.Price = 0.01;
            CrudControl.PageIsReady = true;
        }

        protected async void CreateFruitAsync()
        {
            CrudControl.PageIsReady = false;

            var fruitGetDTO = await FruitService.CreateWithGetObjectAsync("Fruits", FruitPostDTO);

            if (fruitGetDTO is not null)
            {
                SnackbarService.ShowSnackbar("Sucesso", "Fruta cadastrada com sucesso", Severity.Success, 2500);
                Logger.LogInformation("Fruta cadastrada com sucesso");

                await GetFruitsAsync.InvokeAsync(fruitGetDTO.Id);
                StateHasChanged();  
                MudDialog.Close();
            }

            CrudControl.PageIsReady = true;
            StateHasChanged();
        }

        protected void Cancel() => MudDialog.Cancel();
    }
}
