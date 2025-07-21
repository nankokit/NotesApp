using MediatR;

namespace NotesApp.Application.Commands;

public class LoginCommand : IRequest<(string AccessToken, string RefreshToken)>
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}