namespace Infrastructure.Interfaces;

public interface IArtifactRepository
{
    Task SaveAsync(string artifact, Guid id, string path);
    Task<string> LoadAsync(Guid id, string path);
}