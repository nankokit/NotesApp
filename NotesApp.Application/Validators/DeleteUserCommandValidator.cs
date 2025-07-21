using FluentValidation;
using NotesApp.Application.Commands;

namespace NotesApp.Application.Validators;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("User ID is required.");
    }
}