namespace Application.Interfaces;

public interface IUserStreamingService
{
    Task Stream(Guid userId, string content, bool isEndOfStream);
    Task Status(Guid userId, string content);
    Task StreamEnd(Guid userId);
}