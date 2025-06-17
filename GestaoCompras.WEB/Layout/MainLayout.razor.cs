using GestaoCompras.Enums.Users;
using GestaoCompras.Web.Interfaces.Access;
using Microsoft.AspNetCore.Components;

namespace GestaoCompras.Web.Layout;

public class MainLayoutBase : ComponentBase
{
    [Inject] ISessionDataService SessionDataService { get; set; }

    protected UserRole UserRole { get; set; }

    protected override async void OnInitialized()
    {
        UserRole = await SessionDataService.GetSavedUserRoleAsync();
    }
}
