using Microsoft.AspNetCore.Components;

namespace GestaoCompras.Web.Layout
{
    public class DataGridSkeletonBase : ComponentBase
    {
        [Parameter] public bool ShowDetailHeader { get; set; }
        [Parameter] public bool ShowCrudHeader { get; set; }
    }
}
