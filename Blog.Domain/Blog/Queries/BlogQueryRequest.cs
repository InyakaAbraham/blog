using Blog.Models;
using MediatR;

namespace Blog.Domain.Blog.Queries;

public class BlogQueryRequest:IRequest<PagedList<BlogQueryResponse>>
{
    public PageParameters? PageParameters { get; set; }
    public bool? AllPost { get; set; }
    public long? Id { get; set; }
    public string? Tag { get; set; }
}
