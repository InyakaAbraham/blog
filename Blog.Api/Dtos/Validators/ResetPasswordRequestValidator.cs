using FluentValidation;

namespace Blog.Api.Dtos.Validators;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.emailAddress).EmailAddress();
        RuleFor(x => x.Token).Length(6);
        RuleFor(x => x.Password).MinimumLength(8);
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .MinimumLength(8).WithMessage("Password do not match");
    }
}