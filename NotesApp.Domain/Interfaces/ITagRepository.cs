using NotesApp.Domain.Entities;

namespace NotesApp.Domain.Interfaces;

public interface ITagRepository
{
    Task<IEnumerable<Tag>> GetAllAsync(string? search, int page, int pageSize, CancellationToken cancellationToken);
    Task<Tag> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<Tag> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Tag tag, CancellationToken cancellationToken);
    Task UpdateAsync(Tag tag, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<int> CountAsync(string? search, CancellationToken cancellationToken);
}