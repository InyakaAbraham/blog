using Blog.Models;

namespace Blog.Domain.Blog.Commands;

public class BlogCommandResponse
{
    public BlogCommandResponse(BlogPost post)
    {
        PostId = post.PostId;
        CoverImagePath = post.CoverImagePath;
        Title = post.Title;
        Summary = post.Summary;
        Body = post.Body;
        Tags = post.Tags;
        CategoryName = post.CategoryName;
        Category = post.Category;
        Author = post.Author;
        AuthorId = post.AuthorId;
        Created = post.Created;
        Updated = post.Updated;
    }

    public long PostId { get; set; }
    public string CoverImagePath { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
    public string Body { get; set; }
    public string? Tags { get; set; }
    public string? CategoryName { get; set; }
    public Category? Category { get; set; }
    public long AuthorId { get; set; }
    public Author? Author { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}
