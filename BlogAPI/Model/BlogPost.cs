using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace BlogAPI.Model;

public class BlogPost
{
    [Key]
    public int PostId {get; set;}
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string[] Tags { get; set; }

    [ForeignKey("Category")]
    [JsonIgnore]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [ForeignKey("Author")]
    [JsonIgnore]
    public int AuthorId { get; set; }
    public Author? Author { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;

    public DateTime Updated { get; set; } = DateTime.UtcNow;
}