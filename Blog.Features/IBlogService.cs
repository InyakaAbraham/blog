using Blog.Models;

namespace Blog.Features;

public interface IBlogService
{
    public Task<List<BlogPost>> GetAllPosts();
    public Task<BlogPost?> GetPostById(int id);
    public Task<List<BlogPost>> GetPostsByAuthor(int id);
    public Task<BlogPost> AddPost(BlogPost newPost);
    public Task<BlogPost?> UpdatePost(BlogPost updatePost);
    public Task<Author> AddAuthor(Author newAuthor);
    public Task<Category> AddCategory(Category newCategory);
    public Task DeletePost(int id);
}