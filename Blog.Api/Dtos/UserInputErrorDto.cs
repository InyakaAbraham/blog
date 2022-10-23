using Blog.Models;
using FluentValidation.Results;

namespace Blog.Api.Dtos;

public class UserInputErrorDto : ResponseDto<List<ValidationError>>
{
    public UserInputErrorDto(ValidationResult validationResult, string message = "User Input Error") :
        base(ResponseCode.UserInputError, message, validationResult.Errors
            .Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList())
    {
    }

    public UserInputErrorDto(string message = "Invalid Input", params ValidationError[]? validationErrors) :
        base(ResponseCode.UserInputError,
            message, validationErrors?.ToList())
    {
    }
}