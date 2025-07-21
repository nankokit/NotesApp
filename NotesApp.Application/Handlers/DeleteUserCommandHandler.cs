using MediatR;
using Microsoft.Extensions.Logging;
using NotesApp.Application.Commands;
using NotesApp.Domain.Exceptions;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(IUserRepository userRepository, ILogger<DeleteUserCommandHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", request.Id);
            throw new ResourceNotFoundException("User", request.Id.ToString());
        }

        await _userRepository.DeleteAsync(request.Id, cancellationToken);
        _logger.LogInformation("Successfully deleted user with ID: {UserId}", request.Id);
    }
}