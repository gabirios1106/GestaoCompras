using GestaoCompras.DTO.Access;
using GestaoCompras.Web.Extensions;
using GestaoCompras.Web.Interfaces.Access;
using GestaoCompras.Web.Interfaces.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace GestaoCompras.Web.Pages.Access;

public class UserLoginBase : ComponentBase
{
    [Inject] ISnackbarService SnackbarService { get; set; }
    [Inject] IJSRuntime JSRuntime { get; set; }
    [Inject] IAccessService AccessService { get; set; }
    [Inject] IDialogService DialogService { get; set; }
    [Inject] ISessionDataService SessionDataService { get; set; }
    [Inject] IConfiguration Configuration { get; set; }
    [Inject] ITokenService TokenService { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] ILogger<UserLoginBase> Logger { get; set; }

    private RefreshTokenGetDTO _refreshTokenGetDTO = new();
    private DateTime _date = DateTime.Now;

    protected UserLoginPostDTO UserLoginPostDTO { get; set; } = new();
    protected AuthenticatedUserGetDTO AuthenticatedUserGetDTO { get; set; }
    protected bool IsRevalidate { get; set; } = false;
    protected bool SubmitInProgress { get; set; }
    protected bool RememberMe { get; set; }
    protected Dictionary<string, object> InputAttributes { get; set; } = new Dictionary<string, object>() { { "autocomplete", "new-password" } };

    protected override async Task OnInitializedAsync()
    {
        UserLoginPostDTO.UserName = SessionDataService.GetUserNameToRevalidate();

        if (!string.IsNullOrEmpty(UserLoginPostDTO.UserName))
        {
            IsRevalidate = true;
            SnackbarService.ShowSnackbar("Sessão expirada", "Informe sua senha para se conectar novamente", Severity.Warning, 5000);

            Logger.LogWarning("A sessão do usuário {UserName} expirou", UserLoginPostDTO.UserName);

            SessionDataService.SetUserNameToRevalidate(string.Empty);
        }

        if (string.IsNullOrEmpty(UserLoginPostDTO.UserName))
            RememberMe = await SessionDataService.GetRememberMeAsync();

        if (RememberMe)
            UserLoginPostDTO.UserName = await SessionDataService.GetUserToRemember();

        var envName = Configuration.GetValue<string>("EnvName");

        if (envName == "Development")
        {
            UserLoginPostDTO.UserName = "gabirios1106@gmail.com";
            UserLoginPostDTO.PasswordHash = "ga110600";

            //UserLoginPostDTO.UserName = "funcionarioteste@gmail.com";
            //UserLoginPostDTO.PasswordHash = "funcionario123";

            Logger.LogInformation("Preenchendo dados automáticamente para o usuário {UserName}, pois este é um ambiente de {EnvName}", UserLoginPostDTO.UserName, envName);

            await LoginAsync();
        }
    }

    protected async Task LoginAsync()
    {
        ChangeButtonSubmit();

        Logger.LogInformation("Iniciado o processo de login para o usuário {UserName}", UserLoginPostDTO.UserName);

        try
        {
            UserLoginPostDTO.ReCaptchaToken = await JSRuntime.GetGoogleReCaptchaToken();
            Logger.LogInformation("Token ReCaptcha obtido");
        }
        catch (Exception e)
        {
            UserLoginPostDTO.ReCaptchaToken = string.Empty;
            Logger.LogError("Erro ao obter o token ReCaptcha. ErrorMessage: {Message}. StackTrace: {StackTrace}", e.Message, e.StackTrace);
        }

        UserLoginPostDTO.AccessAttemptDate = new DateTime(_date.Year, _date.Month, _date.Day);

        AuthenticatedUserGetDTO = await AccessService.LoginAsync(UserLoginPostDTO);

        if (AuthenticatedUserGetDTO != null)
        {
            if (RememberMe)
                await SessionDataService.SetUserToRememberAsync(UserLoginPostDTO.UserName);

            await TokenService.UserLoginAsync(AuthenticatedUserGetDTO);

            var routeToRedirect = SessionDataService.GetRouteToRedirect();

            NavigationManager.NavigateTo($"/{routeToRedirect}");
            Logger.LogInformation("Redirecionamento da navegação para a rota '/{RouteToRedirect}'", routeToRedirect);

            SnackbarService.ShowSnackbar("Sucesso", "Acesso validado", Severity.Success, 1750);
            StateHasChanged();
        }

        ChangeButtonSubmit();
    }

    public void ChangeButtonSubmit()
    {
        SubmitInProgress = !SubmitInProgress;
        StateHasChanged();
    }

    protected async Task ChangeUserAsync()
    {
        Logger.LogInformation("Limpando os dados do usuário logado.");

        SnackbarService.ShowSnackbar("Aguarde", "Realizando logout...", Severity.Info, 1750);
        var logoutSuccessfull = await AccessService.LogoutAsync();

        if (logoutSuccessfull)
        {
            TokenService.UserLogoutAsync();
            SnackbarService.ShowSnackbar("Sucesso", "Usuário desconectado", Severity.Success, 1750);
        }

        IsRevalidate = false;
        UserLoginPostDTO = new();
    }

    protected async Task ChangeRememberMeOptionAsync(bool rememberMe)
    {
        await SessionDataService.SetRememberMeAsync(rememberMe);

        if (!rememberMe)
            await SessionDataService.SetUserToRememberAsync();

        RememberMe = await SessionDataService.GetRememberMeAsync();
    }
}
