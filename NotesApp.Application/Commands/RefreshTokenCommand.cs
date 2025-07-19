using MediatR;

namespace NotesApp.Application.Commands;

public class RefreshTokenCommand : IRequest<string>
{
    public required string RefreshToken { get; set; }
}
