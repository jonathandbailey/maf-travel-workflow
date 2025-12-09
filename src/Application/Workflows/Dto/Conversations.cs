namespace Application.Workflows.Dto;

public sealed record ConversationRequest(Guid SessionId, Guid UserId, string Message, Guid ExchangeId);

public sealed record ConversationResponse(Guid SessionId, Guid UserId, string Message, Guid ExchangeId);