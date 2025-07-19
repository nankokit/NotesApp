using FluentValidation;
using NotesApp.Application.Queries;

namespace NotesApp.Application.Validators;

public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("User ID is required.");
    }
}