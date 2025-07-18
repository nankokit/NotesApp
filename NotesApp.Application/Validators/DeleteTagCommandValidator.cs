using FluentValidation;
using NotesApp.Application.Commands;

namespace NotesApp.Application.Validators;

public class DeleteTagCommandValidator : AbstractValidator<DeleteTagCommand>
{
    public DeleteTagCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}