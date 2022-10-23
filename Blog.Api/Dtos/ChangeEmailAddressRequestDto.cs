namespace Blog.Api.Dtos;

public class ChangeEmailAddressRequestDto
{
    public string oldEmailAddress { get; set; }
    public string newEmailAddress { get; set; }
}