using Agents.Middleware;
using Agents.Repository;
using Agents.Services;
using Agents.Settings;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace Agents.Extensions;

public static class AgentExtensions
{
    public static IServiceCollection AddAgentServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LanguageModelSettings>(settings =>
            configuration.GetSection("LanguageModelSettings").Bind(settings));


        services.AddSingleton<IAgentMemoryService, AgentMemoryService>();
        services.AddSingleton<IAgentMemoryMiddleware, AgentMemoryMiddleware>();
        services.AddSingleton<IAgentFactory, AgentFactory>();
        services.AddSingleton<IAgentTemplateRepository, AgentTemplateRepository>();

        return services;
    }

    public static string GetAgUiThreadId(this AgentRunOptions options)
    {
        if (options is not ChatClientAgentRunOptions chatClientAgentOptions)
        {
            throw new ArgumentException($"Invalid agent run options, must be of type : ChatClientAgentRunOptions. Type is : {options.GetType()}");
        }

        if (chatClientAgentOptions.ChatOptions == null)
        {
            throw new ArgumentException("The provided ChatClientAgentRunOptions must have ChatOptions set.");
        }

        if (chatClientAgentOptions.ChatOptions.AdditionalProperties == null)
        {
            throw new ArgumentException("The provided ChatClientAgentRunOptions must have ChatOptions.AdditionalProperties set.");
        }

        if (!chatClientAgentOptions.ChatOptions.AdditionalProperties.ContainsKey("ag_ui_thread_id"))
        {
            throw new ArgumentException("The provided ChatClientAgentRunOptions must have ChatOptions.AdditionalProperties['ag_ui_thread_id'] set.");
        }

        var additionalProperty = chatClientAgentOptions.ChatOptions.AdditionalProperties["ag_ui_thread_id"];

        if (additionalProperty == null)
        {
            throw new ArgumentException("The provided ChatClientAgentRunOptions must have ChatOptions.AdditionalProperties['ag_ui_thread_id'] not null.");
        }

        var threadId = additionalProperty.ToString();

        if (string.IsNullOrEmpty(threadId))
        {
            throw new ArgumentException("The provided ChatClientAgentRunOptions must have ChatOptions.AdditionalProperties['ag_ui_thread_id'] not empty.");
        }

        return threadId;
    }
}