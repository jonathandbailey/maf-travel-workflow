using Agents.Extensions;
using Api.Extensions;
using Application.Extensions;
using Infrastructure.Extensions;
using Travel.Workflows.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddCorsPolicyFromServiceDiscovery();

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddWorkflowServices();

builder.Services.AddAgentServices(builder.Configuration);

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

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

await app.MapAgUiToAgent();

app.UseCorsPolicyServiceDiscovery();



app.Run();
