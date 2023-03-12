using System.Text.Json.Serialization;
using Blog.Models;

namespace Blog.Domain.User.Queries;

public class UserQueryResponse
{
    public UserQueryResponse(Author? author)
    {
        AuthorId = author.AuthorId;
        Username = author.Username;
        FirstName = author.FirstName;
        LastName = author.LastName;
        Description = author.Description;
        EmailAddress = author.Description;
        PasswordHash = author.PasswordHash;
        VerifiedAt = author.VerifiedAt;
        LastLogin = (DateTime)author.LastLogin;
        CreatedAt = author.CreatedAt;
        Roles = author.Roles;
        BlogPosts = author.BlogPosts;
    }

    [JsonIgnore] public long AuthorId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Description { get; set; }
    [JsonIgnore] public string? EmailAddress { get; set; }
    [JsonIgnore] public string? PasswordHash { get; set; }
    [JsonIgnore] public DateTime? VerifiedAt { get; set; }
    [JsonIgnore] public DateTime LastLogin { get; set; }
    [JsonIgnore] public DateTime CreatedAt { get; set; }
    [JsonIgnore] public List<Role?> Roles { get; set; }
    [JsonIgnore] public List<BlogPost> BlogPosts { get; set; }
}
