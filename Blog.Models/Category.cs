using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Blog.Models;

public class Category
{
    [Key]
    public string CategoryName { get; set; } = string.Empty;
    [JsonIgnore]
    public List<BlogPost>? BlogPosts { get; set; }
}