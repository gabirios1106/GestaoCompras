using GestaoCompras.DTO.Fruit;
using GestaoCompras.DTO.Supplier;
using GestaoCompras.Enums.General;
using GestaoCompras.Web.Interfaces.Supplier;
using Microsoft.AspNetCore.Components;

namespace GestaoCompras.Web.Pages.Supplier
{
    public class SupplierSelectComponentBase : ComponentBase
    {
        [Inject] ISupplierService SupplierService { get; set; }

        [Parameter] public string SupplierIdSelected { get; set; }
        [Parameter] public string NewSupplierIdSelected { get; set; }
        [Parameter] public bool Disabled { get; set; }
        [Parameter] public bool ReadOnly { get; set; }
        [Parameter] public bool Clearable { get; set; }
        [Parameter] public int MaxItems { get; set; }
        [Parameter] public EventCallback<int> ChangeSupplierSelected { get; set; }
        [Parameter] public EventCallback ReloadPage { get; set; }


        protected List<SupplierGetDTO> SuppliersGetDTO { get; set; }

        protected bool PageIsReady { get; set; }
        protected int ItemsInSuppliers { get; set; }

        private string _actualNewSupplierIdSelectedParameter;

        protected override async Task OnInitializedAsync()
        {
            _actualNewSupplierIdSelectedParameter = NewSupplierIdSelected;
            await GetSupplierAsync();

            ItemsInSuppliers = SuppliersGetDTO.Count;
            MaxItems = ItemsInSuppliers;

            PageIsReady = true;
        }

        protected async Task <IEnumerable<string>> Search(string value, CancellationToken token)
        {
            var supplier = SuppliersGetDTO.Select(s => s.Name);

            Console.WriteLine(ItemsInSuppliers);

            if (string.IsNullOrEmpty(value))
                return supplier;

            return supplier.Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase));
        }

        protected override async Task OnParametersSetAsync()
        {
            if (NewSupplierIdSelected != "0" && NewSupplierIdSelected != _actualNewSupplierIdSelectedParameter)
            {
                await GetSupplierAsync();
                SupplierIdSelected = NewSupplierIdSelected;

                StateHasChanged();
            }
        }

        protected async Task GetSupplierAsync()
        {
            SuppliersGetDTO = await SupplierService.GetSuppliersAsync($"Suppliers?orderBy=ASC-NAME");
            PageIsReady = true;
        }

        protected async Task OnChangeSupplier(string selectedSupplierName)
        {

            if (string.IsNullOrEmpty(selectedSupplierName))
                SupplierIdSelected = string.Empty;

            if (!string.IsNullOrEmpty(selectedSupplierName) && ChangeSupplierSelected.HasDelegate)
            {
                var supplierIdSelected = SuppliersGetDTO.FirstOrDefault(f => f.Name == selectedSupplierName).Id;
                SupplierIdSelected = supplierIdSelected.ToString();
                await ChangeSupplierSelected.InvokeAsync(supplierIdSelected);
            }
        }
    }
}
