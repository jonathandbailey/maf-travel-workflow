using Agents;
using Infrastructure.Interfaces;
using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Dto;
using Travel.Workflows.Nodes;
using Travel.Workflows.Services;

namespace Travel.Workflows;

public class WorkflowFactory(IAgentFactory agentFactory, IArtifactRepository artifactRepository, ITravelPlanService travelPlanService) : IWorkflowFactory
{
    public async Task<Workflow> Create()
    {
        var reasonAgent = await agentFactory.CreateReasonAgent();

        var flightAgent = await agentFactory.CreateFlightAgent();
      
        var requestPort = RequestPort.Create<UserRequest, ReasoningInputDto>("user-input");

        var reasonNode = new ReasonNode(reasonAgent, travelPlanService);
        var actNode = new ActNode(travelPlanService);
     
        var flightWorkerNode = new FlightWorkerNode(flightAgent, travelPlanService);
   
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