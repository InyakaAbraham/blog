using System.Text.Json.Serialization;

namespace Blog.Models;

public class User
{
    public string Username { get; set; }

    [JsonIgnore] public string PasswordHash { get; set; }
}