using Application.Agents;
using Application.Observability;
using Application.Workflows.ReAct.Dto;
using Application.Workflows.ReAct.Nodes;
using Application.Workflows.ReWoo.Dto;
using Application.Workflows.ReWoo.Nodes;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace Application.Workflows;

public class ReActWooWorkflow(IAgent reasonAgent, IAgent actAgent, IAgent orchestrationAgent, CheckpointManager checkpointManager, CheckpointInfo? checkpointInfo, WorkflowState state)
{
    private CheckpointManager CheckpointManager { get; set; } = checkpointManager;

    public CheckpointInfo? CheckpointInfo { get; private set; } = checkpointInfo;

    public WorkflowState State { get; private set; } = state;

    public async Task<WorkflowResponse> Execute(ChatMessage message)
    {
        using var activity = Telemetry.Start("WorkflowExecute");

        activity?.AddTag("workflow.state", State.ToString());
        activity?.AddTag("workflow.instance.id", CheckpointInfo?.CheckpointId);
        activity?.AddTag("workflow.has_checkpoint", (CheckpointInfo != null).ToString());
        activity?.AddTag("workflow.user.message", message.Text);

        var workflow = await BuildWorkflow();

        var run = await workflow.CreateStreamingRun(message, State, CheckpointManager, CheckpointInfo);

        await foreach (var evt in run.Run.WatchStreamAsync())
        {
            if (State == WorkflowState.Initialized)
            {
                State = WorkflowState.Executing;
            }
            
            if (evt is SuperStepCompletedEvent superStepCompletedEvt)
            {
                var checkpoint = superStepCompletedEvt.CompletionInfo!.Checkpoint;

                if (checkpoint != null)
                {
                    CheckpointInfo = checkpoint;
                }
            }

            if (evt is ReasonActWorkflowCompleteEvent reasonActWorkflowCompleteEvent)
            {
                return new WorkflowResponse(WorkflowState.Completed, reasonActWorkflowCompleteEvent.Message);
            }

            if (evt is RequestInfoEvent requestInfoEvent)
            {
                switch (State)
                {
                    case WorkflowState.Executing:
                    {
                        var response = requestInfoEvent.HandleRequestForUserInput();

                        State = WorkflowState.WaitingForUserInput;

                        return response;
                    }
                    case WorkflowState.WaitingForUserInput:
                    {
                        var resp = requestInfoEvent.Request.CreateResponse(new UserResponse(message.Text));

                        State = WorkflowState.Executing;
                        await run.Run.SendResponseAsync(resp);
                        break;
                    }
                }
            }
        }

        return new WorkflowResponse(WorkflowState.Completed, string.Empty);
    }

    private async Task<Workflow<ChatMessage>> BuildWorkflow()
    {
        var requestPort = RequestPort.Create<UserRequest, UserResponse>("user-input");

        var reasonNode = new ReasonNode(reasonAgent);
        var actNode = new ActNode(actAgent);
        var orchestrationNode = new OrchestrationNode(orchestrationAgent);

        var flightWorkerNode = new FlightWorkerNode();
        var hotelWorkerNode = new HotelWorkerNode();
        var trainWorkerNode = new TrainWorkerNode();

        var builder = new WorkflowBuilder(reasonNode);

        builder.AddEdge(reasonNode, actNode);
        builder.AddEdge(actNode, requestPort);
        builder.AddEdge(requestPort, actNode);
        builder.AddEdge(actNode, reasonNode);
        builder.AddEdge(actNode, orchestrationNode);

        builder.AddEdge<OrchestratorWorkerTaskDto>(
            source: orchestrationNode, 
            target:flightWorkerNode, 
            condition: result => result?.Worker == "research_flights");

        builder.AddEdge<OrchestratorWorkerTaskDto>(
            source: orchestrationNode,
            target: trainWorkerNode,
            condition: result => result?.Worker == "research_trains");

        builder.AddEdge<OrchestratorWorkerTaskDto>(
            source: orchestrationNode,
            target: hotelWorkerNode,
            condition: result => result?.Worker == "research_hotels");

        return await builder.BuildAsync<ChatMessage>();
    }
}



