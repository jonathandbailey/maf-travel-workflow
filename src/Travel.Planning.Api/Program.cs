using A2A;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var agentCard = new AgentCard
{
    Name = "Travel_Planning",
    Description = "An agent that plans your travel.",
    Url = "https://localhost:7286/api/a2a/travel/",
    Skills =
    [
        new AgentSkill { Name = "Search Flights", Description = "Searches for your flights." }
    ],
    Version = "1.0"
};

var taskManager = new TaskManager();

taskManager.OnAgentCardQuery += (args, cancellationToken) => Task.FromResult(agentCard);

taskManager.OnMessageReceived += async (@params, token) =>
{
  
    var message = new AgentMessage()
    {
        Role = MessageRole.Agent,
        MessageId = Guid.NewGuid().ToString(),
        ContextId = @params.Message.ContextId,
        Parts = [new TextPart() {
            Text = "This is the test response"
        }]
    };

    return message;
};

app.MapA2A(taskManager, "/api/a2a/travel");

app.Run();


