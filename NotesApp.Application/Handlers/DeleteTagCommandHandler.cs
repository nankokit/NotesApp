using MediatR;
using Microsoft.Extensions.Logging;
using NotesApp.Application.Commands;
using NotesApp.Domain.Exceptions;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class DeleteTagCommandHandler : IRequestHandler<DeleteTagCommand>
{
    private readonly ITagRepository _tagRepository;
    private readonly INoteRepository _noteRepository;
    private readonly ILogger<DeleteTagCommandHandler> _logger;

    public DeleteTagCommandHandler(
        ITagRepository tagRepository,
        INoteRepository noteRepository,
        ILogger<DeleteTagCommandHandler> logger)
    {
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _noteRepository = noteRepository ?? throw new ArgumentNullException(nameof(noteRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tag == null)
        {
            _logger.LogWarning("Tag with ID {TagId} not found", request.Id);
            throw new ResourceNotFoundException("Tag", request.Id.ToString());
        }

        var notesWithTag = await _noteRepository.GetAllAsync(null, new List<string> { tag.Name }, null, true, 1, int.MaxValue, cancellationToken);
        if (notesWithTag.Any())
        {
            _logger.LogError("Cannot delete tag {TagName} because it is associated with {Count} note(s)", tag.Name, notesWithTag.Count());
            throw new ResourceInUseException("Tag", tag.Name, notesWithTag.Count());
        }

        await _tagRepository.DeleteAsync(request.Id, cancellationToken);
        _logger.LogInformation("Successfully deleted tag with ID: {TagId}", request.Id);
    }
}