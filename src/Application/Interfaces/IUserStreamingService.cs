namespace Application.Interfaces;

public interface IUserStreamingService
{
    Task Stream(string content, bool isEndOfStream);
    Task Status(string content, string details, string source);
    Task Artifact(string key);
    Task TravelPlan();
}