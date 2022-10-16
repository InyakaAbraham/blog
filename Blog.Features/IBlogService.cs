using Blog.Models;
using Blog.Models.Helper;

namespace Blog.Features;

public interface IBlogService
{
    public Task<List<BlogPost>> GetAllPosts(BlogParameters blogParameters);
    public Task<BlogPost?> GetPostById(long id);
    public Task<List<BlogPost>> GetPostByTitle(string title, BlogParameters blogParameters);
    public Task<List<BlogPost>> GetPostByAuthor(long id, BlogParameters blogParameters);
    public Task<BlogPost?> AddPost(BlogPost newPost);
    public Task<BlogPost?> UpdatePost(BlogPost updatePost);
    public Task DeletePost(long id);
    public Task<Author> CreateUser(Author author);
    public Task<Author?> GetAuthorByEmailAddress(string userName);
    public Task<Author?> GetAuthorByUsername(string userName);
    public Task<Author?> GetAuthorById(long authorId);
    public Task<Category?> GetCategoryByName(string categoryName);
    public Task<string?> CreatePasswordHash(string password);
    public bool VerifyPassword(string password, Author author);
    public Task<string> CreateToken(Author author);
}