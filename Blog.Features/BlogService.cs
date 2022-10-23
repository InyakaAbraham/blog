using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blog.Models;
using Blog.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using Role = Blog.Models.Role;

namespace Blog.Features;

public class BlogService : IBlogService
{
    
    private static readonly Random GenerateRandomToken = new();
    private readonly AppSettings _appSettings;
    private readonly DataContext _dataContext;
    private readonly IEmailService _emailService;
    private readonly IDatabase _database;

    public BlogService
    (
        DataContext dataContext, 
        IConnectionMultiplexer redis,
        AppSettings appSettings, 
        IEmailService emailService
        )
    {
        _dataContext = dataContext;
        _appSettings = appSettings;
        _emailService = emailService;
        _database = redis.GetDatabase();

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


    public async Task<Author> UpdateAuthor(Author author)
    {
        _dataContext.Authors.Update(author);
        await _dataContext.SaveChangesAsync();
        return author;
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
        
        var token = CreateRandomToken();
        
        await _database.StringSetAsync($"email_verification_otp:{author.EmailAddress}",
            token, TimeSpan.FromMinutes(20));
        
        _emailService.Send("to_address@example.com", "Verification Token", token);
        
        _dataContext.Authors.Add(author);
        await _dataContext.SaveChangesAsync();
        return author;
    }

    public async Task<string?> CreatePasswordHash(string password)
    {
        return await Task.FromResult(BCrypt.Net.BCrypt.HashPassword(password));
    }

    public Task<bool> VerifyPassword(string password, Author author)
    {
        return Task.FromResult(BCrypt.Net.BCrypt.Verify(password, author.PasswordHash));
    }

    public Task<string> CreateJwtToken(Author author)
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

    public string CreateRandomToken()
    {
        int CreateRandomNumber(int min, int max)
        {
            lock (GenerateRandomToken)
            {
                return GenerateRandomToken.Next(min, max);
            }
        }

        return CreateRandomNumber(0, 1000000).ToString("D6");
    }

    public async Task<bool> VerifyAuthor(string emailAddress, string token)
    {
        var tokenCheck = await _database.StringGetAsync($"email_verification_otp:{emailAddress}");

        if (token == tokenCheck)
        {
            var author = await GetAuthorByEmailAddress(emailAddress);

            if (author != null)
            {
                author.VerifiedAt = DateTime.UtcNow;
            }

            _dataContext.Authors.Update(author!);
            await _dataContext.SaveChangesAsync();
            return true;
        }

        return false;
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