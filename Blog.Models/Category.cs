using System.Text.Json.Serialization;

namespace Blog.Models;

public class Category
{
    public string? CategoryName { get; set; }

    [JsonIgnore] public List<BlogPost>? BlogPosts { get; set; }
}