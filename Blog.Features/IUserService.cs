using Blog.Models;

namespace Blog.Features;

public interface IUserService
{
    public void CreatPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
    public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
    public string CreateToken(User user);
}