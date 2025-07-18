using MediatR;
using NotesApp.Application.Commands;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class DeleteTagCommandHandler : IRequestHandler<DeleteTagCommand>
{
    private readonly ITagRepository _tagRepository;
    private readonly INoteRepository _noteRepository;

    public DeleteTagCommandHandler(ITagRepository tagRepository, INoteRepository noteRepository)
    {
        _tagRepository = tagRepository;
        _noteRepository = noteRepository;
    }

    public async Task Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tag == null)
        {
            throw new KeyNotFoundException($"Tag with ID '{request.Id}' not found");
        }

        var notesWithTag = await _noteRepository.GetAllAsync(null, new List<string> { tag.Name }, null, true, 1, int.MaxValue, cancellationToken);
        if (notesWithTag.Any())
        {
            throw new InvalidOperationException($"Cannot delete tag '{tag.Name}' because it is associated with {notesWithTag.Count()} note(s).");
        }

        await _tagRepository.DeleteAsync(request.Id, cancellationToken);
    }
}