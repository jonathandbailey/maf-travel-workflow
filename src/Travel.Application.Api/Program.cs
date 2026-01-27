using Infrastructure.Extensions;
using Infrastructure.Settings;
using System.Text.Json.Serialization;
using Travel.Application.Api;
using Travel.Application.Api.Middleware;
using Travel.Application.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<AzureStorageSettings>((options) => builder.Configuration.GetSection("AzureStorageSeedSettings").Bind(options));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(ApiMappings).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(ApplicationServicesExtensions).Assembly);
});

builder.Services.AddTransient<GlobalExceptionHandler>();

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapApi();

app.UseMiddleware<GlobalExceptionHandler>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

