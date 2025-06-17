using GestaoCompras.DTO.Supplier;
using GestaoCompras.Web.Interfaces.Common;
using GestaoCompras.Web.Interfaces.Supplier;
using GestaoCompras.Web.Utils.Classes;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GestaoCompras.Web.Pages.Supplier
{
    public class SupplierCreateDialogBase : ComponentBase
    {
        [Inject] ISnackbarService SnackbarService { get; set; }
        [Inject] ISupplierService SupplierService { get; set; }
        [Inject] ILogger<SupplierCreateDialogBase> Logger { get; set; }

        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public EventCallback<int> GetSuppliersAsync { get; set; }

        protected SupplierPostDTO SupplierPostDTO { get; set; } = new();
        protected CrudControl<SupplierPostDTO> CrudControl { get; set; } = new CrudControl<SupplierPostDTO>();

        protected override void OnInitialized()
        {
            CrudControl.PageIsReady = true;
        }

        protected async void CreateSupplierAsync()
        {
            CrudControl.PageIsReady = false;
                
            var supplierGetDTO = await SupplierService.CreateWithGetObjectAsync("Suppliers", SupplierPostDTO);

            if (supplierGetDTO is not null)
            {
                SnackbarService.ShowSnackbar("Sucesso", "Fornecedor cadastrado com sucesso", Severity.Success, 2500);
                Logger.LogInformation("Fornecedor cadastrado com sucesso");

                await GetSuppliersAsync.InvokeAsync(supplierGetDTO.Id);
                StateHasChanged();
                MudDialog.Close();
            }

            CrudControl.PageIsReady = true;
            StateHasChanged();
        }

        protected void Cancel() => MudDialog.Cancel();
    }
}
