using Application.Agents;
using Application.Infrastructure;
using Application.Services;
using Application.Workflows.ReAct.Dto;
using Application.Workflows.ReAct.Nodes;
using Application.Workflows.ReWoo.Dto;
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

        var orchestrationAgent = await agentFactory.Create(AgentTypes.Orchestration);

        var flightAgent = await agentFactory.Create(AgentTypes.FlightWorker);

        var hotelAgent = await agentFactory.Create(AgentTypes.HotelWorker);

        var trainAgent = await agentFactory.Create(AgentTypes.TrainWorker);

        var userAgent = await agentFactory.Create(AgentTypes.User);

        var requestPort = RequestPort.Create<UserRequest, UserResponse>("user-input");

        var reasonNode = new ReasonNode(reasonAgent, travelPlanService);
        var actNode = new ActNode(actAgent, travelPlanService);
        var orchestrationNode = new OrchestrationNode(orchestrationAgent);

        var flightWorkerNode = new FlightWorkerNode(flightAgent);
        var hotelWorkerNode = new HotelWorkerNode(hotelAgent);
        var trainWorkerNode = new TrainWorkerNode(trainAgent);

        var artifactStorageNode = new ArtifactStorageNode(artifactRepository);

        var userNode = new UserNode(userAgent);

        var agentCapabilityNode = new AgentCapabilityNode();

        var builder = new WorkflowBuilder(reasonNode);

        builder.AddEdge(reasonNode, actNode);
        builder.AddEdge(actNode, userNode);
        builder.AddEdge(actNode, agentCapabilityNode);
        builder.AddEdge(agentCapabilityNode, reasonNode);
        builder.AddEdge(userNode, requestPort);
        builder.AddEdge(requestPort, userNode);
        builder.AddEdge(userNode, reasonNode);
        builder.AddEdge(actNode, reasonNode);
        builder.AddEdge(actNode, orchestrationNode);

        builder.AddEdge<OrchestratorWorkerTaskDto>(
            source: orchestrationNode,
            target: flightWorkerNode,
            condition: result => result?.Worker == "research_flights");

        builder.AddEdge<OrchestratorWorkerTaskDto>(
            source: orchestrationNode,
            target: trainWorkerNode,
            condition: result => result?.Worker == "research_trains");

        builder.AddEdge<OrchestratorWorkerTaskDto>(
            source: orchestrationNode,
            target: hotelWorkerNode,
            condition: result => result?.Worker == "research_hotels");

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