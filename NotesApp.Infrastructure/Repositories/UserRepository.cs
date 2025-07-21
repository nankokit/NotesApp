using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotesApp.Domain.Entities;
using NotesApp.Domain.Exceptions;
using NotesApp.Domain.Interfaces;
using NotesApp.Infrastructure.Data;

namespace NotesApp.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly NotesDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(NotesDbContext context, ILogger<UserRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching user with username: {Username}", username);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        if (user == null)
            _logger.LogWarning("User with username {Username} not found", username);
        else
            _logger.LogInformation("Successfully retrieved user with username: {Username}", username);

        return user;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching user with ID: {Id}", id);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user == null)
            _logger.LogWarning("User with ID {Id} not found", id);
        else
            _logger.LogInformation("Successfully retrieved user with ID: {Id}", id);

        return user;
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        if (user == null)
        {
            _logger.LogError("Attempted to add a null user");
            throw new InvalidInputException("User cannot be null");
        }
        _logger.LogInformation("Adding user with ID: {Id}, Username: {Username}", user.Id, user.Username);
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully added user with ID: {Id}", user.Id);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting user with ID: {Id}", id);
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        if (user == null)
            _logger.LogWarning("User with ID {Id} not found for deletion", id);
        else
        {
            _context.Users.Remove(user);
            _logger.LogInformation("Successfully deleted user with ID: {Id}", id);
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
}