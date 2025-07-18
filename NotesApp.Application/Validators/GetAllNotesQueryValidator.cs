using FluentValidation;
using NotesApp.Application.Handlers;
using NotesApp.Application.Queries;

namespace NotesApp.Application.Validators;

public class GetAllNotesQueryValidator : AbstractValidator<GetAllNotesQuery>
{
    public GetAllNotesQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");
        RuleFor(query => query.SortBy)
            .Must(sortBy => string.IsNullOrEmpty(sortBy) || new[] { "name", "creationdate" }.Contains(sortBy?.ToLower()))
            .WithMessage("SortBy must be 'name' or 'creationDate'");
        RuleFor(query => query.Search)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
            .When(query => query.Search != null);
        RuleForEach(query => query.Tags)
            .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters")
            .When(query => query.Tags != null);
    }
}