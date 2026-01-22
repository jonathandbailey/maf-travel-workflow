using Agents.Services;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Travel.Workflows.Repository;
using Travel.Workflows.Services;
using Workflows.Interfaces;

namespace Travel.Workflows.Extensions;

public static class WorkflowServiceExtensions
{
    public static IServiceCollection AddWorkflowServices(this IServiceCollection services)
    {
        services.AddSingleton<IWorkflowFactory, WorkflowFactory>();
        services.AddSingleton<IWorkflowRepository, WorkflowRepository>();

        services.AddSingleton<ITravelService, TravelService>();
        services.AddSingleton<IFlightService, FlightService>();

        services.AddSingleton<ICheckpointRepository, CheckpointRepository>();

        services.AddSingleton<IA2AAgentServiceDiscovery, A2AAgentServiceDiscovery>();

        return services;
    }

    public static async Task<Guid> GetThreadId(this IWorkflowContext context, CancellationToken cancellationToken)
    {
        return await context.ReadStateAsync<Guid>("agent_thread_id", scopeName: "workflow", cancellationToken);
    }

    public static async Task AddThreadId(this IWorkflowContext context, Guid threadId, CancellationToken cancellationToken)
    {
        await context.QueueStateUpdateAsync("agent_thread_id", threadId, scopeName: "workflow", cancellationToken: cancellationToken);
    }

    public static ChatClientAgentRunOptions ToChatClientAgentRunOptions(this Guid threadId)
    {
        var chatOptions = new ChatClientAgentRunOptions()
        {
            ChatOptions = new ChatOptions()
            {
                AdditionalProperties = new AdditionalPropertiesDictionary()
                {
                    { "agent_thread_id", threadId }
                }
            }
        };

        return chatOptions;
    }

    public class NullableDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();

                // Handle empty strings or the string "null"
                if (string.IsNullOrWhiteSpace(stringValue) || stringValue.Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                // Try to parse the date string
                if (DateTime.TryParse(stringValue, out var dateTime))
                {
                    return dateTime;
                }
            }

            // If we can't parse it, return null instead of throwing
            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}