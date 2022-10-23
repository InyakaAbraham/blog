using Blog.Models;

namespace Blog.Api.Dtos;

public class UserNotAuthorizedResponseDto : ResponseDto<dynamic>
{
    public UserNotAuthorizedResponseDto(string message = "Not Authorized") : base(ResponseCode.UserNotAuthorized,
        message, null)
    {
    }
}