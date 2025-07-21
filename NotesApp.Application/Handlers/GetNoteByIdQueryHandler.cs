using MediatR;
using Microsoft.Extensions.Logging;
using NotesApp.Application.DTOs;
using NotesApp.Application.Queries;
using NotesApp.Domain.Exceptions;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class GetNoteByIdQueryHandler : IRequestHandler<GetNoteByIdQuery, NoteDto>
{
    private readonly INoteRepository _noteRepository;
    private readonly IMinioService _minioService;
    private readonly ILogger<GetNoteByIdQueryHandler> _logger;

    public GetNoteByIdQueryHandler(
        INoteRepository noteRepository,
        IMinioService minioService,
        ILogger<GetNoteByIdQueryHandler> logger)
    {
        _noteRepository = noteRepository ?? throw new ArgumentNullException(nameof(noteRepository));
        _minioService = minioService ?? throw new ArgumentNullException(nameof(minioService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<NoteDto> Handle(GetNoteByIdQuery request, CancellationToken cancellationToken)
    {
        var note = await _noteRepository.GetByIdAsync(request.Id, cancellationToken);
        if (note == null)
        {
            _logger.LogWarning("Note with ID {NoteId} not found", request.Id);
            throw new ResourceNotFoundException("Note", request.Id.ToString());
        }

        var imageUrls = new List<string>();
        if (note.ImageFileNames != null)
        {
            foreach (var fileName in note.ImageFileNames)
            {
                try
                {
                    var url = await _minioService.GetPresignedUrlAsync(fileName, cancellationToken);
                    imageUrls.Add(url);
                    _logger.LogInformation("Generated presigned URL for image: {FileName}", fileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate presigned URL for image: {FileName}", fileName);
                    throw new ResourceNotFoundException("Image", fileName);
                }
            }
        }

        var noteDto = new NoteDto
        {
            Id = note.Id,
            Name = note.Name,
            Description = note.Description,
            TagNames = note.Tags?.Select(tag => tag.Name).ToList(),
            ImageUrls = imageUrls,
            CreationDate = note.CreationDate
        };

        _logger.LogInformation("Successfully retrieved note with ID: {NoteId}", note.Id);

        return noteDto;
    }
}