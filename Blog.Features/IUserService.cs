using Blog.Models;

namespace Blog.Features;

public interface IUserService
{
    public Task<Author> CreateUser(Author author);
    public Task<Author> UpdateAuthor(Author author);
    public Task<Author?> GetAuthorByEmailAddress(string userName);
    public Task<Author?> GetAuthorByUsername(string userName);
    public Task<Author?> GetAuthorById(long authorId);
    public Task<string?> CreatePasswordHash(string password);
    public Task<bool> VerifyPassword(string password, Author author);
    public Task<bool> ForgotPassword(string emailAddress);
    public Task<bool> ResetPassword(string emailAddress, string token, string password);
    public Task<bool> ChangePassword(string password, string newPassword, string emailAddress);
    public Task<bool> ChangeEmailAddress(string oldEmailAddress, string password);
    public Task<string> CreateJwtToken(Author author);
    public string CreateRandomToken();
    public Task<bool> VerifyAuthor(string emailAddress, string token);
    public Task<bool> VerifyNewEmail(string oldEmailAddress, string newEmailAddress, string token);
}