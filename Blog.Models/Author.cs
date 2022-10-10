using System.Text.Json.Serialization;

namespace Blog.Models;

public class User
{
    public long UserId { get; set; }
    public string Username { get; set; }
    public string EmailAddress { get; set; }
    public string Description { get; set; }
    [JsonIgnore] public string? PasswordHash { get; set; }
    public List<Role> Roles { get; set; }
    [JsonIgnore] public List<BlogPost?> BlogPosts { get; set; }
}
