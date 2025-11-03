namespace ConsoleApp.Workflows.Conversations;

public class ConversationWorkFlowRequest
{
    public required string Message { get; init; }

    public required ConversationWorkflowState State { get; init; }

    public required Guid SessionId { get;init; }
}

public class ConversationWorkFlowResponse
{
    public required string Message { get; init; }

    public required ConversationWorkflowState State { get;init; }

    public required Guid SessionId { get; init; }
}

public enum ConversationWorkflowState
{
    Started,
    Completed,
    UserResponse,
    AssistantRequest

}