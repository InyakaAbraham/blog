namespace Blog.Api.Dtos;

public class PaginatedDto<TResultDto> : SuccessResponseDto<TResultDto>
{
    public PaginatedDto(TResultDto data, PageInformation pageInfo) : base(data)
    {
        PageInfo = pageInfo;
    }

    public PageInformation PageInfo { get; set; }

    public class PageInformation
    {
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }
}