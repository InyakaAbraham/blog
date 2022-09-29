using Blog.Api.Blog.Model.Dto;
using Blog.Models;

namespace Blog.Service;

public interface IUserService
{
    public Task<User> RegisterUser(UserDto userDto);
    public Task<string> LoginUser(UserDto userDto);
    public void CreatPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
    public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
    public string CreateToken(User user);
}
