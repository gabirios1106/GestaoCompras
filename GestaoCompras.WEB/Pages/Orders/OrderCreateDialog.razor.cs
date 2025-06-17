using GestaoCompras.DTO.Order;
using GestaoCompras.DTO.Store;
using GestaoCompras.Enums.Orders;
using GestaoCompras.Enums.Users;
using GestaoCompras.Web.Interfaces.Access;
using GestaoCompras.Web.Interfaces.Common;
using GestaoCompras.Web.Interfaces.Orders;
using GestaoCompras.Web.Interfaces.Stores;
using GestaoCompras.Web.Pages.Fruits;
using GestaoCompras.Web.Pages.Supplier;
using GestaoCompras.Web.Utils.Classes;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;

namespace GestaoCompras.Web.Pages.Orders
{
    public class OrderCreateDialogBase : ComponentBase
    {
        [Inject] ISnackbarService SnackbarService { get; set; }
        [Inject] IOrderService OrderService { get; set; }
        [Inject] IStoreService StoreService { get; set; }
        [Inject] IDialogService DialogService { get; set; }
        [Inject] ILogger<OrderCreateDialogBase> Logger { get; set; }
        [Inject] ISessionDataService SessionDataService { get; set; }

        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public EventCallback<bool> GetOrdersAsync { get; set; }

        protected OrderPostDTO OrderPostDTO { get; set; } = new();

        protected List<StoreGetDTO> StoresGetDTO { get; set; } = [];
        protected CrudControl<OrderPostDTO> CrudControl { get; set; } = new CrudControl<OrderPostDTO>();

        protected int NewFruitIdSelected { get; set; }
        protected int NewSupplierIdSelected { get; set; }
        protected bool PaymentMethod { get; set; }
        protected UserRole UserRole { get; set; }


        protected CultureInfo CultureInfoPtBR { get; set; } = new CultureInfo("pt-BR");

        protected override async Task OnInitializedAsync()
        {
            await GetStoresAsync();
            UserRole = await SessionDataService.GetSavedUserRoleAsync();
        }

        protected async Task GetStoresAsync()
        {
            CrudControl.PageIsReady = false;

            StoresGetDTO = await StoreService.GetStoresAsync("Stores?orderBy=ASC-ID");

            if (StoresGetDTO.Count == 0)
            {
                SnackbarService.ShowSnackbar("Erro", "Nenhuma loja cadastrada", Severity.Error, 2500);
                Logger.LogInformation("Nenhuma loja cadastrada");

                StateHasChanged();
                MudDialog.Close();
            }
            else
            {
                foreach (var store in StoresGetDTO)
                {
                    var orderItem = new OrderItemPostDTO(store.Id);
                    OrderPostDTO.OrderItemsPostDTO.Add(orderItem);
                }
            }

            CrudControl.PageIsReady = true;
        }

        protected async void CreateOrderAsync()
        {
            CrudControl.PageIsReady = false;

            if (PaymentMethod)
            {
                OrderPostDTO.StatusOrder = StatusOrder.BOLETO_CHEQUE;
            }

            var createSuccessfull = await OrderService.CreateAsync("Orders", OrderPostDTO);

            if (createSuccessfull)
            {
                SnackbarService.ShowSnackbar("Sucesso", "Pedido cadastrado com sucesso", Severity.Success, 2500);
                Logger.LogInformation("Pedido cadastrado com sucesso");

                await GetOrdersAsync.InvokeAsync(true);
                StateHasChanged();
                MudDialog.Close();

            }

            CrudControl.PageIsReady = true;
            StateHasChanged();
        }

        protected Task ShowCreateFruitDialog()
        {
            Logger.LogInformation("Cadastrar fruta");

            if (!CrudControl.PageIsReady)
                return Task.CompletedTask;

            var dialogOptions = new DialogOptions
            {
                BackgroundClass = "blurry-dialog-class",
                CloseOnEscapeKey = true,
                CloseButton = true,
                MaxWidth = MaxWidth.ExtraSmall,
                FullWidth = true
            };

            var dialogParameters = new DialogParameters<FruitCreateDialog>
            {
                { "GetFruitsAsync", EventCallback.Factory.Create<int>(this, GetFruitsAsync) }
            };

            return DialogService.ShowAsync<FruitCreateDialog>("Cadastrar Fruta", dialogParameters, dialogOptions);
        }

        protected Task ShowCreateSupplierDialog()
        {
            Logger.LogInformation("Cadastrar fornecedor");

            if (!CrudControl.PageIsReady)
                return Task.CompletedTask;

            var dialogOptions = new DialogOptions
            {
                BackgroundClass = "blurry-dialog-class",
                CloseOnEscapeKey = true,
                CloseButton = true,
                MaxWidth = MaxWidth.ExtraSmall,
                FullWidth = true
            };

            var dialogParameters = new DialogParameters<FruitCreateDialog>
            {
                { "GetSuppliersAsync", EventCallback.Factory.Create<int>(this, GetSuppliersAsync) }
            };

            return DialogService.ShowAsync<SupplierCreateDialog>("Cadastrar Fornecedor", dialogParameters, dialogOptions);
        }

        protected void GetFruitsAsync(int fruitId)
        {
            NewFruitIdSelected = fruitId;
            ChangeFruitSelected(fruitId);
        }
        protected void GetSuppliersAsync(int supplierId)
        {
            NewSupplierIdSelected = supplierId;
            ChangeSupplierSelected(supplierId);
        }

        protected void ChangeFruitSelected(int fruitId) => OrderPostDTO.FruitId = fruitId;

        protected void ChangeUnitPriceSelected(double unitPrice)
        {
            OrderPostDTO.UnitPrice = unitPrice;
            OrderPostDTO.RecalculateTotalPrice();
        }

        protected void ChangeSupplierSelected(int supplierId) => OrderPostDTO.SupplierId = supplierId;

        protected void Cancel() => MudDialog.Cancel();

    }
}
