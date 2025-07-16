using NotesApp.Domain.Entities;

namespace NotesApp.Domain.Interfaces;

public interface ITagRepository
{
    Task AddAsync(Tag tag);
    Task<Tag> GetByNameAsync(string name);
    Task<IEnumerable<Tag>> GetAllAsync();
}