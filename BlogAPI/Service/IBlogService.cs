using BlogAPI.Model;
using BlogAPI.ModelDTO;

namespace BlogAPI.Service;

public interface IBlogService
{
    public Task<List<BlogPost>> GetAllPost();
    public Task<BlogPost> GetPostById(int id);
    public Task<List<Author>> GetPostByAuthor(int id);
    public Task<BlogPost> AddPost(NewPostDTO newPost);
    public Task<BlogPost> UpdatePost (NewPostDTO updatePost);
    public Task<Author> AddAuthor(NewAuthorDTO newAuthor);
    public Task<Category> AddCategory(NewCategoryDTO newCategory);
    public Task DeletePost(int id);
}