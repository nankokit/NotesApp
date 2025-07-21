using MediatR;
using Microsoft.Extensions.Logging;
using NotesApp.Application.Commands;
using NotesApp.Domain.Exceptions;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, (string FileName, string Url)>
{
    private readonly IMinioService _minioService;
    private readonly ILogger<UploadImageCommandHandler> _logger;

    public UploadImageCommandHandler(IMinioService minioService, ILogger<UploadImageCommandHandler> logger)
    {
        _minioService = minioService ?? throw new ArgumentNullException(nameof(minioService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<(string FileName, string Url)> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var uniqueFileName = await _minioService.UploadImageAsync(request.FileStream, request.FileName, request.ContentType, cancellationToken);
            var presignedUrl = await _minioService.GetPresignedUrlAsync(uniqueFileName, cancellationToken);
            _logger.LogInformation("Successfully uploaded image: {FileName}, URL: {Url}", uniqueFileName, presignedUrl);
            return (uniqueFileName, presignedUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image: {FileName}", request.FileName);
            throw new FileOperationException("Upload", request.FileName, $"Failed to upload image: {request.FileName}", ex);
        }
    }
}