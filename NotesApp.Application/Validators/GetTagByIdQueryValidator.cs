using FluentValidation;
using NotesApp.Application.Queries;

namespace NotesApp.Application.Validators;

public class GetTagByIdQueryValidator : AbstractValidator<GetTagByIdQuery>
{
    public GetTagByIdQueryValidator()
    {
        RuleFor(query => query.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}