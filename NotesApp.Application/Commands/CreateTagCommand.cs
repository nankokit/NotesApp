using MediatR;
using NotesApp.Application.DTOs;

namespace NotesApp.Application.Commands;

public class CreateTagCommand : IRequest<TagDto>
{
    public required string Name { get; set; }
}