using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Travel.Workflows.Dto;
using Travel.Workflows.Extensions;

namespace Travel.Workflows.Nodes;

public class StartNode() : ReflectingExecutor<StartNode>("StartNode") ,
IMessageHandler<StartWorkflowDto, ReasoningInputDto>
{
    public async ValueTask<ReasoningInputDto> HandleAsync(StartWorkflowDto message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        await context.AddThreadId(message.ThreadId, cancellationToken);

        return message.ReasoningInputDto;
    }
}