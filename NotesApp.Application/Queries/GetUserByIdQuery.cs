using MediatR;
using NotesApp.Application.DTOs;

namespace NotesApp.Application.Queries;

public class GetUserByIdQuery : IRequest<UserDto>
{
    public required Guid Id { get; set; }
}