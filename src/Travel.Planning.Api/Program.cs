using A2A;
using Travel.Planning.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton<IAgentDiscoveryService, AgentDiscoveryService>();
builder.Services.AddSingleton<IWorkflowService, WorkflowService>();

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


