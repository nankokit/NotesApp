namespace NotesApp.Domain.Interfaces;

public interface IMinioService
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken);
    Task<Stream> GetImageAsync(string fileName, CancellationToken cancellationToken);
    Task<string> GetPresignedUrlAsync(string fileName, CancellationToken cancellationToken);
    Task DeleteImageAsync(string fileName, CancellationToken cancellationToken);
}