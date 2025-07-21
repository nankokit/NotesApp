using FluentValidation;
using System.IO;

namespace NotesApp.Application.Commands;

public class CreateNoteCommandValidator : AbstractValidator<CreateNoteCommand>
{
    public CreateNoteCommandValidator()
    {
        RuleFor(note => note.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");
        RuleFor(note => note.Description)
            .NotEmpty().WithMessage("Description is required");
        RuleForEach(note => note.TagNames)
            .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters")
            .When(note => note.TagNames != null);
        RuleForEach(note => note.ImageFileNames)
            .NotEmpty().WithMessage("Image file name cannot be empty")
            .MaximumLength(200).WithMessage("Image file name cannot exceed 200 characters")
            .Must(BeValidFileName).WithMessage("Invalid file name format")
            .When(note => note.ImageFileNames != null);
    }

    private bool BeValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        try
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return !fileName.Any(c => invalidChars.Contains(c));
        }
        catch
        {
            return false;
        }
    }
}