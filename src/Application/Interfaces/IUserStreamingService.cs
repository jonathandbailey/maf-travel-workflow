namespace Application.Interfaces;

public interface IUserStreamingService
{
    Task Stream(Guid userId, string content);
}