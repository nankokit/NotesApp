using NotesApp.Domain.Interfaces;

namespace NotesApp.Domain.Entities;

public class Tag : IEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}
