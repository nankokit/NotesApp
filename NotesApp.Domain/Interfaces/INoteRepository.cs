using NotesApp.Domain.Entities;

namespace NotesApp.Domain.Interfaces;

public interface INoteRepository
{
    Task<IEnumerable<Note>> GetAllAsync(string search, List<string> tags, string sortBy, bool ascending, int page, int pageSize);
    Task<Note> GetByIdAsync(Guid id);
    Task AddAsync(Note note);
    Task UpdateAsync(Note note);
    Task DeleteAsync(Guid id);
    Task<int> CountAsync(string search, List<string> tags);
}