using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blog.Models;
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

    public async Task<List<BlogPost?>> GetAllPosts()
    {
        return await _dataContext.BlogPosts.Include(x=>x!.Author)
            .Include(x=>x!.Category)
            .ToListAsync();
    }

    public async Task<BlogPost?> GetPostById(int id)
    {
        var post = await _dataContext.BlogPosts
            .Where(x => x != null && x.PostId == id).Include(x=>x!.Author)
            .Include(x=>x!.Category)
            .FirstOrDefaultAsync();

        return post ?? null;
    }

    public async Task<List<BlogPost?>> GetPostsByAuthor(int id)
    {
        return await _dataContext.BlogPosts
            .Where(x => x != null && x.AuthorId == id)
            .Include(x=>x!.Category)
            .Include(x=>x!.Author)
            .ToListAsync();
        ;
    }


    public async Task<BlogPost?> AddPost(BlogPost newPost)
    {
        var author = await _dataContext.Authors
            .Where(x => x.AuthorId == newPost.AuthorId)
            .FirstOrDefaultAsync();
        var category = await _dataContext.Categories
            .Where(x => x.CategoryName.ToUpper()==newPost.CategoryName.ToUpper())
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

    public async Task<BlogPost?> UpdatePost(BlogPost updatePost)
    {
        var author = await _dataContext.Authors
            .Where(x => x.AuthorId == updatePost.AuthorId)
            .FirstOrDefaultAsync();
        var category = await _dataContext.Categories
            .Where(x => x.CategoryName.ToUpper()==updatePost.CategoryName.ToUpper())
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

    public async Task AddAuthor(Author newAuthor)
    {
        var author = new Author
        {
            AuthorId = newAuthor.AuthorId,
            Name = newAuthor.Name,
            Description = newAuthor.Description,
        };
        _dataContext.Authors.Add(author);
        await _dataContext.SaveChangesAsync();
    }

    public async Task AddCategory(Category newCategory)
    {
        _dataContext.Categories.Add(new Category
        {
            CategoryName = newCategory.CategoryName
        });
        await _dataContext.SaveChangesAsync();
    }

    public async Task DeletePost(int id)
    {
        _dataContext.BlogPosts.Remove(await _dataContext.BlogPosts.FindAsync(id));
        await _dataContext.SaveChangesAsync();
    }

    public async Task<User?> GetUserByEmailAddress(string emailAddress)
    {
        return await _dataContext.Users.SingleOrDefaultAsync(u => u!.EmailAddress == emailAddress);
    }

    public async Task<User?> GetUserByUsername(string userName)
    {
        return await _dataContext.Users.SingleOrDefaultAsync(u => u!.Username == userName);
    }

    public async Task<Author?> GetAuthorById(int authorId)
    {
        return await _dataContext.Authors.Where(a => a.AuthorId == authorId)
            .Include(a=>a.BlogPosts)
            .SingleOrDefaultAsync();
    }

    public async Task<Category?> GetCategoryByName(string categoryName)
    {
        return await _dataContext.Categories
            .Where(x => x.CategoryName.ToUpper()==categoryName.ToUpper())
            .Include(x=>x.BlogPosts).SingleOrDefaultAsync();
    }

    public async Task<User> CreateUser(User user)
    {
        _dataContext.Users.Add(user);
        await _dataContext.SaveChangesAsync();
        return user;
    }

    public async Task<string?> CreatePasswordHash(string password)
    {
        return await Task.FromResult(BCrypt.Net.BCrypt.HashPassword(password));
    }


    public bool VerifyPassword(string password, User user)
    {
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public Task<string> CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_appSettings.JwtSecret));

        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken
        (
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: cred
        );
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return Task.FromResult(jwt);
    }
}