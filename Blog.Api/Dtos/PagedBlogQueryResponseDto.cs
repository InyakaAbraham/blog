using Blog.Domain.Blog.Queries;

namespace Blog.Api.Dtos;

public class PagedBlogQueryResponseDto:PaginatedDto<List<BlogQueryResponse>>
{
    public PagedBlogQueryResponseDto(List<BlogQueryResponse> data, PageInformation pageInfo) : base(data, pageInfo)
    {
    }
}
