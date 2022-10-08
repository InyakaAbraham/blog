using System.ComponentModel.DataAnnotations;

namespace Blog.Api.Dtos;

public class RegisterUserRequest
{
    [Required]
    [EmailAddress] 
    public string EmailAddress { get; set; }

    public string Username { get; set; }
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; }
}