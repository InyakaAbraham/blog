using Blog.Models;
using Blog.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Blog.Domain.Blog.Commands;

public class BlogCommandHandler : IRequestHandler<BlogCommand, BlogCommandResponse>
{
    private readonly DataContext _dataContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BlogCommandHandler(DataContext dataContext, IHttpContextAccessor httpContextAccessor)
    {
        _dataContext = dataContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<BlogCommandResponse> Handle(BlogCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var coverImagePath = await UploadFile(request.CoverImage);

            BlogPost post;

            if (request.CreateNew)
            {
                var author = await _dataContext.Authors.SingleOrDefaultAsync(x => x.AuthorId == GetContextUserId(), cancellationToken);
                var category = await _dataContext.Categories.SingleOrDefaultAsync(x => x.CategoryName == request.CategoryName, cancellationToken);

                if (category == null)
                {
                    category = new Category()
                    {
                        CategoryName = request.CategoryName
                    };
                    _dataContext.Categories.Add(category);
                    await _dataContext.SaveChangesAsync(cancellationToken);
                }

                post = new BlogPost()
                {
                    Body = request.Body,
                    CategoryName = category.CategoryName,
                    Category = category,
                    Summary = request.Summary,
                    Tags = request.Tags,
                    Title = request.Title,
                    Author = author,
                    Created = DateTime.Now,
                    Updated = DateTime.Now,
                    AuthorId = author.AuthorId,
                    CoverImagePath = coverImagePath
                };
                _dataContext.BlogPosts.Add(post);
            }
            else
            {
                post = await _dataContext.BlogPosts.SingleOrDefaultAsync(x => x.PostId == request.PostId, cancellationToken: cancellationToken);
                post.Body = request.Body;
                post.Summary = request.Summary;
                post.Tags = request.Tags;
                post.Title = request.Title;
                post.CoverImagePath = coverImagePath;
                post.Updated = DateTime.UtcNow;

                _dataContext.BlogPosts.Update(post);
            }

            await _dataContext.SaveChangesAsync(cancellationToken);

            return new BlogCommandResponse
            {
                Body = post.Body,
                CategoryName = post.CategoryName,
                Category = post.Category,
                Author = post.Author,
                Created = post.Created,
                Summary = post.Summary,
                Tags = post.Tags,
                Title = post.Title,
                Updated = post.Updated,
                AuthorId = post.AuthorId,
                CoverImagePath = post.CoverImagePath,
                PostId = post.PostId
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return null;
        }
    }

    private async Task<string> UploadFile(IFormFile file)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "WebRoot/images/", uniqueFileName);

        using (var stream = new FileStream(imagePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return uniqueFileName;
    }

    private long GetContextUserId()
    {
        return long.Parse(_httpContextAccessor.HttpContext!.User.Claims.First(i => i.Type == "sub").Value);

    }
}
