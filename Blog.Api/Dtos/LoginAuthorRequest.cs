using System.ComponentModel.DataAnnotations;

namespace Blog.Api.Dtos;

public class LoginAuthorRequest
{
    [Required] public string EmailAddress { get; set; }
    [Required] public string Password { get; set; }
}