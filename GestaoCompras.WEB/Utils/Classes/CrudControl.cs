using GestaoCompras.Web.Pages.Orders;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GestaoCompras.Web.Utils.Classes;

public class CrudControl <T>
{
    public int TotalPages { get; set; }
    public int ActualPage { get; set; } = 1;
    public int TotalRegs { get; set; }
    public int ItemsPerPage { get; set; } = 25;
    public int Radius { get; set; } = 3;
    public int Skip { get; set; } = 0;
    public string OrderBy { get; set; }
    public string SearchValue { get; set; }
    public string SearchIdValue { get; set; }
    public bool ShowInactive { get; set; } = false;
    public int Colspan { get; set; } = 1;
    public int ExtraColumns { get; set; }
    public int IdDelete { get; set; }
    public Guid GuidDelete { get; set; }
    public string CodeDelete { get; set; }
    public bool PageIsReady { get; set; } = false;
    public DateTime? InitialDate { get; set; }
    public DateTime? FinalDate { get; set; }
    public bool Ticket { get; set; }
    public List<BreadcrumbItem> MudBreadcrumbs { get; set; } = new List<BreadcrumbItem>();
    public EventCallback<bool> GetItems { get; set; }
    public Order Order { get; set; }

    public CrudControl() { }

    public void Initialize(EventCallback<bool> getItems, bool pageIsReady)
    {
        GetItems = getItems;
        PageIsReady = pageIsReady;

        ResetPagination();
    }

    public void ResetPagination() => Skip = 0;

    public void SetPagination(int totalPages, int totalRegs)
    {
        TotalRegs = totalRegs;
        TotalPages = totalPages;
    }

    public async Task SearchItemAsync(string searchValue)
    {
        if (searchValue != SearchValue)
            ResetPagination();

        SearchValue = searchValue;
        SearchIdValue = string.Empty;

        await GetItems.InvokeAsync(true);
    }

    public async Task SearchItemByIdAsync(string searchIdValue)
    {
        if (searchIdValue != SearchIdValue)
            ResetPagination();

        SearchIdValue = searchIdValue;
        SearchValue = string.Empty;

        await GetItems.InvokeAsync(true);
    }

    public async Task ChangeInactiveOptionAsync()
    {
        ResetPagination();
        ShowInactive = (!ShowInactive);

        await GetItems.InvokeAsync(true);
    }

    public async void ChangeItemsPerPageAsync(int itemsPerPage)
    {
        ResetPagination();
        ItemsPerPage = itemsPerPage;

        await GetItems.InvokeAsync(true);
    }

    public async void SelectedPageAsync(int page)
    {
        var updateGrid = TotalPages == ActualPage || (ActualPage * ItemsPerPage) <= TotalRegs;

        ActualPage = page;
        Skip = ((page - 1) * ItemsPerPage);

        if (updateGrid)
            await GetItems.InvokeAsync();
    }

    public async void ClearSearchAsync()
    {
        InitialDate = null;
        FinalDate = null;

        await SearchItemAsync(string.Empty);
    }
}
