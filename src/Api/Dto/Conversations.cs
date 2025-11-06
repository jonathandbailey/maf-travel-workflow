namespace Api.Dto;

public record ConversationRequestDto(string Message, Guid SessionId);

public record ConversationResponseDto(string Message, Guid SessionId);