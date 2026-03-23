using DevHabit.Application.Interfaces;

namespace DevHabit.Infrastructure.Services;

public sealed class FileService : IFileService
{
    private readonly string _basePath;

    public FileService(string basePath = "uploads")
    {
        _basePath = basePath;
    }

    public Task<string> Upload(string base64Data)
    {
        return Task.FromResult(base64Data);
    }

    public Task<string> Upload(string base64Data, string? existingPath)
    {
        return Task.FromResult(base64Data);
    }

    public Task Delete(string? path)
    {
        return Task.CompletedTask;
    }

    public string GetBasePath() => _basePath;
}
