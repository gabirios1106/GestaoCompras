using GestaoCompras.DTO.Common;
using GestaoCompras.DTO.Order;
using GestaoCompras.Enums.Orders;
using GestaoCompras.Enums.Users;
using GestaoCompras.Web.Interfaces.Access;
using GestaoCompras.Web.Interfaces.Common;
using GestaoCompras.Web.Interfaces.Orders;
using GestaoCompras.Web.Layout;
using GestaoCompras.Web.Pages.Orders;
using GestaoCompras.Web.Services.Access;
using GestaoCompras.Web.Utils.Classes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;
using System.Globalization;

namespace GestaoCompras.Web.Pages.CheckAndTicket
{
    public class CheckAndTicketBase : ComponentBase
    {
        [Inject] ISnackbarService SnackbarService { get; set; }
        [Inject] IOrderService OrderService { get; set; }
        [Inject] IDialogService DialogService { get; set; }
        [Inject] ILogger<OrderBase> Logger { get; set; }
        [Inject] ISessionDataService SessionDataService { get; set; }
        [Inject] TokenService TokenService { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }

        private GetWithPaginationGetDTO<OrderGetDTO> _getWithPaginationGetDTO;

        protected List<OrderGetDTO> OrdersGetDTO { get; set; }
        protected OrderPutDTO OrderPutDTO { get; set; } = new();
        protected OrderGetDTO OrderGetDTO { get; set; }

        protected bool HideShowTotalPrice { get; set; } = true;
        protected double AllToPayTicket { get; set; }
        protected string AllToPayHiden { get; set; } = "R$--------";
        protected UserRole UserRole { get; set; }

        protected bool RefreshWeeks { get; set; }
        protected CrudControl<OrderGetDTO> CrudControl { get; set; } = new CrudControl<OrderGetDTO>();
        protected CultureInfo CultureInfoPtBR { get; set; } = new CultureInfo("pt-BR");
        protected bool Autofocus { get; set; }
        protected string ActiveWeekIdSelected { get; set; }


        protected override async void OnInitialized()
        {
            Logger.LogInformation("Inicialização da página de pedidos");

            UserRole = await SessionDataService.GetSavedUserRoleAsync();

            CrudControl.Ticket = true;

            var uri = new Uri(NavigationManager.Uri);
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

            if (query.TryGetValue("searchValue", out var searchValue))
            {
                CrudControl.SearchValue = searchValue;
            }

            await GetOrdersAsync();

            CrudControl.Initialize(EventCallback.Factory.Create<bool>(this, GetOrdersAsync), true);

            StateHasChanged();
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                await TokenService.GetAuthenticationStateAsync();
                Autofocus = true;
            }
        }

        protected async Task GetOrdersAsync(bool refreshWeeks = false)
        {
            Logger.LogInformation("Buscar pedidos");

            CrudControl.PageIsReady = false;
            StateHasChanged();
            OrderGetDTO = null;

            var initialDate = string.Empty;
            var finalDate = string.Empty;

            if (CrudControl.InitialDate.HasValue)
                initialDate = DateTime.TryParse(CrudControl.InitialDate.ToString(), out DateTime date) ? date.ToString("yyyy-MM-ddT00:00:00Z") : DateTime.MinValue.ToString("yyyy-MM-ddTHH:mm:ssZ");

            if (CrudControl.FinalDate.HasValue)
                finalDate = DateTime.TryParse(CrudControl.FinalDate.ToString(), out DateTime date) ? date.ToString("yyyy-MM-ddT23:59:59Z") : DateTime.MaxValue.ToString("yyyy-MM-ddTHH:mm:ssZ");

            if (!CrudControl.InitialDate.HasValue && !CrudControl.FinalDate.HasValue)
                ActiveWeekIdSelected = string.Empty;

            _getWithPaginationGetDTO = await OrderService.GetOrdersAsync($"Orders?showInactive={CrudControl.ShowInactive}&ticket={CrudControl.Ticket}&searchValue={CrudControl.SearchValue}&orderId={CrudControl.SearchIdValue}&skip={CrudControl.Skip}&take={CrudControl.ItemsPerPage}&orderBy={CrudControl.OrderBy}&initialDate={initialDate}&finalDate={finalDate}");

            if (_getWithPaginationGetDTO != null)
            {
                Logger.LogInformation("Definindo paginação dos pedidos");

                OrdersGetDTO = _getWithPaginationGetDTO.ObjectClass;

                AllToPayTicket = _getWithPaginationGetDTO.AllToPayTicket;

                CrudControl.SetPagination(_getWithPaginationGetDTO.TotalPages, _getWithPaginationGetDTO.TotalRegs);
            }

            if (refreshWeeks)
                RefreshWeeks = true;

            CrudControl.PageIsReady = true;
            StateHasChanged();
        }

