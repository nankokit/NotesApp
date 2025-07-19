using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NotesApp.Application.Commands;
using NotesApp.Domain.Entities;
using NotesApp.Domain.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NotesApp.Application.Handlers;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, string>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;

    public RefreshTokenCommandHandler(IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
    }

    public async Task<string> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (refreshToken == null || refreshToken.ExpiryDate <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, refreshToken.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, refreshToken.User.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing in configuration.");
        var tokenExpiryStr = _configuration["Jwt:TokenExpiryMinutes"] ?? throw new InvalidOperationException("Jwt:TokenExpiryMinutes is missing in configuration.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(tokenExpiryStr)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}