namespace DevHabit.Application.Interfaces;

public interface IFileService
{
    Task<string> Upload(string base64Data);
    Task<string> Upload(string base64Data, string? existingPath);
    Task Delete(string? path);
}
