using Blog.Models;
using Blog.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Domain.Blog.Queries;

public class BlogQueryHandler : IRequestHandler<BlogQueryRequest, List<BlogQueryResponse>>
{
    private readonly DataContext _dataContext;

    public BlogQueryHandler(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<List<BlogQueryResponse>> Handle(BlogQueryRequest request, CancellationToken cancellationToken)
    {
        try
        {
            IQueryable<BlogPost> query = _dataContext.BlogPosts.AsQueryable();

            if (!(request.AllPost ?? false))
            {
                if (request.PostId != null)
                {
                    query = query.Where(x => x.PostId == request.PostId);
                }
                else if (request.AuthorId != null)
                {
                    query = query.Where(x => x.AuthorId == request.AuthorId);
                }
                else if (!string.IsNullOrEmpty(request.Tag))
                {
                    query = query.Where(x => x.Tags != null && x.Tags.Contains(request.Tag));
                }
                else
                {
                    query = query.OrderByDescending(x => x.Created).Take(3);
                }

                query = query.Include(x => x.Author).Include(x => x.Category);
            }
            else
            {
                query = query.Include(x => x.Author).Include(x => x.Category).OrderByDescending(x => x.Created);
            }

            List<BlogQueryResponse> posts = await query
                .Select(x => new BlogQueryResponse
                {
                    PostId = x.PostId,
                    AuthorsName = $"{x.Author!.FirstName} {x.Author.LastName}",
                    CoverImagePath = x.CoverImagePath,
                    Title = x.Title,
                    Summary = x.Summary,
                    Body = x.Body,
                    Tags = x.Tags,
                    Category = x.Category,
                    DateCreated = x.Created,
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return posts;
        }
        catch (Exception ex)
        {
            throw new ArgumentNullException(nameof(request), $"An exception was thrown in blog query handler: {ex}");
        }
    }
}
