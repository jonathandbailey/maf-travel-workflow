using AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorageServices();

var blobs = builder.AddAzureBlobsServices(storage);

var gateway =  builder.AddProject<Projects.Travel_Gateway>("travel-gateway");


var api = builder.AddProject<Projects.Travel_Experience_Api>("travel-experience-api").
WithReference(blobs).
    WaitFor(storage)
    .WithEndpoint("http", endpoint => { endpoint.Port = 5000;})
    .WithEndpoint("https", endpoint => { endpoint.Port = 5001; });

var ui = builder.AddUiServices(gateway);

api.WithReference(ui);

gateway.WithReference(ui);


builder.AddProject<Projects.Travel_Workflows_Api>("travel-workflows-api")
    .WithReference(blobs)
    .WaitFor(storage);

builder.AddProject<Projects.Travel_Application_Api>("travel-application-api")
    .WithReference(blobs)
    .WaitFor(storage);


builder.AddProject<Projects.Travel_Application_Mcp>("travel-application-mcp");


builder.Build().Run();
