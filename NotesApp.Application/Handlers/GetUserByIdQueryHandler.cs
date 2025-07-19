using MediatR;
using NotesApp.Application.DTOs;
using NotesApp.Application.Queries;
using NotesApp.Domain.Entities;
using NotesApp.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NotesApp.Application.Handlers;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username
        };
    }
}