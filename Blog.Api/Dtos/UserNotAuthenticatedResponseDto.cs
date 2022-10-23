using Blog.Models;

namespace Blog.Api.Dtos;

public class UserNotAuthenticatedResponseDto : ResponseDto<dynamic>
{
    public UserNotAuthenticatedResponseDto(string message = "Not Authenticated") : base(
        ResponseCode.UserNotAuthenticated, message, null)
    {
    }
}