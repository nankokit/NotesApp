using FluentValidation;

namespace NotesApp.Application.Commands.UpdateNote;

public class UpdateNoteCommandValidator : AbstractValidator<UpdateNoteCommand>
{
    public UpdateNoteCommandValidator()
    {
        RuleFor(note => note.Id).NotEmpty().WithMessage("Id is required");
        RuleFor(note => note.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");
        RuleForEach(note => note.TagNames)
            .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters");
        RuleForEach(note => note.ImageUrls)
            .Must(BeValidUrl).When(x => x.ImageUrls != null)
            .WithMessage("Invalid URL format")
            .MaximumLength(200).WithMessage("Image URL cannot exceed 200 characters");
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}