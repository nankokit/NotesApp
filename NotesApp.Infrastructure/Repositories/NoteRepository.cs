using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NotesApp.Domain.Entities;
using NotesApp.Domain.Interfaces;
using NotesApp.Infrastructure.Data;

namespace NotesApp.Infrastructure.Repositories;

public class NoteRepository : INoteRepository
{
    private readonly NotesDbContext _context;

    public NoteRepository(NotesDbContext context) => _context = context;

    public async Task<IEnumerable<Note>> GetAllAsync(
        string? search, List<string>? tags, NoteSortField? sortBy,
        bool ascending, int page, int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _context.Notes.Include(note => note.Tags).AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(n =>
                EF.Functions.Like(n.Name.ToLower(), $"%{search.ToLower()}%") ||
                EF.Functions.Like(n.Description.ToLower(), $"%{search.ToLower()}%"));
        }

        if (tags != null && tags.Any())
        {
            query = query.Where(n =>
                _context.Set<Dictionary<string, object>>("NoteTag")
                    .Where(nt => EF.Property<Guid>(nt, "NoteId") == n.Id)
                    .Join(_context.Tags,
                        nt => EF.Property<Guid>(nt, "TagsId"),
                        t => t.Id,
                        (nt, t) => t.Name)
                    .Any(tName => tags.Contains(tName)));
        }

        if (sortBy.HasValue)
        {
            query = sortBy.Value switch
            {
                NoteSortField.Name => ascending ? query.OrderBy(n => n.Name) : query.OrderByDescending(n => n.Name),
                NoteSortField.CreationDate => ascending ? query.OrderBy(n => n.CreationDate) : query.OrderByDescending(n => n.CreationDate),
                _ => query.OrderBy(n => n.Id)
            };
        }
        else
            query = query.OrderBy(n => n.Id);

        var skip = (page - 1) * pageSize;
        return await query.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
    }

    public async Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.Notes.Include(n => n.Tags).FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task AddAsync(Note note, CancellationToken cancellationToken)
    {
        if (note == null) throw new ArgumentNullException(nameof(note));
        await _context.Notes.AddAsync(note, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Note note, CancellationToken cancellationToken)
    {
        if (note == null) throw new ArgumentNullException(nameof(note));
        _context.Notes.Update(note);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var note = await _context.Notes.FindAsync(new object[] { id }, cancellationToken);
        if (note == null) throw new KeyNotFoundException($"Note with ID {id} not found");
        _context.Notes.Remove(note);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountAsync(string? search, List<string>? tags, CancellationToken cancellationToken)
    {
        var query = _context.Notes.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(n =>
                EF.Functions.Like(n.Name.ToLower(), $"%{search.ToLower()}%") ||
                EF.Functions.Like(n.Description.ToLower(), $"%{search.ToLower()}%"));
        }

        if (tags != null && tags.Any())
        {
            query = query.Where(n =>
                _context.Set<Dictionary<string, object>>("NoteTag")
                    .Where(nt => EF.Property<Guid>(nt, "NoteId") == n.Id)
                    .Join(_context.Tags,
                        nt => EF.Property<Guid>(nt, "TagsId"),
                        t => t.Id,
                        (nt, t) => t.Name)
                    .Any(tName => tags.Contains(tName)));
        }

        return await query.CountAsync(cancellationToken);
    }
}