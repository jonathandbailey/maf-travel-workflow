using Application.Agents;
using Application.Workflows.Conversations.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Application.Workflows.Conversations.Nodes;

public class ActNode(IAgent agent) : ReflectingExecutor<ActNode>("ActNode"), IMessageHandler<ActRequest>, 
    IMessageHandler<UserResponse>

{
    public async ValueTask HandleAsync(ActRequest request, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var response = await agent.RunAsync(new List<ChatMessage> { request.Message }, cancellationToken: cancellationToken);

        if (JsonOutputParser.HasJson(response.Text))
        {
            var routeAction = JsonOutputParser.Parse<RouteAction>(response.Text);

            if (routeAction.Route == "ask_user")
            {
                var cleanedResponse = JsonOutputParser.Remove(response.Text);
                
                await context.SendMessageAsync(new UserRequest(cleanedResponse), cancellationToken: cancellationToken);
            }
        }
    }

    public async ValueTask HandleAsync(UserResponse userResponse, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        await context.SendMessageAsync(new ActObservation(userResponse.Message), cancellationToken: cancellationToken);
    }
}