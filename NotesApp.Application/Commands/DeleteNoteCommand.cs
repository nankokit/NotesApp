using MediatR;
namespace NotesApp.Application.Commands;

public class DeleteNoteCommand : IRequest
{
    public Guid Id { get; set; }
}