using Blog.Models;
using Blog.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Domain.Blog.Queries;

public class BlogQueryHandler: IRequestHandler<BlogQueryRequest, PagedList<BlogQueryResponse>>
{
    private readonly DataContext _dataContext;
    public BlogQueryHandler(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<PagedList<BlogQueryResponse>> Handle(BlogQueryRequest request, CancellationToken cancellationToken)
    {
        if (request.AllPost==true && string.IsNullOrEmpty(request.Tag) && request.Id==null)
        {
            try
            {
                var listPost = await (from bp in _dataContext.Set<BlogPost>()
                    join ar in _dataContext.Set<Author>() on bp.AuthorId equals ar.AuthorId
                    select new BlogQueryResponse
                    {
                        PostId = bp.PostId,
                        AuthorsName = ar.FirstName + " " + ar.LastName,
                        CoverImagePath = bp.CoverImagePath,
                        Title = bp.Title,
                        Summary = bp.Summary,
                        Body = bp.Body,
                        Tags = bp.Tags,
                        Category = bp.Category,
                        DateCreated = bp.Created
                    }).OrderByDescending(x => x.DateCreated).ToListAsync();
                return await PagedList<BlogQueryResponse>.ToPagedList(listPost, request.PageParameters.PageNumber, request.PageParameters.PageSize);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred: {ex.Message}");
                return null;
            }
        }
        return null;
    }
}
