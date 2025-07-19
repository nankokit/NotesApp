using NotesApp.Domain.Entities;

namespace NotesApp.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
}