using System.Text.Json.Serialization;

namespace Blog.Models;

public class Author
{
    [JsonIgnore] public long AuthorId { get; set; }
    public string Username { get; set; }
    public string Description { get; set; }
    [JsonIgnore] public string EmailAddress { get; set; }
    [JsonIgnore] public string? PasswordHash { get; set; }
    [JsonIgnore] public List<Role> Roles { get; set; }
    [JsonIgnore] public List<BlogPost?> BlogPosts { get; set; }
}