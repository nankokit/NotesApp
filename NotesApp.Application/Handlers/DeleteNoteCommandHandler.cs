using MediatR;
using NotesApp.Application.Commands;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class DeleteNoteCommandHandler : IRequestHandler<DeleteNoteCommand>
{
    private readonly INoteRepository _noteRepository;

    public DeleteNoteCommandHandler(INoteRepository noteRepository) => _noteRepository = noteRepository;

    public async Task Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        await _noteRepository.DeleteAsync(request.Id, cancellationToken);
    }
}