using MediatR;

namespace NotesApp.Application.Commands;

public class DeleteUserCommand : IRequest
{
    public required Guid Id { get; set; }
}