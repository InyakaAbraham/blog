using Blog.Models;

namespace Blog.Features;

public interface IBlogService
{
    public Task<List<BlogPost?>> GetAllPosts();
    public Task<BlogPost?> GetPostById(long id);
    public Task<List<BlogPost?>> GetPostByTitle(string title);
    public Task<List<BlogPost?>> GetPostByAuthor(long id);
    public Task<BlogPost?> AddPost(BlogPost newPost);
    public Task<BlogPost?> UpdatePost(BlogPost updatePost);
    public Task DeletePost(long id);
    public Task<Author> CreateAuthor(Author author);
    public Task<Author?> GetAuthorByEmailAddress(string userName);
    public Task<Author?> GetAuthorByUsername(string userName);
    public Task<Author?> GetAuthorById(long authorId);
    public Task<Category?> GetCategoryByName(string categoryName);
    public Task<string?> CreatePasswordHash(string password);
    public bool VerifyPassword(string password, Author author);
    public Task<string> CreateToken(Author author);
}