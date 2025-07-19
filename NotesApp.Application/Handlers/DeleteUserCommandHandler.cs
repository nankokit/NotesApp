using MediatR;
using NotesApp.Application.Commands;
using NotesApp.Domain.Interfaces;

namespace NotesApp.Application.Handlers;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        await _userRepository.DeleteAsync(request.Id, cancellationToken);
    }
}