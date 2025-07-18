using FluentValidation;
using NotesApp.Application.Commands;

namespace NotesApp.Application.Validators;

public class BulkCreateNoteCommandValidator : AbstractValidator<BulkCreateNoteCommand>
{
    public BulkCreateNoteCommandValidator()
    {
        RuleFor(command => command.Notes)
            .NotEmpty().WithMessage("Notes list cannot be empty")
            .NotNull().WithMessage("Notes list cannot be null");
        RuleForEach(command => command.Notes)
            .SetValidator(new CreateNoteCommandValidator());
    }
}