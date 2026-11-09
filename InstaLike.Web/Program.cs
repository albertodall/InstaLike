using System.Reflection;
using InstaLike.Core.Domain;
using InstaLike.Core.Services;
using InstaLike.Web.Extensions;
using InstaLike.Web.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Services.AddSerilog((services, cfg) => cfg
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services),
    writeToProviders: true);

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.AddHttpContextAccessor();
builder.Services.ConfigureTelemetry(builder.Environment.IsDevelopment());

builder.Services.AddSingleton<ISequentialIdGenerator<SequentialGuid>, SequentialGuidGenerator>();

builder.Services.ConfigurePipeline();
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly(), typeof(EntityBase<>).Assembly));

if (IsOnPremDeployment(builder.Configuration))
{
    builder.Services.ConfigureOnPremDataAccess(builder.Configuration);
}
else
{
    builder.Services.ConfigureCloudDataAccess(builder.Configuration);
}
builder.Services.ConfigureAzureComputerVision(builder.Configuration);

builder.Services.ConfigureAuthentication();
builder.Services.ConfigureAuthorization();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.UseCookiePolicy();

app.MapDefaultControllerRoute();

app.Run();

static bool IsOnPremDeployment(IConfiguration configuration) =>
    configuration.GetValue<DeploymentType>("Settings:DeploymentType") == DeploymentType.OnPrem;
