namespace Blog.Api.Dtos;

public class UpdateAuthorRequestDto
{
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Description { get; set; }
}