using FluentValidation;

namespace Blog.Api.Dtos.Validators;

public class ChangeEmailAddressRequestDtoValidator : AbstractValidator<ChangeEmailAddressRequestDto>
{
    public ChangeEmailAddressRequestDtoValidator()
    {
        RuleFor(x => x.newEmailAddress).EmailAddress().NotEmpty().WithMessage("Enter a valid email format");
        RuleFor(x => x.oldEmailAddress).EmailAddress().NotEmpty().WithMessage("Enter a valid email format");
    }
}