namespace Blog.Api.Dtos;

public class ChangePasswordRequest
{
    public string EmailAddress { get; set; }
    public string OldPassword { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}