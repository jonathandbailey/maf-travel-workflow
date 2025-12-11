using Application.Agents;
using Application.Infrastructure;
using Application.Services;
using Application.Workflows.Dto;
using Application.Workflows.Nodes;
using Microsoft.Agents.AI.Workflows;

namespace Application.Workflows;

public class WorkflowFactory(IAgentFactory agentFactory, IArtifactRepository artifactRepository, ITravelPlanService travelPlanService) : IWorkflowFactory
{
    public async Task<Workflow> Create()
    {
        var reasonAgent = await agentFactory.Create(AgentTypes.Reason);
      
        var flightAgent = await agentFactory.Create(AgentTypes.FlightWorker);

        var hotelAgent = await agentFactory.Create(AgentTypes.HotelWorker);

        var userAgent = await agentFactory.Create(AgentTypes.User);

        var parserAgent = await agentFactory.Create(AgentTypes.Parser);

        var requestPort = RequestPort.Create<UserRequest, UserResponse>("user-input");

        var reasonNode = new ReasonNode(reasonAgent, travelPlanService);
        var actNode = new ActNode(travelPlanService);
     
        var flightWorkerNode = new FlightWorkerNode(flightAgent);
        var hotelWorkerNode = new HotelWorkerNode(hotelAgent);

        var artifactStorageNode = new ArtifactStorageNode(artifactRepository);

        var userNode = new UserNode(userAgent, parserAgent);
     
        var builder = new WorkflowBuilder(userNode);

        builder.AddEdge(userNode, reasonNode);


        builder.AddEdge(reasonNode, actNode);
        builder.AddEdge(actNode, userNode);
        builder.AddEdge(userNode, requestPort);
        
        builder.AddEdge(requestPort, userNode);
        
        builder.AddEdge(actNode, reasonNode);
        
        builder.AddEdge(actNode, flightWorkerNode);
        builder.AddEdge(actNode, hotelWorkerNode);

        builder.AddEdge(flightWorkerNode, artifactStorageNode);
        builder.AddEdge(hotelWorkerNode, artifactStorageNode);

        return builder.Build();
    }
}

public interface IWorkflowFactory
{
    Task<Workflow> Create();
}