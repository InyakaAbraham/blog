using Blog.Models;

namespace Blog.Features;

public interface IUserService
{
    public Task<string?> CreatePasswordHash(string password);
    public bool VerifyPasswordHash(string password, string passwordHash);
    public Task<string?> CreateToken(User user);
}