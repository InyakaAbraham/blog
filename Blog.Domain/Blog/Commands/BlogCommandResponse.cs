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

    private long PostId { get; set; }
    private string CoverImagePath { get; set; }
    private string Title { get; set; }
    private string Summary { get; set; }
    private string Body { get; set; }
    private string? Tags { get; set; }
    private string? CategoryName { get; set; }
    private Category? Category { get; set; }
    private long AuthorId { get; set; }
    private Author? Author { get; set; }
    private DateTime Created { get; set; }
    private DateTime Updated { get; set; }
}
