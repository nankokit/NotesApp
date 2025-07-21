using MediatR;
using Microsoft.Extensions.Logging;
using NotesApp.Application.Commands;
using NotesApp.Domain.Exceptions;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class UpdateTagCommandHandler : IRequestHandler<UpdateTagCommand>
{
    private readonly ITagRepository _tagRepository;
    private readonly INoteRepository _noteRepository;
    private readonly ILogger<UpdateTagCommandHandler> _logger;

    public UpdateTagCommandHandler(
        ITagRepository tagRepository,
        INoteRepository noteRepository,
        ILogger<UpdateTagCommandHandler> logger)
    {
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _noteRepository = noteRepository ?? throw new ArgumentNullException(nameof(noteRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tag == null)
        {
            _logger.LogWarning("Tag with ID {TagId} not found", request.Id);
            throw new ResourceNotFoundException("Tag", request.Id.ToString());
        }

        var existingTag = await _tagRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingTag != null && existingTag.Id != request.Id)
        {
            _logger.LogError("Tag with name {TagName} already exists", request.Name);
            throw new DuplicateResourceException("Tag", request.Name);
        }

        tag.Name = request.Name;
        await _tagRepository.UpdateAsync(tag, cancellationToken);
        _logger.LogInformation("Successfully updated tag with ID: {TagId}, Name: {TagName}", tag.Id, tag.Name);

        var notes = await _noteRepository.GetAllAsync(null, new List<string> { tag.Name }, null, true, 1, int.MaxValue, cancellationToken);
        foreach (var note in notes)
        {
            if (note.Tags != null)
            {
                var tagToUpdate = note.Tags.FirstOrDefault(t => t.Id == tag.Id);
                if (tagToUpdate != null)
                {
                    tagToUpdate.Name = request.Name;
                    await _noteRepository.UpdateAsync(note, cancellationToken);
                    _logger.LogInformation("Updated tag {TagName} in note ID: {NoteId}", request.Name, note.Id);
                }
            }
        }
    }
}