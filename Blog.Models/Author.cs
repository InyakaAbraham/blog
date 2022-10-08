using System.Text.Json.Serialization;

namespace Blog.Models;

public class Author
{
    public int AuthorId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    [JsonIgnore] public List<BlogPost>? BlogPosts { get; set; }
}