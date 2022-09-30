using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Blog.Models;

public class BlogPost
{
    [Key] public int PostId { get; set; }

    public string Title { get; set; }
    public string Summary { get; set; }
    public string Body { get; set; }
    public string[]? Tags { get; set; }

    [ForeignKey("Category")] [JsonIgnore] public string CategoryName { get; set; }

    public Category? Category { get; set; }

    [ForeignKey("Author")] [JsonIgnore] public int AuthorId { get; set; }

    public Author? Author { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Updated { get; set; } = DateTime.UtcNow;
}