using MediatR;
namespace NotesApp.Application.Commands;

public class DeleteNoteCommand : IRequest
{
    public required Guid Id { get; set; }
}