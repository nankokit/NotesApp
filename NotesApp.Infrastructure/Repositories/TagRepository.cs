using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NotesApp.Application.DTOs;
using NotesApp.Domain.Entities;
using NotesApp.Domain.Interfaces;
using NotesApp.Infrastructure.Data;

namespace NotesApp.Infrastructure.Repositories;

public class TagRepository : ITagRepository
{
    private readonly NotesDbContext _context;

    public TagRepository(NotesDbContext context) => _context = context;

    public async Task<IEnumerable<Tag>> GetAllAsync(string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Tags.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(t => t.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var skip = (page - 1) * pageSize;
        return await query.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
    }

    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => await _context.Tags.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken) => await _context.Tags.FirstOrDefaultAsync(t => t.Name == name, cancellationToken);

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken)
    {
        if (tag == null) throw new ArgumentNullException(nameof(tag));
        await _context.Tags.AddAsync(tag, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Tag tag, CancellationToken cancellationToken)
    {
        _context.Tags.Update(tag);
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tag = await _context.Tags.FindAsync(new object[] { id }, cancellationToken);
        if (tag == null) throw new KeyNotFoundException($"Tag with ID {id} not found");
        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountAsync(string? search, CancellationToken cancellationToken)
    {
        var query = _context.Tags.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(t => t.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return await query.CountAsync(cancellationToken);
    }
}
