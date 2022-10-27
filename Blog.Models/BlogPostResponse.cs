namespace Blog.Models;

public class BlogPostResponse
{
    public string AuthorsName { get; set; }
    public string CoverImagePath { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
    public string Body { get; set; }
    public string Tags { get; set; }
    public Category Category { get; set; }
    public DateTime DateCreated { get; set; }
}