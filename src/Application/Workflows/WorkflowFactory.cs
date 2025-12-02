using Application.Agents;
using Application.Infrastructure;
using Application.Workflows.ReAct.Dto;
using Application.Workflows.ReAct.Nodes;
using Application.Workflows.ReWoo.Dto;
using Application.Workflows.ReWoo.Nodes;
using Application.Workflows.Users;
using Microsoft.Agents.AI.Workflows;

namespace Application.Workflows;

public class WorkflowFactory(IAgentFactory agentFactory, IArtifactRepository artifactRepository) : IWorkflowFactory
{
    public async Task<Workflow> Create()
    {
        var reasonAgent = await agentFactory.Create(AgentTypes.Reason);

        var actAgent = await agentFactory.Create(AgentTypes.Act);

        var orchestrationAgent = await agentFactory.Create(AgentTypes.Orchestration);

        var flightAgent = await agentFactory.Create(AgentTypes.FlightWorker);

        var hotelAgent = await agentFactory.Create(AgentTypes.HotelWorker);

        var trainAgent = await agentFactory.Create(AgentTypes.TrainWorker);

        var requestPort = RequestPort.Create<UserRequest, ActObservation>("user-input");

        var reasonNode = new ReasonNode(reasonAgent);
        var actNode = new ActNode(actAgent);
        var orchestrationNode = new OrchestrationNode(orchestrationAgent);

        var flightWorkerNode = new FlightWorkerNode(flightAgent);
        var hotelWorkerNode = new HotelWorkerNode(hotelAgent);
        var trainWorkerNode = new TrainWorkerNode(trainAgent);

        var artifactStorageNode = new ArtifactStorageNode(artifactRepository);

        var userNode = new UserNode();

        var builder = new WorkflowBuilder(reasonNode);

        builder.AddEdge(reasonNode, actNode);
        builder.AddEdge(actNode, userNode);
        builder.AddEdge(userNode, requestPort);
        builder.AddEdge(requestPort, reasonNode);
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