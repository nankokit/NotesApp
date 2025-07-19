using MediatR;

namespace NotesApp.Application.Commands;

public class LoginCommand : IRequest<string>
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}