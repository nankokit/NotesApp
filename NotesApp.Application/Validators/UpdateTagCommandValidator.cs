using FluentValidation;
using NotesApp.Application.Commands;

namespace NotesApp.Application.Validators;

public class UpdateTagCommandValidator : AbstractValidator<UpdateTagCommand>
{
    public UpdateTagCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Id is required");
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Tag name is required")
            .MaximumLength(100).WithMessage("Tag name cannot exceed 100 characters");
    }
}