        protected Task ShowCreateDialog()
        {
            Logger.LogInformation("Cadastrar pedido");

            if (!CrudControl.PageIsReady)
                return Task.CompletedTask;

            var dialogOptions = new DialogOptions
            {
                BackgroundClass = "blurry-dialog-class",
                CloseOnEscapeKey = true,
                CloseButton = true,
                MaxWidth = MaxWidth.Large,
                FullWidth = true
            };

            var dialogParameters = new DialogParameters<OrderCreateDialog>
            {
                { "GetOrdersAsync", EventCallback.Factory.Create<bool>(this, GetOrdersAsync) }
            };

            return DialogService.ShowAsync<OrderCreateDialog>("Cadastrar Pedido", dialogParameters, dialogOptions);
        }

        protected async void ChangeOrderStatus(OrderGetDTO orderGetDTO)
        {
            OrderPutDTO = new OrderPutDTO(orderGetDTO);

            if (OrderPutDTO.StatusOrder == StatusOrder.BOLETO_CHEQUE || OrderPutDTO.StatusOrder == StatusOrder.A_PAGAR)
            {
                OrderPutDTO.StatusOrder = StatusOrder.PAGO;
            }
            if (orderGetDTO.WasAlreadyTicket && orderGetDTO.StatusOrder == StatusOrder.PAGO)
            {
                OrderPutDTO.StatusOrder = StatusOrder.BOLETO_CHEQUE;
            }
            if (orderGetDTO.WasAlreadyTicket && orderGetDTO.StatusOrder == StatusOrder.BOLETO_CHEQUE)
            {
                OrderPutDTO.StatusOrder = StatusOrder.PAGO;
            }

            var createSuccessfull = await OrderService.UpdateAsync("Orders", OrderPutDTO);

            if (createSuccessfull)
            {
                SnackbarService.ShowSnackbar("Sucesso", "Status do Pedido atualizado com sucesso", Severity.Success, 2500);
                Logger.LogInformation("Status do Pedido atualizado com sucesso");

                await GetOrdersAsync();
                StateHasChanged();
            }

            CrudControl.PageIsReady = true;
            StateHasChanged();

        }

        protected Task ShowUpdateDialog(OrderGetDTO orderGetDTO)
        {
            Logger.LogInformation("Atualizar pedido");

            if (!CrudControl.PageIsReady)
                return Task.CompletedTask;

            OrderPutDTO = new OrderPutDTO(orderGetDTO);

            var dialogOptions = new DialogOptions
            {
                BackgroundClass = "blurry-dialog-class",
                CloseOnEscapeKey = true,
                CloseButton = true,
                MaxWidth = MaxWidth.Large,
                FullWidth = true
            };

            var dialogParameters = new DialogParameters<OrderUpdateDialog>
            {
                { "GetOrdersAsync", EventCallback.Factory.Create<bool>(this, GetOrdersAsync) },
                { "OrderPutDTO", OrderPutDTO }
            };

            return DialogService.ShowAsync<OrderUpdateDialog>("Atualizar Pedido", dialogParameters, dialogOptions);
        }

        protected async void DeleteProductAsync()
        {
            OrdersGetDTO = null;

            var deleteSuccessfull = await OrderService.DeleteAsync($"Orders/{CrudControl.IdDelete}");

            if (deleteSuccessfull)
            {
                SnackbarService.ShowSnackbar("Sucesso", "Pedido excluido com sucesso", Severity.Success, 2500);
                Logger.LogInformation("Pedido excluido com sucesso");
            }

            await GetOrdersAsync();
            StateHasChanged();
        }

        protected void ShowConfirmationDelete(int orderId)
        {
            Logger.LogInformation("Excluir produto");

            CrudControl.IdDelete = orderId;

            if (!CrudControl.PageIsReady)
                return;

            var dialogOptions = new DialogOptions
            {
                BackgroundClass = "blurry-dialog-class",
                CloseOnEscapeKey = true,
                CloseButton = true,
                MaxWidth = MaxWidth.ExtraSmall,
                FullWidth = true
            };

            var dialogParameters = new DialogParameters<ConfirmationDialog>
            {
                { "ConfirmAsync", EventCallback.Factory.Create(this, DeleteProductAsync) }
            };

            DialogService.ShowAsync<ConfirmationDialog>("Confirmar exclusão", dialogParameters, dialogOptions);
        }

        protected void ChangeHideShowTotalPrice()
        {
            HideShowTotalPrice = !HideShowTotalPrice;

        }

        protected void ChangeActiveWeekIdSelected(string activeWeekId) =>
            ActiveWeekIdSelected = activeWeekId;

        protected void ChangeInitialDateSelected(DateTime? initialDate) =>
            CrudControl.InitialDate = initialDate;

        protected void ChangeEndDateSelected(DateTime? endDate) =>
            CrudControl.FinalDate = endDate;

        protected void FilterBySupplier(string supplierName)
        {
            CrudControl.SearchValue = supplierName;
            ReloadPage();
        }

        protected async void ReloadPage() =>
            await GetOrdersAsync();
    }
}
