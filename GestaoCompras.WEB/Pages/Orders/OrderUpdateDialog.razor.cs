using GestaoCompras.DTO.Order;
using GestaoCompras.Enums.Orders;
using GestaoCompras.Enums.Users;
using GestaoCompras.Web.Interfaces.Access;
using GestaoCompras.Web.Interfaces.Common;
using GestaoCompras.Web.Interfaces.Orders;
using GestaoCompras.Web.Pages.Fruits;
using GestaoCompras.Web.Pages.Supplier;
using GestaoCompras.Web.Utils.Classes;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;

namespace GestaoCompras.Web.Pages.Orders
{
    public class OrderUpdateDialogBase : ComponentBase
    {
        [Inject] ISnackbarService SnackbarService { get; set; }
        [Inject] IOrderService OrderService { get; set; }
        [Inject] ISessionDataService SessionDataService { get; set; }   

        [Inject] IDialogService DialogService { get; set; }
        [Inject] ILogger<OrderUpdateDialogBase> Logger { get; set; }

        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public EventCallback<bool> GetOrdersAsync { get; set; }

        [Parameter] public OrderPutDTO OrderPutDTO { get; set; } = new();

        protected CrudControl<OrderPutDTO> CrudControl { get; set; } = new CrudControl<OrderPutDTO>();

        protected string TotalPrice { get; set; }
        protected int TotalLoad { get; set; }
        protected bool PaymentMethod { get; set; } 
        protected int NewFruitIdSelected { get; set; }
        protected int NewSupplierIdSelected { get; set; }
        protected UserRole UserRole { get; set; }


        protected CultureInfo CultureInfoPtBR { get; set; } = new CultureInfo("pt-BR");

        protected override async Task OnInitializedAsync()
        {
            CrudControl.PageIsReady = true;

            UserRole = await SessionDataService.GetSavedUserRoleAsync();
        }

        protected async void UpdateOrderAsync()
        {
            CrudControl.PageIsReady = false;

            if (PaymentMethod)
            {
                OrderPutDTO.StatusOrder = StatusOrder.BOLETO_CHEQUE;
                OrderPutDTO.WasAlreadyTicket = true;
            }

            var createSuccessfull = await OrderService.UpdateAsync("Orders", OrderPutDTO);

            if (createSuccessfull)
            {
                SnackbarService.ShowSnackbar("Sucesso", "Pedido editado com sucesso", Severity.Success, 2500);
                Logger.LogInformation("Pedido editado com sucesso");

                await GetOrdersAsync.InvokeAsync();
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

        protected void ChangeFruitSelected(int fruitId) => OrderPutDTO.FruitId = fruitId;

        protected void ChangeUnitPriceSelected(double unitPrice) => OrderPutDTO.UnitPrice = unitPrice;

        protected void ChangeSupplierSelected(int supplierId) => OrderPutDTO.SupplierId = supplierId;

        protected void CalculateTotalLoad()
        {
            TotalLoad = OrderPutDTO.BackLoad + OrderPutDTO.MiddleLoad + OrderPutDTO.FrontLoad;
            CalculateTotalPrice();
            OrderPutDTO.TotalLoad = TotalLoad;
        }

        protected void CalculateTotalPrice()
        {
            var totalPrice = TotalLoad * OrderPutDTO.UnitPrice;
            TotalPrice = totalPrice.ToString("C", CultureInfoPtBR.NumberFormat);
            OrderPutDTO.TotalPrice = totalPrice;
        }

        protected void Cancel() => MudDialog.Cancel();

    }
}

