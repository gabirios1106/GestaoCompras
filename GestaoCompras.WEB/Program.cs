using GestaoCompras.Web;
using GestaoCompras.Web.Interfaces.Access;
using GestaoCompras.Web.Interfaces.Common;
using GestaoCompras.Web.Interfaces.Fruits;
using GestaoCompras.Web.Interfaces.Orders;
using GestaoCompras.Web.Interfaces.Stores;
using GestaoCompras.Web.Interfaces.Supplier;
using GestaoCompras.Web.Services.Access;
using GestaoCompras.Web.Services.Common;
using GestaoCompras.Web.Services.Fruits;
using GestaoCompras.Web.Services.Orders;
using GestaoCompras.Web.Services.Stores;
using GestaoCompras.Web.Services.Supplier;
using GestaoCompras.Web.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using SnackbarService = GestaoCompras.Web.Services.Common.SnackbarService;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiEndpointUri = AppSettings.GetAppSettings(builder.Configuration["EnvName"], "APIEndpoint");

builder.Services.AddHttpClient("BackEndAPI",
    client => client.BaseAddress = new Uri(apiEndpointUri));

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 2500;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<TokenService>();

builder.Services.AddScoped<ITokenService, TokenService>(
    provider => provider.GetRequiredService<TokenService>());

builder.Services.AddScoped<AuthenticationStateProvider, TokenService>(
    provider => provider.GetRequiredService<TokenService>());

#region PersonalServices
#region Access
builder.Services.AddScoped<IAccessService, AccessService>();
builder.Services.AddSingleton<ISessionDataService, SessionDataService>();
#endregion Access

#region Fruit
builder.Services.AddScoped<IFruitService, FruitService>();
#endregion Fruit

#region Store
builder.Services.AddScoped<IStoreService, StoreService>();
#endregion Store

#region Order
builder.Services.AddScoped<IOrderService, OrderService>();
#endregion Order

#region Supplier
builder.Services.AddScoped<ISupplierService, SupplierService>();
#endregion Supplier

#region Commom
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<IErrorMessageService, ErrorMessageService>();
builder.Services.AddScoped<IHttpMessageService, HttpMessageService>();
builder.Services.AddScoped<ISnackbarService, SnackbarService>();
#endregion Commom
#endregion PersonalServices


#region Logging
builder.Logging.AddConfiguration(
    builder.Configuration.GetSection("Logging"));
#endregion Logging


await builder.Build().RunAsync();
