using Blog.Models;
using Blog.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Blog.Features;

public class BlogService : IBlogService
{
    private readonly DataContext _dataContext;
    private readonly IUserService _userService;

    public BlogService(DataContext dataContext, IUserService userService)
    {
        _dataContext = dataContext;
        _userService = userService;
    }

    public async Task<PagedList<BlogPost>> GetAllPosts(PageParameters pageParameters)
    {
        var post = await _dataContext.BlogPosts.Include(x => x.Author)
            .Include(x => x.Category)
            .OrderBy(x => x.PostId)
            .ToListAsync();
        return await PagedList<BlogPost>.ToPagedList(post, pageParameters.PageNumber, pageParameters.PageSize);
    }

    public async Task<BlogPost?> GetPostById(long id)
    {
        var post = await _dataContext.BlogPosts
            .Where(x => x.PostId == id).Include(x => x!.Author)
            .Include(x => x!.Category)
            .OrderBy(x => x.PostId)
            .FirstOrDefaultAsync();

        return post ?? null;
    }

    public async Task<PagedList<BlogPost>> GetPostByTitle(string title, PageParameters pageParameters)
    {
        var post = await _dataContext.BlogPosts.Where(b => b.Title.Contains(title))
            .Include(b => b.Author)
            .Include(b => b.Category)
            .OrderBy(x => x.PostId).ToListAsync();
        return await PagedList<BlogPost>.ToPagedList(post, pageParameters.PageNumber, pageParameters.PageSize);
    }

    public async Task<PagedList<BlogPost>> GetPostByAuthor(long id, PageParameters pageParameters)
    {
        var post = await _dataContext.BlogPosts
            .Where(x => x.AuthorId == id).Include(x => x.Author)
            .Include(x => x.Category)
            .OrderBy(x => x.Title)
            .ToListAsync();
        return await PagedList<BlogPost>.ToPagedList(post, pageParameters.PageNumber, pageParameters.PageSize);
    }

    public async Task<BlogPost?> AddPost(BlogPost newPost)
    {
        var author = await _userService.GetAuthorById(newPost.AuthorId);
        var post = new BlogPost
        {
            Title = newPost.Title,
            Summary = newPost.Summary,
            Body = newPost.Body,
            Author = author,
            Tags = newPost.Tags,
            AuthorId = newPost.AuthorId,
            Category = await AddCategory(new Category
            {
                CategoryName = newPost.CategoryName
            }),
            CategoryName = newPost.CategoryName,
            Updated = newPost.Updated,
            Created = DateTime.UtcNow
        };

        _dataContext.BlogPosts.Add(post);
        await _dataContext.SaveChangesAsync();

        return post;
    }

    public async Task<BlogPost?> UpdatePost(BlogPost updatePost)
    {
        var author = await _dataContext.Authors
            .Where(x => x.AuthorId == updatePost.AuthorId)
            .FirstOrDefaultAsync();
        var category = await _dataContext.Categories
            .Where(x => x.CategoryName.ToUpper() == updatePost.CategoryName.ToUpper())
            .FirstOrDefaultAsync();
        if (author == null) return null;

        var post = await _dataContext.BlogPosts.FindAsync(updatePost.PostId);

        if (post != null)
        {
            post.Title = updatePost.Title;
            post.Summary = updatePost.Summary;
            post.Body = updatePost.Body;
            post.Tags = updatePost.Tags;
            post.Category = category;
            post.Author = author;
            post.Created = post.Created;
            post.Updated = DateTime.UtcNow;

            _dataContext.BlogPosts.Update(post);
            await _dataContext.SaveChangesAsync();

            return post;
        }

        return null;
    }

    public async Task DeletePost(long id)
    {
        _dataContext.BlogPosts.Remove((await GetPostById(id))!);
        await _dataContext.SaveChangesAsync();
    }

    public async Task<Category?> GetCategoryByName(string categoryName)
    {
        return await _dataContext.Categories
            .Where(x => x!.CategoryName.ToUpper() == categoryName.ToUpper())
            .Include(x => x!.BlogPosts).FirstOrDefaultAsync();
    }

    public async Task<Category?> AddCategory(Category newCategory)
    {
        var category = await GetCategoryByName(newCategory.CategoryName);
        if (category != null)
            return category;

        var cat = new Category
        {
            CategoryName = newCategory.CategoryName
        };
        _dataContext.Categories.Add(cat);
        await _dataContext.SaveChangesAsync();
        return category;
    }
}