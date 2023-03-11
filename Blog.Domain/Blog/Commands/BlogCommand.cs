using MediatR;
using Microsoft.AspNetCore.Http;

namespace Blog.Domain.Blog.Commands;

public class BlogCommand : IRequest<BlogCommandResponse>
{
    public IFormFile CoverImage { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
    public string Body { get; set; }
    public string Tags { get; set; }
    public string CategoryName { get; set; }
    public int CreateNew { get; set; }
    public long? PostId { get; set; }
}
