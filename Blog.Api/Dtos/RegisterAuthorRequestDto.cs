using System.ComponentModel.DataAnnotations;

namespace Blog.Api.Dtos;

public class RegisterAuthorRequestDto
{
    [Required] public string EmailAddress { get; set; }
    public string FirstName { get; set; }
    public string? LastName { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    [Required] public string Description { get; set; }
}