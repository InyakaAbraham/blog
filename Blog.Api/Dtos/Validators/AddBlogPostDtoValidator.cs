using FluentValidation;

namespace Blog.Api.Dtos.Validators;

public class AddBlogPostDtoValidator : AbstractValidator<NewBlogPostDto>
{
    public AddBlogPostDtoValidator()
    {
        RuleFor(x => x.Body).NotEmpty().WithMessage("Field Required");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Field Required");
        RuleFor(x => x.Summary).NotEmpty().WithMessage("Field Required");
        RuleFor(x => x.Tags).NotEmpty().WithMessage("Field Required");
        RuleFor(x => x.CategoryName).NotEmpty().WithMessage("Field Required");
        RuleFor(x => x.CoverImage).SetValidator(new FileValidator()).NotEmpty().WithMessage("Check file reqirement");
    }
}