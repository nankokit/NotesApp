using MediatR;

namespace NotesApp.Application.Commands;

public class UpdateTagCommand : IRequest
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}