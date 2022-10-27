using FluentValidation;

namespace Blog.Api.Dtos.Validators;

public class LoginAuthorRequestDtoValidator : AbstractValidator<LoginAuthorRequestDto>
{
    public LoginAuthorRequestDtoValidator()
    {
        RuleFor(x => x.EmailAddress).EmailAddress().WithMessage("Enter a valid Email format");
        RuleFor(x => x.Password).MinimumLength(8).WithMessage("Password must be at least 8 characters");
    }
}