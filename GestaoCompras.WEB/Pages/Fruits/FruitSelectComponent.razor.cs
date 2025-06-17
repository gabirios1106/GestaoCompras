using GestaoCompras.DTO.Fruit;
using GestaoCompras.DTO.Supplier;
using GestaoCompras.Web.Interfaces.Fruits;
using Microsoft.AspNetCore.Components;
using System.Linq;

namespace GestaoCompras.Web.Pages.Fruits;

public class FruitSelectComponentBase : ComponentBase
{
    [Inject] IFruitService FruitService { get; set; }

    [Parameter] public string FruitIdSelected { get; set; }
    [Parameter] public string NewFruitIdSelected { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public bool Clearable { get; set; }
    [Parameter] public int MaxItems { get; set; } 
    [Parameter] public EventCallback<int> ChangeFruitSelected { get; set; }
    [Parameter] public EventCallback<double> ChangeUnitPriceSelected { get; set; }
    [Parameter] public EventCallback ReloadPage { get; set; }

    protected List<FruitGetDTO> FruitsGetDTO { get; set; }

    protected int ItemsInFruits { get; set; }

    protected bool PageIsReady { get; set; }

    private string _actualNewFruitIdSelectedParameter;

    protected override async Task OnInitializedAsync()
    {
        _actualNewFruitIdSelectedParameter = NewFruitIdSelected;
        await GetFruitAsync();

        //ItemsInFruits = FruitsGetDTO.Count;
        //MaxItems = ItemsInFruits;

        MaxItems = FruitsGetDTO.Count;

        PageIsReady = true;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (NewFruitIdSelected != "0" && NewFruitIdSelected != _actualNewFruitIdSelectedParameter)
        {
            await GetFruitAsync();
            FruitIdSelected = NewFruitIdSelected;

            StateHasChanged();
        }
    }

    protected Task<IEnumerable<string>> Search(string value, CancellationToken token)
    {
        var fruit = FruitsGetDTO.Select(s => s.Name);
        Console.WriteLine(ItemsInFruits);

        if (string.IsNullOrEmpty(value))
            return Task.FromResult(fruit);

        return Task.FromResult(fruit.Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase)));
    }

    protected async Task GetFruitAsync()
    {
        FruitsGetDTO = await FruitService.GetFruitsAsync($"Fruits?orderBy=ASC-NAME");
        PageIsReady = true;
    }

    protected async Task OnChangeFruit(string selectedFruitName)
    {
        if (string.IsNullOrEmpty(selectedFruitName))
            FruitIdSelected = string.Empty;

        if (!string.IsNullOrEmpty(selectedFruitName) && ChangeFruitSelected.HasDelegate)
        {
            var fruitIdSelected = FruitsGetDTO.FirstOrDefault(f => f.Name == selectedFruitName).Id;
            FruitIdSelected = fruitIdSelected.ToString();
            await ChangeFruitSelected.InvokeAsync(fruitIdSelected);

            if (ChangeUnitPriceSelected.HasDelegate)
            {
                var unitPrice = FruitsGetDTO.FirstOrDefault(f => f.Id == fruitIdSelected).Price;
                await ChangeUnitPriceSelected.InvokeAsync(unitPrice);
            }
        }
    }
}

