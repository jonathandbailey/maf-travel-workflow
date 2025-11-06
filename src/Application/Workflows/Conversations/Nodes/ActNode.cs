using Application.Agents;
using Application.Observability;
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
        using var activity = Telemetry.StarActivity("Act-Node");

        activity?.SetTag("Reason Request (Act Node)", request.Message);

        var response = await agent.RunAsync(new List<ChatMessage> { request.Message }, cancellationToken: cancellationToken);

        if (JsonOutputParser.HasJson(response.Text))
        {
            var routeAction = JsonOutputParser.Parse<RouteAction>(response.Text);

            if (routeAction.Route == "ask_user")
            {
                var cleanedResponse = JsonOutputParser.Remove(response.Text);

                activity?.SetTag("Ask User:", cleanedResponse);

                await context.SendMessageAsync(new UserRequest(cleanedResponse), cancellationToken: cancellationToken);
            }
        }
    }

    public async ValueTask HandleAsync(UserResponse userResponse, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var activity = Telemetry.StarActivity("Act-Node");

        activity?.SetTag("User Response/Observation:", userResponse);

        await context.SendMessageAsync(new ActObservation(userResponse.Message), cancellationToken: cancellationToken);
    }
}