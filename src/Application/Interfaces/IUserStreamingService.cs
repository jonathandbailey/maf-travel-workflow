namespace Application.Interfaces;

public interface IUserStreamingService
{
    Task Stream(Guid userId, string content);
    Task Status(Guid userId, string content);
}