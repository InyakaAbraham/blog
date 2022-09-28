using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Blog.Models;

public class Author
{
    [Key]
    public int AuthorId {get; set;}
    public string Name { get; set; } = string.Empty;
    public string Description {get;set;} = string.Empty;

    [JsonIgnore]
    public List<BlogPost>? BlogPosts { get; set; }
}