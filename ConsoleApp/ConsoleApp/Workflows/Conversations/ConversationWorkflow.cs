using ConsoleApp.Workflows.Conversations.ReAct;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace ConsoleApp.Workflows.Conversations;

public class ConversationWorkflow(AIAgent reasonAgent, AIAgent actAgent)
{
    public async Task Execute(string userInput, CancellationToken cancellationToken)
    {
        var checkpointManager = CheckpointManager.CreateInMemory();
        var checkpoints = new List<CheckpointInfo>();

        var reasonNode = new ReasonNode(reasonAgent);
        var actNode = new ActNode(actAgent);

        var inputPort = RequestPort.Create<UserRequest, UserResponse>("user-input");

        var builder = new WorkflowBuilder(reasonNode);
        builder.AddEdge(reasonNode, actNode);
        builder.AddEdge(actNode, inputPort);

        var workflow = await builder.BuildAsync<string>();

        var run = await InProcessExecution.StreamAsync(workflow, userInput, checkpointManager, cancellationToken: cancellationToken);

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
                    checkpoints.Add(checkpoint);
                }
            }

            if (evt is WaitForUserInputEvent userInputEvent)
            {

            }

            if (evt is RequestInfoEvent requestInfoEvent)
            {
                return;
            }
            
        }
    }
}