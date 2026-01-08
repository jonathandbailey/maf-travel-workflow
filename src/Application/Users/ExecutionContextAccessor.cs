namespace Application.Users;

public class ExecutionContextAccessor : IExecutionContextAccessor
{
    private IExecutionContext? _context;

    public IExecutionContext Context =>
        _context ?? throw new InvalidOperationException("SessionContext not initialized.");

    public void Initialize(Guid userId, Guid sessionId, Guid requestId)
    {
        if (_context != null)
            throw new InvalidOperationException("SessionContext already initialized.");

        _context = new ExecutionContext(userId, sessionId, requestId);
    }

    public void Initialize(Guid userId, Guid sessionId)
    {
        if (_context != null)
            throw new InvalidOperationException("SessionContext already initialized.");

        _context = new ExecutionContext(userId, sessionId, Guid.NewGuid());
    }
}


public interface IExecutionContextAccessor
{
    IExecutionContext Context { get; }
    void Initialize(Guid userId, Guid sessionId, Guid requestId);
    void Initialize(Guid userId, Guid sessionId);
}
