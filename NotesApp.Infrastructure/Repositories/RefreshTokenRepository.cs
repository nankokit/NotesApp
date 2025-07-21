using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotesApp.Domain.Entities;
using NotesApp.Domain.Exceptions;
using NotesApp.Domain.Interfaces;
using NotesApp.Infrastructure.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NotesApp.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly NotesDbContext _context;
    private readonly ILogger<RefreshTokenRepository> _logger;

    public RefreshTokenRepository(NotesDbContext context, ILogger<RefreshTokenRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        _logger.LogInformation("Successfully retrieved refresh token: {Token}", token);

        return refreshToken;
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        if (refreshToken == null)
        {
            _logger.LogError("Attempted to add a null refresh token");
            throw new InvalidInputException("Refresh token cannot be null");
        }
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully added refresh token for user ID: {UserId}", refreshToken.UserId);
    }
}