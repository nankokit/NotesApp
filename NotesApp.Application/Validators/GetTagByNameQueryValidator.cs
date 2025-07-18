using FluentValidation;
using NotesApp.Application.Queries;

namespace NotesApp.Application.Validators;

public class GetTagByNameQueryValidator : AbstractValidator<GetTagByNameQuery>
{
    public GetTagByNameQueryValidator()
    {
        RuleFor(query => query.Name)
            .NotEmpty().WithMessage("Tag name is required")
            .MaximumLength(100).WithMessage("Tag name cannot exceed 100 characters");
    }
}