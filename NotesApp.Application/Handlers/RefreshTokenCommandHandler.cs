using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NotesApp.Application.Commands;
using NotesApp.Domain.Exceptions;
using NotesApp.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NotesApp.Application.Handlers;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, string>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (refreshToken == null || refreshToken.ExpiryDate <= DateTime.UtcNow)
        {
            _logger.LogWarning("Invalid or expired refresh token: {RefreshToken}", request.RefreshToken);
            throw new NotesApp.Domain.Exceptions.UnauthorizedAccessException("Invalid or expired refresh token");
        }

        var jwtKey = _configuration["Jwt:Key"];
        var tokenExpiryStr = _configuration["Jwt:TokenExpiryMinutes"];
        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(tokenExpiryStr))
        {
            _logger.LogError("Jwt:Key or Jwt:TokenExpiryMinutes is missing in configuration");
            throw new ConfigurationException("JWT configuration is missing");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, refreshToken.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, refreshToken.User.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(tokenExpiryStr)),
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        _logger.LogInformation("Successfully generated new access token for user ID: {UserId}", refreshToken.UserId);

        return accessToken;
    }
}