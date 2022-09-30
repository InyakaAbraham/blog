using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Blog.Models;

public class User
{
    [Key] public string Username { get; set; }

    [JsonIgnore] public byte[] PasswordHash { get; set; }

    [JsonIgnore] public byte[] PasswordSalt { get; set; }
}