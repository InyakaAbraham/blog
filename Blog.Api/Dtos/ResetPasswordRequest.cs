namespace Blog.Api.Dtos;

public class ResetPasswordRequest
{
    public string emailAddress { get; set; }
    public string Token { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}