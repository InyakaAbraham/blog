using Blog.Models;

namespace Blog.Features;

public interface IBlogService
{
    public Task<PagedList<BlogPost>> GetAllPosts(PageParameters pageParameters);
    public Task<BlogPost?> GetPostById(long id);
    public Task<PagedList<BlogPost>> GetPostByTitle(string title, PageParameters pageParameters);
    public Task<PagedList<BlogPost>> GetPostByAuthor(long id, PageParameters pageParameters);
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