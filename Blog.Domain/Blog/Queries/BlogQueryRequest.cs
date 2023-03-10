using Blog.Models;
using MediatR;

namespace Blog.Domain.Blog.Queries;

public class BlogQueryRequest:IRequest<List<BlogQueryResponse>>
{
    public PageParameters? PageParameters { get; set; }
    public bool? AllPost { get; set; }
    public long? AuthorId { get; set; }
    public long? PostId { get; set; }
    public string? Tag { get; set; }
}
