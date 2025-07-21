using MediatR;

namespace NotesApp.Application.Commands;

public class UpdateTagCommand : IRequest
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
}