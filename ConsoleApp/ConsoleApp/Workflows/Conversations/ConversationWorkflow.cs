using Azure.Core;
using ConsoleApp.Workflows.Conversations.ReAct;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace ConsoleApp.Workflows.Conversations;

public class ConversationWorkflow(AIAgent reasonAgent, AIAgent actAgent, CheckpointStore checkPointStore, CheckpointManager checkpointManager)
{
    public async Task<ConversationWorkFlowResponse> Execute(ConversationWorkFlowRequest request, CancellationToken cancellationToken)
    {
        var reasonNode = new ReasonNode(reasonAgent);
        var actNode = new ActNode(actAgent);

        var inputPort = RequestPort.Create<UserRequest, UserResponse>("user-input");

        var builder = new WorkflowBuilder(reasonNode);
        
        builder.AddEdge(reasonNode, actNode);
        builder.AddEdge(actNode, inputPort);
        builder.AddEdge( inputPort, actNode);

        var workflow = await builder.BuildAsync<string>();

        Checkpointed<StreamingRun> run;

        if (checkPointStore.HasCheckpoint(request.SessionId))
        {
            var checkpointInfo = checkPointStore.Get(request.SessionId);
            
            run = await InProcessExecution.ResumeStreamAsync(workflow, checkpointInfo, checkpointManager, checkpointInfo.RunId, cancellationToken: cancellationToken);
        }
        else
        {
            run = await InProcessExecution.StreamAsync(workflow, request.Message, checkpointManager, cancellationToken: cancellationToken);
        }
       

        await foreach (var evt in run.Run.WatchStreamAsync(cancellationToken))
        {
            if (evt is ConversationStreamingEvent { Data: not null } streamingEvent)
            {
                var messageString = streamingEvent.Data?.ToString() ?? string.Empty;
                Console.Write(messageString);
                
            }

            if (evt is SuperStepCompletedEvent superStepCompletedEvt)
            {
                var checkpoint = superStepCompletedEvt.CompletionInfo!.Checkpoint;
                if (checkpoint != null)
                {
                    checkPointStore.Add(request.SessionId, checkpoint);

                    Console.WriteLine();
                    Console.WriteLine($"-Checkpoint added: {checkpoint}, {request.SessionId}");
                }
            }

            if (evt is WaitForUserInputEvent userInputEvent)
            {

            }

            if (evt is RequestInfoEvent requestInfoEvent)
            {
                if (request.State == ConversationWorkflowState.UserResponse)
                {
                    var resp = requestInfoEvent.Request.CreateResponse(new UserResponse() { Message = request.Message});
                    await run.Run.SendResponseAsync(resp);
                }
                else
                {
                    return new ConversationWorkFlowResponse()
                        { Message = requestInfoEvent.Data?.ToString(), State = ConversationWorkflowState.AssistantRequest, SessionId = request.SessionId };
                }
            }
            
        }

        return new ConversationWorkFlowResponse()
            { Message = string.Empty, State = ConversationWorkflowState.Completed, SessionId = request.SessionId};

     }
}