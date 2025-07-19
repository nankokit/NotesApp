using MediatR;
using NotesApp.Application.Commands;
using NotesApp.Application.DTOs;
using NotesApp.Domain.Entities;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, UserDto>
{
    private readonly IUserRepository _userRepository;

    public RegisterCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Username already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _userRepository.AddAsync(user, cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username
        };
    }
}