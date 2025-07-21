using FluentValidation;
using NotesApp.Application.Commands;
using System.IO;

namespace NotesApp.Application.Validators;

public class UploadImageCommandValidator : AbstractValidator<UploadImageCommand>
{
    public UploadImageCommandValidator()
    {
        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("File stream is required")
            .Must(stream => stream.Length > 0).WithMessage("File cannot be empty");
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .MaximumLength(100).WithMessage("File name cannot exceed 100 characters")
            .Must(BeValidFileName).WithMessage("Invalid file name format")
            .Must(HaveValidImageExtension).WithMessage("File must be an image with extension .jpg, .jpeg, .png, .gif, .bmp, or .webp");
        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required")
            .Must(BeValidImageContentType).WithMessage("Invalid image content type. Supported types: image/jpeg, image/png, image/gif, image/bmp, image/webp");
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

    private bool HaveValidImageExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        return validExtensions.Contains(extension);
    }

    private bool BeValidImageContentType(string contentType)
    {
        var validContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp" };
        return validContentTypes.Contains(contentType.ToLowerInvariant());
    }
}