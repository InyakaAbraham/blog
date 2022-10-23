using FluentValidation;

namespace Blog.Api.Dtos.Validators;

public class RegisterAuthorRequestValidator:AbstractValidator<RegisterAuthorRequest>
{
    public RegisterAuthorRequestValidator()
    {
        RuleFor(x => x.EmailAddress).EmailAddress().WithMessage("Enter a valid Email format");
        RuleFor(x => x.Username).NotEmpty().MinimumLength(4).WithMessage("Username must be at least 4 characters");
        RuleFor(x => x.Password).MinimumLength(8).WithMessage("Password must be at least 8 character");
        RuleFor(x => x.Description).MinimumLength(250);
    }
}