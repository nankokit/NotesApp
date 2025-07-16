using MediatR;
using NotesApp.Application.DTOs;

namespace NotesApp.Application.Commands;

public class CreateNoteCommand : IRequest<NoteDto>
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public List<string> TagNames { get; set; } = new List<string>();
    public List<string>? ImageUrls { get; set; }
}