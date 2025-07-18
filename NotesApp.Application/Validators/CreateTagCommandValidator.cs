using FluentValidation;
using NotesApp.Application.Commands;

namespace NotesApp.Application.Validators;

public class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name is required")
            .MaximumLength(100).WithMessage("Tag name cannot exceed 100 characters");
    }
}