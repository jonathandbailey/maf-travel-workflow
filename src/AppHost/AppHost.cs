using AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorageServices();

var blobs = builder.AddAzureBlobsServices(storage);

var api = builder.AddProject<Projects.Api>("api").
    WithReference(blobs).
    WaitFor(storage)
    .WithEndpoint("http", endpoint => { endpoint.Port = 5000;})
    .WithEndpoint("https", endpoint => { endpoint.Port = 5001; });

var ui = builder.AddUiServices(api);

api.WithReference(ui);

builder.AddProject<Projects.Travel_Planning_Api>("travel-planning-api");

builder.Build().Run();
