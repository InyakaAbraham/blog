using Blog.Models;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Blog.Api.Dtos;

public class UserInputErrorDto:ResponseDto<List<ValidationError>>
{
    public UserInputErrorDto(ValidationResult validationResult,string message="User Input Error" ) : 
        base(ResponseCode.UserInputError, message, validationResult.Errors
            .Select(x=>new ValidationError(x.PropertyName,x.ErrorMessage)).ToList())
    {
    }
}