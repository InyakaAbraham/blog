using FluentValidation;

namespace Blog.Api.Dtos.Validators;

public class UpdateAuthorRequestDtoValidator : AbstractValidator<UpdateAuthorRequestDto>
{
    public UpdateAuthorRequestDtoValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(4).WithMessage("Username must be at least 4 characters");
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("Must have First Name");
        RuleFor(x => x.Description).MinimumLength(250);
    }
}