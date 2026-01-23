using System.Text.Json.Serialization;
using Infrastructure.Extensions;
using Infrastructure.Settings;
using Travel.Application.Api;
using Travel.Application.Api.Infrastructure;
using Travel.Application.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<AzureStorageSettings>((options) => builder.Configuration.GetSection("AzureStorageSeedSettings").Bind(options));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(ApiMappings).Assembly);
});

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddHostedService<AzureStorageSeedService>();

builder.Services.AddScoped<ITravelPlanRepository, TravelPlanPlanRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IFlightRepository, FlightRepository>();

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

