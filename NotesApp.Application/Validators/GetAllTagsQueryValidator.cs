using FluentValidation;
using NotesApp.Application.Queries;

namespace NotesApp.Application.Validators;

public class GetAllTagsQueryValidator : AbstractValidator<GetAllTagsQuery>
{
    public GetAllTagsQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");
        RuleFor(query => query.Search)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
            .When(query => query.Search != null);
    }
}