using MediatR;

namespace NotesApp.Application.Commands;

public class DeleteTagCommand : IRequest
{
    public required Guid Id { get; set; }
}