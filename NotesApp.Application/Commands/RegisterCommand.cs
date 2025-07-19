using MediatR;
using NotesApp.Application.DTOs;
namespace NotesApp.Application.Commands;

public class RegisterCommand : IRequest<UserDto>
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}