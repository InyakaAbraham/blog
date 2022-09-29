using Blog.Api.Blog.Model.Dto;
using Blog.Models;

namespace Blog.Service;

public interface IBlogService
{
    public Task<List<BlogPost>> GetAllPost();
    public Task<BlogPost> GetPostById(int id);
    public Task<List<BlogPost>> GetPostByAuthor(int id);
    public Task<BlogPost> AddPost(NewPostDto newPost);
    public Task<BlogPost> UpdatePost(NewPostDto updatePost);
    public Task<Author> AddAuthor(NewAuthorDto newAuthor);
    public Task<Category> AddCategory(NewCategoryDto newCategory);
    public Task DeletePost(int id);
}