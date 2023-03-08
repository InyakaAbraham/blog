using System.Diagnostics.CodeAnalysis;
using Blog.Models;
using Blog.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Blog.Features;

public class BlogService : IBlogService
{
    private readonly DataContext _dataContext;

    public BlogService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<PagedList<BlogPostResponse>> GetAllPosts(PageParameters pageParameters)
    {
        try
        {
            var listPost = await (from bp in _dataContext.Set<BlogPost>()
                join ar in _dataContext.Set<Author>() on bp.AuthorId equals ar.AuthorId
                select new BlogPostResponse
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
            return await PagedList<BlogPostResponse>.ToPagedList(listPost, pageParameters.PageNumber, pageParameters.PageSize);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return null;

        }
    }
    public async Task<PagedList<BlogPostResponse>> GetRecentPost(PageParameters pageParameters)
    {
        try
        {
            var lisPost = await (from bp in _dataContext.Set<BlogPost>()
                join ar in _dataContext.Set<Author>() on bp.AuthorId equals ar.AuthorId
                select new BlogPostResponse
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
                }).OrderByDescending(x => x.DateCreated).Take(3).ToListAsync();

            return await PagedList<BlogPostResponse>.ToPagedList(lisPost, pageParameters.PageNumber, pageParameters.PageSize);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
             return null;
        }

    }
    public async Task<BlogPostResponse?> GetPostById(long id)
    {
        try
        {
            var blogPost = await _dataContext.BlogPosts
                .Where(x => x.PostId == id)
                .Include(x => x.Author)
                .Include(x => x.Category)
                .OrderBy(x => x.PostId)
                .FirstOrDefaultAsync();

            if (blogPost == null)
            {
                return null;
            }

            var response = new BlogPostResponse
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

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return null;
        }

    }

    public async Task<PagedList<BlogPostResponse>> GetPostByTag(string tag, PageParameters pageParameters)
    {
        try
        {
            var post = await (from bp in _dataContext.BlogPosts.Where(x => x.Tags.Contains(tag))
                join ar in _dataContext.Authors on bp.AuthorId equals ar.AuthorId
                select new BlogPostResponse
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

            return await PagedList<BlogPostResponse>.ToPagedList(post, pageParameters.PageNumber, pageParameters.PageSize);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return null;
        }

    }

    public async Task<PagedList<BlogPostResponse>> GetPostByAuthor(PageParameters pageParameters, long id)
    {
        try
        {
            if (await _dataContext.Authors
                    .Where(x => x.AuthorId == id)
                    .SingleOrDefaultAsync() == null) return null;

            var post = await (from bp in _dataContext.BlogPosts
                join ar in _dataContext.Authors.Where(x => x.AuthorId == id) on
                    bp.AuthorId equals ar.AuthorId
                select new BlogPostResponse
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

            return await PagedList<BlogPostResponse>.ToPagedList(post, pageParameters.PageNumber, pageParameters.PageSize);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> AddPost(BlogPost newPost)
    {
        try
        {
            _dataContext.BlogPosts.Add(newPost);
            await _dataContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return false;
        }

    }

    public async Task<BlogPost?> UpdatePost(BlogPost updatePost)
    {
        try
        {
            var post = await _dataContext.BlogPosts.SingleOrDefaultAsync(x => x.PostId == updatePost.PostId);
            _dataContext.BlogPosts.Update(post);
            await _dataContext.SaveChangesAsync();
            return updatePost;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return null;
        }

    }

    public async Task DeletePost(long id)
    {
        try
        {
            var post = await _dataContext.BlogPosts.SingleOrDefaultAsync(x => x.PostId == id);
            _dataContext.BlogPosts.Remove(post);
            await _dataContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
        }

    }

    public async Task<BlogPost?> GetPost(long id)
    {
        try
        {
            return await _dataContext.BlogPosts.SingleOrDefaultAsync(x => x.PostId == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return null;
        }
    }

    public async Task<Category?> GetCategoryByName(string? categoryName)
    {
        try
        {
            return await _dataContext.Categories
                .Where(x => x.CategoryName!.ToUpper() == categoryName!.ToUpper())
                .Include(x => x.BlogPosts).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return null;
        }
    }

    public async Task<Category?> AddCategory(Category category)
    {
        try
        {
            var validateCategory = await GetCategoryByName(category.CategoryName);

            if (validateCategory != null)
                return validateCategory;

            var newCategory = new Category
            {
                CategoryName = category.CategoryName
            };

            _dataContext.Categories.Add(newCategory);
            await _dataContext.SaveChangesAsync();

            return newCategory;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return null;
        }
    }

    public Task<string> UploadFile(IFormFile file)
    {
        try
        {
            var uniqueFileName = Guid.NewGuid() + "_" + file.FileName;
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "WebRoot/images/", uniqueFileName);

            file.CopyTo(new FileStream(imagePath, FileMode.Create));
            return Task.FromResult(uniqueFileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return Task.FromResult<string?>(null);
        }

    }
}
