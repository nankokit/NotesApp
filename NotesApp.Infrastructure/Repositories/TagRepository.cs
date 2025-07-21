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

public class TagRepository : ITagRepository
{
    private readonly NotesDbContext _context;
    private readonly ILogger<TagRepository> _logger;

    public TagRepository(NotesDbContext context, ILogger<TagRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Tag>> GetAllAsync(string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Tags.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(t => t.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var skip = (page - 1) * pageSize;
        var tags = await query.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
        _logger.LogInformation("Retrieved {Count} tags", tags.Count);

        return tags;
    }

    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (tag == null)
            _logger.LogWarning("Tag with ID {Id} not found", id);
        else
            _logger.LogInformation("Successfully retrieved tag with ID: {Id}", id);

        return tag;
    }

    public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
        if (tag == null)
            _logger.LogWarning("Tag with name {Name} not found", name);
        else
            _logger.LogInformation("Successfully retrieved tag with name: {Name}", name);
        return tag;
    }

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken)
    {
        if (tag == null)
        {
            _logger.LogError("Attempted to add a null tag");
            throw new InvalidInputException("Tag cannot be null");
        }
        await _context.Tags.AddAsync(tag, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully added tag with ID: {Id}", tag.Id);
    }

    public async Task UpdateAsync(Tag tag, CancellationToken cancellationToken)
    {
        if (tag == null)
        {
            _logger.LogError("Attempted to update a null tag");
            throw new InvalidInputException("Tag cannot be null");
        }
        _context.Tags.Update(tag);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully updated tag with ID: {Id}", tag.Id);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tag = await _context.Tags.FindAsync(new object[] { id }, cancellationToken);
        if (tag == null)
            _logger.LogWarning("Tag with ID {Id} not found for deletion", id);
        else
        {
            _context.Tags.Remove(tag);
            _logger.LogInformation("Successfully deleted tag with ID: {Id}", id);
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountAsync(string? search, CancellationToken cancellationToken)
    {
        var query = _context.Tags.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(t => t.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var count = await query.CountAsync(cancellationToken);
        _logger.LogInformation("Total tags counted: {Count}", count);

        return count;
    }
}