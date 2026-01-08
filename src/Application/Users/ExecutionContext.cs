namespace Application.Users;

public class ExecutionContext(Guid userId, Guid sessionId, Guid requestId) : IExecutionContext
{
    public Guid UserId { get; } = userId;

    public Guid SessionId { get;  } = sessionId;
    public Guid RequestId { get; } = requestId;
}

public interface IExecutionContext
{
    Guid UserId { get;  }
    Guid SessionId { get;  }
    Guid RequestId { get; }
}