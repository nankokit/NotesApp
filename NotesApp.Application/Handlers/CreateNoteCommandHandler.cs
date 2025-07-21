using MediatR;
using Microsoft.Extensions.Logging;
using NotesApp.Application.Commands;
using NotesApp.Application.DTOs;
using NotesApp.Domain.Entities;
using NotesApp.Domain.Exceptions;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class CreateNoteCommandHandler : IRequestHandler<CreateNoteCommand, NoteDto>
{
    private readonly INoteRepository _noteRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IMinioService _minioService;
    private readonly ILogger<CreateNoteCommandHandler> _logger;

    public CreateNoteCommandHandler(
        INoteRepository noteRepository,
        ITagRepository tagRepository,
        IMinioService minioService,
        ILogger<CreateNoteCommandHandler> logger)
    {
        _noteRepository = noteRepository ?? throw new ArgumentNullException(nameof(noteRepository));
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _minioService = minioService ?? throw new ArgumentNullException(nameof(minioService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<NoteDto> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Tags = new List<Tag>(),
            ImageFileNames = request.ImageFileNames,
            CreationDate = DateTime.UtcNow
        };

        foreach (var tagName in request.TagNames ?? new List<string>())
        {
            var tag = await _tagRepository.GetByNameAsync(tagName, cancellationToken);
            if (tag == null)
            {
                tag = new Tag { Id = Guid.NewGuid(), Name = tagName };
                await _tagRepository.AddAsync(tag, cancellationToken);
                _logger.LogInformation("Created new tag with name: {TagName}", tagName);
            }
            note.Tags.Add(tag);
        }

        var imageUrls = new List<string>();
        if (request.ImageFileNames != null)
        {
            foreach (var fileName in request.ImageFileNames)
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

        await _noteRepository.AddAsync(note, cancellationToken);
        _logger.LogInformation("Successfully created note with ID: {NoteId}", note.Id);

        return new NoteDto
        {
            Id = note.Id,
            Name = note.Name,
            Description = note.Description,
            TagNames = note.Tags.Select(t => t.Name).ToList(),
            ImageUrls = imageUrls,
            CreationDate = note.CreationDate
        };
    }
}