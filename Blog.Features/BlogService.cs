using Blog.Models;
using Blog.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Blog.Features;

public class BlogService : IBlogService
{
    private readonly DataContext _dataContext;

    public BlogService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<List<BlogPost>> GetAllPost()
    {
        return await _dataContext.BlogPosts.Include(x => x.Author)
            .Include(x => x.Category)
            .ToListAsync();
    }

    public async Task<BlogPost?> GetPostById(int id)
    {
        var post = await _dataContext.BlogPosts.Where(x => x.PostId == id)
            .Include(x => x.Author)
            .Include(x => x.Category)
            .FirstOrDefaultAsync();
        if (post == null) return null;
        return post;
    }

    public async Task<List<BlogPost>> GetPostByAuthor(int id)
    {
        var posts = await _dataContext.BlogPosts.Where(x => x.AuthorId == id)
            .Include(x => x.Author)
            .Include(x => x.Category)
            .ToListAsync();
        return posts;
    }


    public async Task<BlogPost> AddPost(BlogPost newPost)
    {
        var author = await _dataContext.Authors.Where(x => x.AuthorId == newPost.AuthorId)
            .Include(x => x.BlogPosts)
            .FirstOrDefaultAsync();

        var category = await _dataContext.Categories
            .Where(x => x.CategoryName.ToUpper() == newPost.CategoryName.ToUpper())
            .FirstOrDefaultAsync();

        var post = new BlogPost
        {
            Title = newPost.Title,
            Summary = newPost.Summary,
            Body = newPost.Body,
            Author = author,
            Tags = newPost.Tags,
            AuthorId = newPost.AuthorId,
            Category = category,
            CategoryName = newPost.CategoryName,
            Updated = newPost.Updated,
            Created = DateTime.UtcNow
        };
        _dataContext.BlogPosts.Add(post);
        await _dataContext.SaveChangesAsync();
        return post;
    }

    public async Task<BlogPost> UpdatePost(BlogPost updatePost)
    {
        var author = await _dataContext.Authors.Where(x => x.AuthorId == updatePost.AuthorId)
            .Include(x => x.BlogPosts)
            .FirstOrDefaultAsync();
        var category = await _dataContext.Categories
            .Where(x => x.CategoryName.ToUpper() == updatePost.CategoryName.ToUpper())
            .FirstOrDefaultAsync();
        if (author == null) return null;
        var post = await _dataContext.BlogPosts.FindAsync(updatePost.PostId);
        post.PostId = updatePost.PostId;
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

    public async Task<Author> AddAuthor(Author newAuthor)
    {
        var post = await _dataContext.BlogPosts.Where(x => x.AuthorId == newAuthor.AuthorId)
            .Include(x => x.Author)
            .ToListAsync();
        var author = new Author
        {
            AuthorId = newAuthor.AuthorId,
            Name = newAuthor.Name,
            Description = newAuthor.Description,
            BlogPosts = post
        };
        _dataContext.Authors.Add(author);
        await _dataContext.SaveChangesAsync();
        return author;
    }

    public async Task<Category> AddCategory(Category newCategory)
    {
        var category = new Category
        {
            CategoryName = newCategory.CategoryName
        };
        _dataContext.Categories.Add(category);
        await _dataContext.SaveChangesAsync();
        return category;
    }

    public async Task DeletePost(int id)
    {
        var post = await _dataContext.BlogPosts.FindAsync(id);
        _dataContext.BlogPosts.Remove(post);
        await _dataContext.SaveChangesAsync();
    }
}