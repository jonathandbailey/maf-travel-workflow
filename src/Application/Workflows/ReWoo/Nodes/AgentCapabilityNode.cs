using Application.Workflows.ReAct.Dto;
using Application.Workflows.ReWoo.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace Application.Workflows.ReWoo.Nodes;

public class AgentCapabilityNode() : ReflectingExecutor<AgentCapabilityNode>("") , IMessageHandler<RequestInputsDto>
{
    public async ValueTask HandleAsync(RequestInputsDto message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        const string response = "{\"capabilities\": [\"flight_search\", \"hotel_search\"], \"required_inputs\" : [\"origin\", \"destination\", \"depart_date\", \"return_date\"]}";

        await context.SendMessageAsync(new ActObservation(string.Empty, string.Empty), cancellationToken: cancellationToken);
    }
}