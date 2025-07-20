using MediatR;
using NotesApp.Application.Commands;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, (string FileName, string Url)>
{
    private readonly IMinioService _minioService;

    public UploadImageCommandHandler(IMinioService minioService)
    {
        _minioService = minioService;
    }

    public async Task<(string FileName, string Url)> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        var uniqueFileName = await _minioService.UploadImageAsync(request.FileStream, request.FileName, request.ContentType, cancellationToken);
        var presignedUrl = await _minioService.GetPresignedUrlAsync(uniqueFileName, cancellationToken);

        return (uniqueFileName, presignedUrl);
    }
}