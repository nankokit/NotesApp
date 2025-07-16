using NotesApp.Domain.Entities;

namespace NotesApp.Domain.Interfaces;

public interface INoteRepository
{
    Task<IEnumerable<Note>> GetAllAsync(string? search, List<string>? tags, string? sortBy, bool ascending, int page, int pageSize, CancellationToken cancellationToken);
    Task<Note> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Note note, CancellationToken cancellationToken);
    Task UpdateAsync(Note note, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<int> CountAsync(string? search, List<string>? tags, CancellationToken cancellationToken);
}