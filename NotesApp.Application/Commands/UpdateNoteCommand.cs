using MediatR;
namespace NotesApp.Application.Commands;

public class UpdateNoteCommand : IRequest
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public List<string> TagNames { get; set; } = new List<string>();
    public List<string>? ImageUrls { get; set; }
}