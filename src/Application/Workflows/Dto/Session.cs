namespace Application.Workflows.Dto;

public record SessionState(Guid SessionId, Guid UserId, Guid RequestId);