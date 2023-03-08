using Blog.Models;
using Microsoft.AspNetCore.Http;

namespace Blog.Features;

public interface IBlogService
{
    public Task<PagedList<BlogPostResponse>> GetAllPosts(PageParameters pageParameters);
    public Task<PagedList<BlogPostResponse>> GetRecentPost(PageParameters pageParameters);
    public Task<BlogPostResponse?> GetPostById(long id);
    public Task<BlogPost?> GetPost(long id);
    public Task<PagedList<BlogPostResponse>> GetPostByTag(string title, PageParameters pageParameters);
    public Task<PagedList<BlogPostResponse>> GetPostByAuthor(PageParameters pageParameters, long id);
    public Task<bool> AddPost(BlogPost newPost);
    public Task<BlogPost?> UpdatePost(BlogPost updatePost);
    public Task DeletePost(long id);
    public Task<Category?> GetCategoryByName(string? categoryName);
    public Task<string> UploadFile(IFormFile file);
    public Task<Category?> AddCategory(Category category);
}
