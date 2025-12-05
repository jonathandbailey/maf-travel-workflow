using Application.Agents;
using Application.Infrastructure;
using Application.Services;
using Application.Workflows.ReAct.Dto;
using Application.Workflows.ReAct.Nodes;
using Application.Workflows.ReWoo.Nodes;
using Application.Workflows.Users;
using Microsoft.Agents.AI.Workflows;

namespace Application.Workflows;

public class WorkflowFactory(IAgentFactory agentFactory, IArtifactRepository artifactRepository, ITravelPlanService travelPlanService) : IWorkflowFactory
{
    public async Task<Workflow> Create()
    {
        var reasonAgent = await agentFactory.Create(AgentTypes.Reason);

        var actAgent = await agentFactory.Create(AgentTypes.Act);
    
        var flightAgent = await agentFactory.Create(AgentTypes.FlightWorker);

        var hotelAgent = await agentFactory.Create(AgentTypes.HotelWorker);

        var trainAgent = await agentFactory.Create(AgentTypes.TrainWorker);

        var userAgent = await agentFactory.Create(AgentTypes.User);

        var requestPort = RequestPort.Create<UserRequest, UserResponse>("user-input");

        var reasonNode = new ReasonNode(reasonAgent, travelPlanService);
        var actNode = new ActNode(actAgent, travelPlanService);
     
        var flightWorkerNode = new FlightWorkerNode(flightAgent);
        var hotelWorkerNode = new HotelWorkerNode(hotelAgent);
        var trainWorkerNode = new TrainWorkerNode(trainAgent);

        var artifactStorageNode = new ArtifactStorageNode(artifactRepository);

        var userNode = new UserNode(userAgent);
 
        var builder = new WorkflowBuilder(reasonNode);

        builder.AddEdge(reasonNode, actNode);
        builder.AddEdge(actNode, userNode);
        builder.AddEdge(userNode, requestPort);
        builder.AddEdge(requestPort, userNode);
        builder.AddEdge(userNode, reasonNode);
        
        builder.AddEdge(actNode, reasonNode);
        
        builder.AddEdge(actNode, flightWorkerNode);
        builder.AddEdge(actNode, hotelWorkerNode);
        builder.AddEdge(actNode, trainWorkerNode);

        builder.AddEdge(flightWorkerNode, artifactStorageNode);
        builder.AddEdge(hotelWorkerNode, artifactStorageNode);
        builder.AddEdge(trainWorkerNode, artifactStorageNode);

        return builder.Build();
    }
}

public interface IWorkflowFactory
{
    Task<Workflow> Create();
}