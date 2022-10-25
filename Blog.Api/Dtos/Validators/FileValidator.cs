using FluentValidation;

namespace Blog.Api.Dtos.Validators;

public class FileValidator : AbstractValidator<IFormFile>
{
    public FileValidator()
    {
        RuleFor(x => x.Length).NotNull().LessThanOrEqualTo(2000000)
            .WithMessage("File size is larger than allowed");

        RuleFor(x => x.ContentType).NotNull()
            .Must(x => x.Equals("image/jpeg") || x.Equals("image/jpg") || x.Equals("image/png"))
            .WithMessage("File type is not allowed");
    }
}