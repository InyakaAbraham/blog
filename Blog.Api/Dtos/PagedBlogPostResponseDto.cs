using Blog.Models;

namespace Blog.Api.Dtos;

public class PagedBlogPostResponseDto:PaginatedDto<List<BlogPost>>
{
    public PagedBlogPostResponseDto(List<BlogPost> data, PageInformation pageInfo) : base(data, pageInfo)
    {
    }
}