using GestaoCompras.DTO.Order;
using GestaoCompras.Web.Interfaces.Orders;
using Microsoft.AspNetCore.Components;

namespace GestaoCompras.Web.Pages.Orders;

public class ActiveWeeksSelectComponentBase : ComponentBase
{
    [Inject] IOrderService OrderService { get; set; }

    [Parameter] public string ActiveWeekIdSelected { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public bool RefreshWeeks { get; set; }
    [Parameter] public EventCallback<string> ChangeActiveWeekIdSelected { get; set; }
    [Parameter] public EventCallback<DateTime?> ChangeInitialDateSelected { get; set; }
    [Parameter] public EventCallback<DateTime?> ChangeEndDateSelected { get; set; }
    [Parameter] public EventCallback ReloadPage { get; set; }

    protected List<ActiveWeekGetDTO> ActiveWeeksGetDTO { get; set; }
    protected bool PageIsReady { get; set; }

    private bool _actualRefreshWeeks;

    protected override async Task OnParametersSetAsync()
    {
        if (_actualRefreshWeeks != RefreshWeeks && RefreshWeeks)
        {
            await GetActiveWeeksAsync();
            _actualRefreshWeeks = RefreshWeeks;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await GetActiveWeeksAsync();

        PageIsReady = true;
        _actualRefreshWeeks = RefreshWeeks;
    }

    protected async Task GetActiveWeeksAsync()
    {
        ActiveWeeksGetDTO = await OrderService.GetActiveWeeksAsync($"Orders/GetActiveWeeks");

        _actualRefreshWeeks = false;
        PageIsReady = true;
    }

    protected async Task OnChangeActiveWeek(string selectedActiveWeekId)
    {
        if (ChangeInitialDateSelected.HasDelegate && ChangeEndDateSelected.HasDelegate)
        {
            if (string.IsNullOrEmpty(selectedActiveWeekId))
            {
                ActiveWeekIdSelected = null;
                await ChangeActiveWeekIdSelected.InvokeAsync(string.Empty);
                await ChangeInitialDateSelected.InvokeAsync(null);
                await ChangeEndDateSelected.InvokeAsync(null);
                await ReloadPage.InvokeAsync();
            }

            else
            {
                if (ChangeInitialDateSelected.HasDelegate && ChangeEndDateSelected.HasDelegate)
                {
                    if (Guid.TryParse(selectedActiveWeekId, out Guid id))
                    {
                        ActiveWeekIdSelected = id.ToString();

                        var initialDate = ActiveWeeksGetDTO.Where(a => a.Id == id).FirstOrDefault().InitialDate;
                        var endDate = ActiveWeeksGetDTO.Where(a => a.Id == id).FirstOrDefault().EndDate;

                        await ChangeActiveWeekIdSelected.InvokeAsync(selectedActiveWeekId.ToString());
                        await ChangeInitialDateSelected.InvokeAsync(initialDate);
                        await ChangeEndDateSelected.InvokeAsync(endDate);
                        await ReloadPage.InvokeAsync();
                    }
                }
            }
        }
    }
}
