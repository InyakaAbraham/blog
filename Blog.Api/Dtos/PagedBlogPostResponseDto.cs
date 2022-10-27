using Blog.Models;

namespace Blog.Api.Dtos;

public class PagedBlogPostResponseDto : PaginatedDto<List<BlogPostResponse>>
{
    public PagedBlogPostResponseDto(List<BlogPostResponse> data, PageInformation pageInfo) : base(data, pageInfo)
    {
    }
}