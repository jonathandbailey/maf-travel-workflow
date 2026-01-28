using System.Diagnostics;
using Microsoft.Agents.AI;

namespace Travel.Workflows.Observability;

public static class TravelWorkflowTelemetry
{
    private static readonly ActivitySource Source = new ActivitySource("Travel.Workflows", "1.0.0");
    private static readonly int MaxPreviewLength = 100;

    public static Activity? Start(string name)
    {
        return Source.StartActivity(name);
    }

    public static Activity? InvokeNode(string name, Guid threadId)
    {
        var tags = new ActivityTagsCollection
        {
            { "gen_ai.workflow.name", name },
            { "gen_ai.conversation.id", threadId }
        };

        return Source.StartActivity($"invoke_node {name}", ActivityKind.Internal, null, tags);
    }

    public static void AddNodeUsage(this Activity activity, AgentRunResponse agentRunResponse)
    {
        activity.SetTag("gen_ai.usage.input_tokens", agentRunResponse.Usage?.InputTokenCount );
        activity.SetTag("gen_ai.usage.output_tokens", agentRunResponse.Usage?.OutputTokenCount );
        activity.SetTag("gen_ai.usage.total_tokens", agentRunResponse.Usage?.TotalTokenCount );
    }

    public static void AddNodeInput(this Activity activity, string input)
    {
        activity.SetTag("gen_ai.task.input", Truncate(input));
    }

    public static void AddNodeOutput(this Activity activity, string output)
    {
        activity.SetTag("gen_ai.task.output", Truncate(output));
    }

    private static string Truncate(string value)
    {
        var truncated = value.Length > MaxPreviewLength;

        var preview = truncated ? value[..MaxPreviewLength] : value;

        if (truncated)
        {
            preview += " -[Truncated]";
        }

        return preview;

    }

}