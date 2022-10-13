using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blog.Models;
using Blog.Models.Enums;
using Blog.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Blog.Features;

public class BlogService : IBlogService
{
    private readonly AppSettings _appSettings;
    private readonly DataContext _dataContext;

    public BlogService(DataContext dataContext, AppSettings appSettings)
    {
        _dataContext = dataContext;
        _appSettings = appSettings;
    }

    public async Task<List<BlogPost>> GetAllPosts()
    {
        return await _dataContext.BlogPosts.Include(x => x!.Author)
            .Include(x => x!.Category)
            .ToListAsync();
    }

    public async Task<BlogPost?> GetPostById(long id)
    {
        var post = await _dataContext.BlogPosts
            .Where(x => x.PostId == id).Include(x => x!.Author)
            .Include(x => x!.Category)
            .FirstOrDefaultAsync();

        return post ?? null;
    }

    public async Task<List<BlogPost>> GetPostByTitle(string title)
    {
        return await _dataContext.BlogPosts.Where(b => b.Title.Contains(title))
            .Include(b => b.Author)
            .Include(b => b.Category)
            .ToListAsync();
    }

    public async Task<List<BlogPost>> GetPostByAuthor(long id)
    {
        return await _dataContext.BlogPosts.Where(b => b.AuthorId == id)
            .Include(b => b.Author)
            .Include(b => b.Category)
            .ToListAsync();
    }

    public async Task<BlogPost?> AddPost(BlogPost newPost)
    {
        var author = await GetAuthorById(newPost.AuthorId);
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


    public async Task<Author?> GetAuthorByEmailAddress(string emailAddress)
    {
        return await _dataContext.Authors.Include(x => x.Roles)
            .SingleOrDefaultAsync(y => y!.EmailAddress == emailAddress);
    }

    public async Task<Author?> GetAuthorByUsername(string userName)
    {
        return await _dataContext.Authors.SingleOrDefaultAsync(u => u!.Username == userName);
    }

    public async Task<Author?> GetAuthorById(long authorId)
    {
        return await _dataContext.Authors.Where(a => a.AuthorId == authorId)
            .Include(a => a.BlogPosts)
            .SingleOrDefaultAsync();
    }

    public async Task<Category?> GetCategoryByName(string categoryName)
    {
        return await _dataContext.Categories
            .Where(x => x!.CategoryName.ToUpper() == categoryName.ToUpper())
            .Include(x => x!.BlogPosts).FirstOrDefaultAsync();
    }

    public async Task<Author> CreateUser(Author author)
    {
        author.Roles = new List<Role?>
            { await _dataContext.Roles.SingleOrDefaultAsync(x => x.Id == UserRole.Author) };
        _dataContext.Authors.Add(author);
        await _dataContext.SaveChangesAsync();
        return author;
    }

    public async Task<string?> CreatePasswordHash(string password)
    {
        return await Task.FromResult(BCrypt.Net.BCrypt.HashPassword(password));
    }

    public bool VerifyPassword(string password, Author author)
    {
        return BCrypt.Net.BCrypt.Verify(password, author.PasswordHash);
    }

    public Task<string> CreateToken(Author author)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JwtSecret));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var claims = new List<Claim>
            { new("sub", author.AuthorId.ToString()), new("role", UserRole.Default.ToString()) };

        claims.AddRange(author.Roles.Select(role => new Claim("role", role.Id.ToString())));

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(
            new JwtSecurityToken
            (
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: cred
            )));
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