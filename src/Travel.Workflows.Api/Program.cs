using Agents.Extensions;
using Infrastructure.Extensions;
using Infrastructure.Settings;
using System.Text.Json.Serialization;
using Travel.Planning.Api.Services;
using Travel.Workflows.Api.Services;
using Travel.Workflows.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddWorkflowServices();
builder.Services.AddAgentServices(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<AzureStorageSeedSettings>((options) => builder.Configuration.GetSection("AzureStorageSeedSettings").Bind(options));



builder.Services.AddSingleton<IAgentDiscoveryService, AgentDiscoveryService>();
builder.Services.AddSingleton<IWorkflowService, WorkflowService>();
builder.Services.AddSingleton<ITravelWorkflowService, TravelWorkflowService>();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var workflowService = app.Services.GetRequiredService<IWorkflowService>();

app.MapA2A(workflowService.TaskManager, "/api/a2a/travel");

app.Run();


