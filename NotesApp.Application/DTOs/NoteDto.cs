namespace NotesApp.Application.DTOs;

public class NoteDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public List<string>? TagNames { get; set; }
    public List<string>? ImageUrls { get; set; }
    public DateTime CreationDate { get; set; }
}