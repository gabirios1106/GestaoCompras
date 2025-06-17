using GestaoCompras.Web.Interfaces.Access;
using GestaoCompras.Web.Interfaces.Common;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GestaoCompras.Web.Pages.Users;

public class UserMenuBase : ComponentBase
{
    [Inject] ISnackbarService SnackbarService { get; set; }
    [Inject] IAccessService AccessService { get; set; }
    [Inject] ISessionDataService SessionDataService { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] ITokenService TokenAuthenticationProvider { get; set; }
    [Inject] ILogger<UserMenuBase> Logger { get; set; }

    protected String BatteryLevelIcon { get; set; } = Icons.Material.Filled.BatteryFull;
    protected string Name { get; set; }

    protected async override Task OnInitializedAsync()
    {
        Logger.LogInformation("Componente inicializado...");

        Name = await SessionDataService.GetSavedNameAsync();
        UpdateBatteryIcon();
    }

    protected async Task GetOrdersAsync()
    {

    }


    protected async void LogoutAsync()
    {
        SnackbarService.ShowSnackbar("Aguarde", "Realizando logout...", Severity.Info, 1750);

        var logoutSuccessfull = await AccessService.LogoutAsync();

        TokenAuthenticationProvider.UserLogoutAsync();
        NavigationManager.NavigateTo("/");

        if (logoutSuccessfull)
            SnackbarService.ShowSnackbar("Sucesso", "Usuário desconectado", Severity.Success, 1750);
    }

    protected async void UpdateBatteryIcon()
    {
        var canLoop = true;

        while (canLoop)
        {
            var authenticationState = await SessionDataService.GetAuthenticationState();
            canLoop = authenticationState.User.Claims.Any();

            if (canLoop)
            {
                StateHasChanged();

                var timeSessionRemaining = await SessionDataService.GetTimeSessionRemaining();
                var tokenLifeTime = await SessionDataService.GetTokenLifeTimeAsync();
                var sessionRemainingPercent = timeSessionRemaining / tokenLifeTime;
                var delay = Math.Round(tokenLifeTime / 5, 2);

                if (sessionRemainingPercent <= 1 && sessionRemainingPercent >= 0.75)
                    BatteryLevelIcon = Icons.Material.Filled.BatteryFull;

                if (sessionRemainingPercent < 0.75 && sessionRemainingPercent >= 0.5)
                    BatteryLevelIcon = Icons.Material.Filled.Battery5Bar;

                if (sessionRemainingPercent < 0.5 && sessionRemainingPercent >= 0.25)
                    BatteryLevelIcon = Icons.Material.Filled.Battery3Bar;

                if (sessionRemainingPercent > 0 && sessionRemainingPercent < 0.25)
                    BatteryLevelIcon = Icons.Material.Filled.Battery1Bar;

                if (sessionRemainingPercent <= 0)
                {
                    BatteryLevelIcon = Icons.Material.Filled.Battery0Bar;
                    canLoop = false;
                }

                StateHasChanged();

                if (canLoop)
                    await Task.Delay(TimeSpan.FromSeconds(delay));
            }
        }

        Logger.LogInformation("Sessão finalizada");
    }
}
