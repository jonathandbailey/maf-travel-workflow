namespace Application.Workflows.Conversations.Dto;

public sealed record ConversationRequest(Guid SessionId, string Message);

public sealed record ConversationResponse(Guid SessionId, string Message);