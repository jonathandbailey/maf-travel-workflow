using Agents.Extensions;
using Api;
using Api.Extensions;
using Api.Hub;
using Api.Middleware;
using Api.Settings;
using Application.Extensions;
using Infrastructure.Extensions;
using Workflows.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<HubSettings>(options => builder.Configuration.GetSection("HubSettings").Bind(options));

builder.AddCorsPolicyFromServiceDiscovery();

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddWorkflowServices();

builder.Services.AddAgentServices(builder.Configuration);

builder.Services.AddApiServices();

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();

builder.Services.AddSignalR();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapApi();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.MapHub<UserHub>("hub");

app.UseMiddleware<ExecutionContextMiddleware>();

await app.MapAgUiToAgent();

app.UseCorsPolicyServiceDiscovery();



app.Run();
