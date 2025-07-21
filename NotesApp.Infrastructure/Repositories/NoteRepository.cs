using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotesApp.Domain.Entities;
using NotesApp.Domain.Exceptions;
using NotesApp.Domain.Interfaces;
using NotesApp.Infrastructure.Data;

namespace NotesApp.Infrastructure.Repositories;

public class NoteRepository : INoteRepository
{
    private readonly NotesDbContext _context;
    private readonly ILogger<NoteRepository> _logger;

    public NoteRepository(NotesDbContext context, ILogger<NoteRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

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
        var notes = await query.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} notes", notes.Count);

        return notes;
    }

    public async Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var note = await _context.Notes.Include(n => n.Tags).FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        if (note == null)
            _logger.LogWarning("Note with ID {Id} not found", id);
        else
            _logger.LogInformation("Successfully retrieved note with ID: {Id}", id);

        return note;
    }

    public async Task AddAsync(Note note, CancellationToken cancellationToken)
    {
        if (note == null)
        {
            _logger.LogError("Attempted to add a null note");
            throw new InvalidInputException("Note cannot be null");
        }
        await _context.Notes.AddAsync(note, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully added note with ID: {Id}", note.Id);
    }

    public async Task UpdateAsync(Note note, CancellationToken cancellationToken)
    {
        if (note == null)
        {
            _logger.LogError("Attempted to update a null note");
            throw new InvalidInputException("Note cannot be null");
        }
        _context.Notes.Update(note);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully updated note with ID: {Id}", note.Id);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var note = await _context.Notes.FindAsync(new object[] { id }, cancellationToken);
        if (note == null)
            _logger.LogWarning("Note with ID {Id} not found for deletion", id);
        else
            _context.Notes.Remove(note);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully deleted note with ID: {Id}", id);
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

        var count = await query.CountAsync(cancellationToken);
        _logger.LogInformation("Total notes counted: {Count}", count);

        return count;
    }
}