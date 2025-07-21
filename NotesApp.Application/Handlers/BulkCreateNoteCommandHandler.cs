using MediatR;
using Microsoft.Extensions.Logging;
using NotesApp.Application.Commands;
using NotesApp.Application.DTOs;
using NotesApp.Domain.Entities;
using NotesApp.Domain.Exceptions;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class BulkCreateNoteCommandHandler : IRequestHandler<BulkCreateNoteCommand, List<NoteDto>>
{
    private readonly INoteRepository _noteRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IMinioService _minioService;
    private readonly ILogger<BulkCreateNoteCommandHandler> _logger;

    public BulkCreateNoteCommandHandler(
        INoteRepository noteRepository,
        ITagRepository tagRepository,
        IMinioService minioService,
        ILogger<BulkCreateNoteCommandHandler> logger)
    {
        _noteRepository = noteRepository ?? throw new ArgumentNullException(nameof(noteRepository));
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _minioService = minioService ?? throw new ArgumentNullException(nameof(minioService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<NoteDto>> Handle(BulkCreateNoteCommand request, CancellationToken cancellationToken)
    {
        if (request?.Notes == null || !request.Notes.Any())
        {
            _logger.LogError("Bulk create note command contains no notes");
            throw new InvalidInputException("Notes collection cannot be null or empty");
        }

        var noteDtos = new List<NoteDto>();
        var notesToAdd = new List<Note>();

        foreach (var noteCommand in request.Notes)
        {
            if (string.IsNullOrEmpty(noteCommand.Name))
            {
                _logger.LogError("Note name is null or empty in bulk create command");
                throw new InvalidInputException("Note name cannot be null or empty");
            }

            var note = new Note
            {
                Id = Guid.NewGuid(),
                Name = noteCommand.Name,
                Description = noteCommand.Description,
                Tags = new List<Tag>(),
                ImageFileNames = noteCommand.ImageFileNames,
                CreationDate = DateTime.UtcNow
            };

            foreach (var tagName in noteCommand.TagNames ?? new List<string>())
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
            if (noteCommand.ImageFileNames != null)
            {
                foreach (var fileName in noteCommand.ImageFileNames)
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

            notesToAdd.Add(note);
            noteDtos.Add(new NoteDto
            {
                Id = note.Id,
                Name = note.Name,
                Description = note.Description,
                TagNames = note.Tags.Select(t => t.Name).ToList(),
                ImageUrls = imageUrls,
                CreationDate = note.CreationDate
            });
        }

        foreach (var note in notesToAdd)
        {
            await _noteRepository.AddAsync(note, cancellationToken);
            _logger.LogInformation("Successfully added note with ID: {NoteId}", note.Id);
        }

        _logger.LogInformation("Successfully created {Count} notes", noteDtos.Count);
        return noteDtos;
    }
}