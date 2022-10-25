namespace Blog.Api.Dtos;

public class ChangePasswordRequestDto
{
    public string EmailAddress { get; set; }
    public string OldPassword { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}