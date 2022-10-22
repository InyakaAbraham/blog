namespace Blog.Models;

public enum ResponseCode
{
    Success = 0,
    ServerError = 1,
    UserInputError = 2,
    UserNotAuthenticated = 3,
    UserNotAuthorized = 4
}