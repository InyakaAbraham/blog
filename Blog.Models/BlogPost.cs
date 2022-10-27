using System.Text.Json.Serialization;

namespace Blog.Models;

public class BlogPost
{
    public long PostId { get; set; }
    public string CoverImagePath { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
    public string Body { get; set; }
    public string? Tags { get; set; }
    [JsonIgnore] public string? CategoryName { get; set; }
    public Category? Category { get; set; }
    [JsonIgnore] public long AuthorId { get; set; }
    public Author? Author { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}