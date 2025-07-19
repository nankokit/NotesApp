using NotesApp.Domain.Interfaces;

namespace NotesApp.Domain.Entities;

public class Note : IEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public List<Tag>? Tags { get; set; }
    public List<string>? ImageUrls { get; set; }
    public DateTime CreationDate { get; set; }
}
