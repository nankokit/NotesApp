using MediatR;
using Microsoft.Extensions.Logging;
using NotesApp.Application.Commands;
using NotesApp.Domain.Exceptions;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class DeleteNoteCommandHandler : IRequestHandler<DeleteNoteCommand>
{
    private readonly INoteRepository _noteRepository;
    private readonly ILogger<DeleteNoteCommandHandler> _logger;

    public DeleteNoteCommandHandler(INoteRepository noteRepository, ILogger<DeleteNoteCommandHandler> logger)
    {
        _noteRepository = noteRepository ?? throw new ArgumentNullException(nameof(noteRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _noteRepository.DeleteAsync(request.Id, cancellationToken);
            _logger.LogInformation("Successfully deleted note with ID: {NoteId}", request.Id);
        }
        catch (ResourceNotFoundException ex)
        {
            _logger.LogWarning("Failed to delete note with ID: {NoteId}. {Message}", request.Id, ex.Message);
            throw;
        }
    }
}