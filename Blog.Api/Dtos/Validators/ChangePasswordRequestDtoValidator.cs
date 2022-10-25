using FluentValidation;

namespace Blog.Api.Dtos.Validators;

public class ChangePasswordRequestDtoValidator : AbstractValidator<ChangePasswordRequestDto>
{
    public ChangePasswordRequestDtoValidator()
    {
        RuleFor(x => x.EmailAddress).EmailAddress();
        RuleFor(x => x.OldPassword).MinimumLength(8);
        RuleFor(x => x.Password).MinimumLength(8);
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .MinimumLength(8).WithMessage("Password do not match");
    }
}