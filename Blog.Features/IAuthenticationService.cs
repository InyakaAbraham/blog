using Blog.Models;

namespace Blog.Features;

public interface IAuthenticationService
{
    public Task<string> CreateJwtToken(Author author);
}
