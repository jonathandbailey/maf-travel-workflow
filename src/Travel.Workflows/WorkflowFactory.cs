using Agents;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
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


        var reasonAgent = await agentFactory.Create("planning_agent", format);

        var fllightSchema = AIJsonUtilities.CreateJsonSchema(typeof(FlightActionResultDto));

        var flightChatResponseFormat = ChatResponseFormat.ForJsonSchema(
            schema: fllightSchema,
            schemaName: "FlightPlan",
            schemaDescription: "User Flight Options for their vacation.");

        var flightAgent = await agentFactory.Create("flight_agent", flightChatResponseFormat);
      
        var requestPort = RequestPort.Create<UserRequest, ReasoningInputDto>("user-input");

        var reasonNode = new PlanningNode(reasonAgent, travelService);
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