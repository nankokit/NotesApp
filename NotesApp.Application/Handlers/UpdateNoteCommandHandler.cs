using MediatR;
using Microsoft.Extensions.Logging;
using NotesApp.Application.Commands;
using NotesApp.Domain.Entities;
using NotesApp.Domain.Exceptions;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class UpdateNoteCommandHandler : IRequestHandler<UpdateNoteCommand>
{
    private readonly INoteRepository _noteRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IMinioService _minioService;
    private readonly ILogger<UpdateNoteCommandHandler> _logger;

    public UpdateNoteCommandHandler(
        INoteRepository noteRepository,
        ITagRepository tagRepository,
        IMinioService minioService,
        ILogger<UpdateNoteCommandHandler> logger)
    {
        _noteRepository = noteRepository ?? throw new ArgumentNullException(nameof(noteRepository));
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _minioService = minioService ?? throw new ArgumentNullException(nameof(minioService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await _noteRepository.GetByIdAsync(request.Id, cancellationToken);
        if (note == null)
        {
            _logger.LogWarning("Note with ID {NoteId} not found", request.Id);
            throw new ResourceNotFoundException("Note", request.Id.ToString());
        }

        note.Name = request.Name;
        note.Description = request.Description;
        note.ImageFileNames = request.ImageFileNames;

        note.Tags?.Clear();
        note.Tags = new List<Tag>();

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

        await _noteRepository.UpdateAsync(note, cancellationToken);
        _logger.LogInformation("Successfully updated note with ID: {NoteId}", note.Id);
    }
}