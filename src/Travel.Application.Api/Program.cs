using Infrastructure.Extensions;
using Infrastructure.Settings;
using Travel.Application.Api;
using Travel.Application.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<AzureStorageSeedSettings>((options) => builder.Configuration.GetSection("AzureStorageSeedSettings").Bind(options));



builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddScoped<ITravelPlanService, TravelPlanService>();
builder.Services.AddScoped<ISessionService, SessionService>();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapApi();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

