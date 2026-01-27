using Agents.Services;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using Travel.Workflows.Dto;
using Travel.Workflows.Nodes;
using Travel.Workflows.Services;

namespace Travel.Workflows;

public class WorkflowFactory(IAgentFactory agentFactory, ITravelService travelService, IFlightService flightService) : IWorkflowFactory
{
    public async Task<Workflow> Create()
    {
        var schema = AIJsonUtilities.CreateJsonSchema(typeof(ReasoningOutputDto));

        var format = ChatResponseFormat.ForJsonSchema(
            schema: schema,
            schemaName: "ReasoningActRequest",
            schemaDescription: "Reasoning State for Act.");


        var planningAgent = await agentFactory.Create("planning_agent", format);

        agentFactory.UseMiddleware(planningAgent, "agent-thread");

        var fllightSchema = AIJsonUtilities.CreateJsonSchema(typeof(FlightAgentReponseDto));


        var flightChatResponseFormat = ChatResponseFormat.ForJsonSchema(
            schema: fllightSchema,
            schemaName: "FlightPlan",
            schemaDescription: "User Flight Options for their vacation.");

        var httpClient = new HttpClient();

      
        var serverUrl = "http://localhost:5146/";
        var transport = new HttpClientTransport(new()
        {
            Endpoint = new Uri(serverUrl),
            Name = "Travel MCP Client",
        }, httpClient);

        var mcpClient = await McpClient.CreateAsync(transport);

        var mcpTools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

        var flightAgent = await agentFactory.Create("flight_agent", flightChatResponseFormat, tools: [..mcpTools]);

        agentFactory.UseMiddleware(flightAgent, "agent-thread");
      
        var requestPort = RequestPort.Create<UserRequest, ReasoningInputDto>("user-input");

        var reasonNode = new PlanningNode(planningAgent, travelService);
        var actNode = new ExecutionNode(travelService);
     
        var flightWorkerNode = new FlightsNode(flightAgent, flightService);
   
        var startNode = new StartNode();
     
        var builder = new WorkflowBuilder(startNode);

        builder.AddEdge(startNode, reasonNode);
        
        builder.AddEdge(reasonNode, actNode);
        builder.AddEdge(actNode, requestPort);

        builder.AddEdge(requestPort, reasonNode);
        
        builder.AddEdge(actNode, reasonNode);
        
        builder.AddEdge(actNode, flightWorkerNode);
        
        builder.AddEdge(flightWorkerNode, actNode);
   
        return builder.Build();
    }
}

public interface IWorkflowFactory
{
    Task<Workflow> Create();
}