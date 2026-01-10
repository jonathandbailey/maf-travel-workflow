using Application.Agents;
using Application.Interfaces;
using Application.Services;
using Application.Workflows.Dto;
using Application.Workflows.Nodes;
using Microsoft.Agents.AI.Workflows;

namespace Application.Workflows;

public class WorkflowFactory(IAgentFactory agentFactory, IArtifactRepository artifactRepository, ITravelPlanService travelPlanService) : IWorkflowFactory
{
    public async Task<Workflow> Create()
    {
        var reasonAgent = await agentFactory.CreateReasonAgent();
      
        var flightAgent = await agentFactory.Create(AgentTypes.FlightWorker);
      
        var requestPort = RequestPort.Create<UserRequest, ReasoningInputDto>("user-input");

        var reasonNode = new ReasonNode(reasonAgent, travelPlanService);
        var actNode = new ActNode(travelPlanService);
     
        var flightWorkerNode = new FlightWorkerNode(flightAgent, travelPlanService);
    
        var artifactStorageNode = new ArtifactStorageNode(artifactRepository);
     
        var builder = new WorkflowBuilder(reasonNode);

        builder.AddEdge(reasonNode, actNode);
        builder.AddEdge(actNode, requestPort);

        builder.AddEdge(requestPort, reasonNode);
        
        builder.AddEdge(actNode, reasonNode);
        
        builder.AddEdge(actNode, flightWorkerNode);
 
        builder.AddEdge(flightWorkerNode, artifactStorageNode);
        
        builder.AddEdge(flightWorkerNode, actNode);
   
        return builder.Build();
    }
}

public interface IWorkflowFactory
{
    Task<Workflow> Create();
}