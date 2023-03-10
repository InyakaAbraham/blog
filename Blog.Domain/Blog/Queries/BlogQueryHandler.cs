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
            if (request.AllPost == true && string.IsNullOrEmpty(request.Tag) && request.AuthorId == null && request.PostId == null)
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
                        }).OrderByDescending(x => x.DateCreated).ToListAsync(cancellationToken);

                    return listPost;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception occurred: {ex.Message}");
                    return null;
                }
            }

            if (request.AllPost == false && string.IsNullOrEmpty(request.Tag) && request.AuthorId == null && request.PostId == null)
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
                        }).OrderByDescending(x => x.DateCreated).Take(3).ToListAsync(cancellationToken);

                    return listPost;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception occurred: {ex.Message}");
                    return null;
                }
            }

            if (request.AllPost == false && string.IsNullOrEmpty(request.Tag) && request.AuthorId != null && request.PostId == null)
            {
                try
                {

                    var post = await (from bp in _dataContext.BlogPosts
                        join ar in _dataContext.Authors.Where(x => x.AuthorId == request.AuthorId) on
                            bp.AuthorId equals ar.AuthorId
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
                        }).OrderByDescending(x => x.DateCreated).ToListAsync(cancellationToken: cancellationToken);

                    return post;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception occurred: {ex.Message}");
                    return null;
                }
            }

            if (request.AllPost == false && string.IsNullOrEmpty(request.Tag) && request.AuthorId == null && request.PostId != null)
            {
                try
                {
                    var blogPost = await _dataContext.BlogPosts
                        .Where(x => x.PostId == request.PostId)
                        .Include(x => x.Author)
                        .Include(x => x.Category)
                        .OrderBy(x => x.PostId)
                        .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (blogPost == null)
                    {
                        return null;
                    }

                    var response = new BlogQueryResponse()
                    {
                        Body = blogPost.Body,
                        Summary = blogPost.Summary,
                        Category = blogPost.Category,
                        Tags = blogPost.Tags,
                        Title = blogPost.Title,
                        CoverImagePath = blogPost.CoverImagePath,
                        AuthorsName = blogPost.Author.FirstName +" "+ blogPost.Author.LastName,
                        DateCreated = blogPost.Created,
                        PostId = blogPost.PostId
                    };

                    return new List<BlogQueryResponse>()
                    {
                        response,
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception occurred: {ex.Message}");
                    return null;
                }
            }

            if (request.AllPost == false && !string.IsNullOrEmpty(request.Tag) && request.AuthorId == null && request.PostId == null)
            {
                try
                {
                    var post = await (from bp in _dataContext.BlogPosts.Where(x => x.Tags.Contains(request.Tag))
                        join ar in _dataContext.Authors on bp.AuthorId equals ar.AuthorId
                        select new BlogQueryResponse()
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
                        }).OrderByDescending(x => x.DateCreated).ToListAsync(cancellationToken: cancellationToken);

                    return post;
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"An exception occurred: {ex.Message}");
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
