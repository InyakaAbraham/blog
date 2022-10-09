using Blog.Models;

namespace Blog.Features;

public interface IBlogService
{
    public Task<List<BlogPost?>> GetAllPosts();
    public Task<BlogPost?> GetPostById(int id);
    public Task<List<BlogPost?>> GetPostsByAuthor(int id);
    public Task<BlogPost?> AddPost(BlogPost newPost);
    public Task DeletePost(int id);
    public Task<BlogPost?> UpdatePost(BlogPost updatePost);
    public Task AddAuthor(Author newAuthor);
    public Task AddCategory(Category newCategory);
    public Task<User?> GetUserByEmailAddress(string userName);
    public Task<User?> GetUserByUsername(string userName);
    public Task<Author?> GetAuthorById(int authorId);
    public Task<Category?> GetCategoryByName(string categoryName);
    public Task<User> CreateUser(User user);
    public Task<string?> CreatePasswordHash(string password);
    public bool VerifyPassword(string password, User user);
    public Task<string> CreateToken(User user);
}