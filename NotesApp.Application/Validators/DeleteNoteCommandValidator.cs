using FluentValidation;
using NotesApp.Application.Commands;

namespace NotesApp.Application.Validators;

public class DeleteNoteCommandValidator : AbstractValidator<DeleteNoteCommand>
{
    public DeleteNoteCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}