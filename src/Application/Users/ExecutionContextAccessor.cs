namespace Application.Users;

public class ExecutionContextAccessor : IExecutionContextAccessor
{
    private static readonly AsyncLocal<IExecutionContext?> _context = new();

    public IExecutionContext Context =>
        _context.Value ?? throw new InvalidOperationException("SessionContext not initialized.");

    public void Initialize(Guid userId, Guid sessionId, Guid requestId)
    {
        if (_context == null)
            throw new InvalidOperationException("SessionContext already initialized.");

        _context.Value = new ExecutionContext(userId, sessionId, requestId);
    }

    public void Initialize(Guid userId, Guid sessionId)
    {
        if (_context == null)
            throw new InvalidOperationException("SessionContext already initialized.");

        _context.Value = new ExecutionContext(userId, sessionId, Guid.NewGuid());
    }
}


public interface IExecutionContextAccessor
{
    IExecutionContext Context { get; }
    void Initialize(Guid userId, Guid sessionId, Guid requestId);
    void Initialize(Guid userId, Guid sessionId);
}
