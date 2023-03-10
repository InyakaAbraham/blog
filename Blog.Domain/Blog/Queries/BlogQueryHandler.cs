using Blog.Models;
using Blog.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Domain.Blog.Queries
{
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
                var query = _dataContext.BlogPosts
                    .Include(x => x.Author)
                    .Include(x => x.Category)
                    .OrderByDescending(x => x.Created)
                    .AsQueryable();

                if ((bool)!request.AllPost)
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
                        query = query.Where(x => x.Tags.Contains(request.Tag));
                    }
                    else
                    {
                        query = query.Take(3);
                    }
                }

                var posts = await query
                    .Select(x => new BlogQueryResponse()
                    {
                        PostId = x.PostId,
                        AuthorsName = x.Author.FirstName + " " + x.Author.LastName,
                        CoverImagePath = x.CoverImagePath,
                        Title = x.Title,
                        Summary = x.Summary,
                        Body = x.Body,
                        Tags = x.Tags,
                        Category = x.Category,
                        DateCreated = x.Created
                    })
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                return posts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred: {ex.Message}");
                throw;
            }
        }
    }
}
