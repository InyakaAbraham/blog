using Blog.Model.DTO;
using Blog.Models;

namespace Blog.Service;

public interface IBlogService
{
    public Task<List<BlogPost>> GetAllPost();
    public Task<BlogPost> GetPostById(int id);
    public Task<List<Author>> GetPostByAuthor(int id);
    public Task<BlogPost> AddPost(NewPostDto newPost);
    public Task<BlogPost> UpdatePost (NewPostDto updatePost);
    public Task<Author> AddAuthor(NewAuthorDto newAuthor);
    public Task<Category> AddCategory(NewCategoryDto newCategory);
    public Task DeletePost(int id);
    public Task<User> RegisterUser(UserDto userDto);
    public Task<string> LoginUser(UserDto userDto);
    public void CreatPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
    public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
    public string CreateToken(User user);
}