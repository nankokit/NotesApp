using FluentValidation;
using FluentValidation.Validators;

namespace NotesApp.Application.Commands;

public class CreateNoteCommandValidator : AbstractValidator<CreateNoteCommand>
{
    public CreateNoteCommandValidator()
    {
        RuleFor(note => note.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");
        RuleFor(note => note.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(1000).WithMessage("Description cannot exceed 100 characters");
        RuleForEach(note => note.TagNames)
            .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters");
        RuleForEach(note => note.ImageUrls)
            .MaximumLength(200).WithMessage("Image URL cannot exceed 200 characters")
            .When(note => note.ImageUrls != null);
    }
}