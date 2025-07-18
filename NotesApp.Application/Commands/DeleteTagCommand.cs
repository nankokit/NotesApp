using MediatR;

namespace NotesApp.Application.Commands;

public class DeleteTagCommand : IRequest
{
    public Guid Id { get; set; }
}