using Api;
using Api.Extensions;
using Api.Hub;
using Api.Settings;
using Application.Extensions;
using Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<HubSettings>(options => builder.Configuration.GetSection("HubSettings").Bind(options));

builder.AddCorsPolicyFromServiceDiscovery();

builder.Services.AddInfrastructureServices();

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

await app.MapAgUiToAgent();

app.UseCorsPolicyServiceDiscovery();



app.Run();
