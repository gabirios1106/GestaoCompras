using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.Text;
using GestaoCompras.API.Data;
using GestaoCompras.Models.Access;
using Microsoft.EntityFrameworkCore.Diagnostics;
using GestaoCompras.API.Automapper;
using Serilog;
using GestaoCompras.API.Interfaces.Access;
using GestaoCompras.API.Interfaces.Common;
using GestaoCompras.API.Interfaces.Users;
using GestaoCompras.API.Services.Access;
using GestaoCompras.API.Services.Common;
using GestaoCompras.API.Services.Users;
using GestaoCompras.API.Interfaces.Fruits;
using GestaoCompras.API.Services.Fruits;
using GestaoCompras.API.Interfaces.Suppliers;
using GestaoCompras.API.Services.Suppliers;
using GestaoCompras.API.Interfaces.Orders;
using GestaoCompras.API.Services.Orders;
using GestaoCompras.API.Interfaces.Stores;
using GestaoCompras.API.Services.Stores;

var builder = WebApplication.CreateBuilder(args);

#region DatabaseConnection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQLConnection"))
           .ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
});
#endregion DatabaseConnection

#region Identity
builder.Services.AddIdentity<User, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    options.User.RequireUniqueEmail = true;

    options.SignIn.RequireConfirmedEmail = true;
});

builder.Services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(2));
#endregion Identity

#region JwtAuthentication
var secretKey = builder.Configuration.GetValue<string>("Jwt:Key");
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.IncludeErrorDetails = builder.Environment.IsDevelopment() ? true : false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        RequireExpirationTime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer"),
        ValidateAudience = true,
        ValidAudience = builder.Configuration.GetValue<string>("Jwt:AudienceWeb"),
        ClockSkew = TimeSpan.Zero
    };
});
#endregion JwtAuthentication

#region CorsPolicy
var APICorsPolicy_Production = "APICorsPolicy_Production";
var APICorsPolicy_Development = "APICorsPolicy_Development";

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: APICorsPolicy_Production,
        builder =>
        {
            builder.WithOrigins("https://localhost", "https://backoffice.mesaderei.com.br")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
        });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: APICorsPolicy_Development,
        builder =>
        {
            builder.WithOrigins("https://localhost", "https://localhost:30497", "https://localhost:44334", "https://localhost:7070")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
        });
});
#endregion CorsPolicy

#region Controller
builder.Services.AddControllers()
                .AddJsonOptions(options => { options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull; })
                .AddNewtonsoftJson(options => { options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; });
#endregion Controller

#region AutoMapper
var mappingConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new AutomapperProfile());
});

var mapper = mappingConfig.CreateMapper();
builder.Services.AddSingleton(mapper);
#endregion AutoMapper

#region PersonalServices
#region Access
builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
builder.Services.AddScoped<IEncryptAndDecryptService, EncryptAndDecryptService>();
builder.Services.AddScoped<IGoogleReCaptchaService, GoogleReCaptchaService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
#endregion Access

#region Common
builder.Services.AddScoped<IExternalApiService, ExternalApiService>();
#endregion Common

#region Fruits
builder.Services.AddScoped<IFruitService, FruitService>();
#endregion Fruits

#region Orders
builder.Services.AddScoped<IOrderService, OrderService>();
#endregion Orders

#region Stores
builder.Services.AddScoped<IStoreService, StoreService>();
#endregion Stores

#region Supplier
builder.Services.AddScoped<ISupplierService, SupplierService>();
#endregion Supplier

#region Users
builder.Services.AddScoped<IUserDataService, UserDataService>();
#endregion Users
#endregion PersonalServices


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Logging
ConfigureLogging();
builder.Host.UseSerilog();
#endregion Logging

var app = builder.Build();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseDeveloperExceptionPage();
    app.UseCors(APICorsPolicy_Development);
}
else if (app.Environment.IsProduction())
    app.UseCors(APICorsPolicy_Production);

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

void ConfigureLogging()
{
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{environment}.json", optional: true).Build();

    Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            .Enrich.WithEnvironmentUserName()
            .Enrich.WithProperty("Environment", environment)
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithProcessName()
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .WriteTo.Console()
            .WriteTo.Debug()
            .CreateLogger();
}