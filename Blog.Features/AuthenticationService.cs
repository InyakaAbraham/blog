using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blog.Models;
using Microsoft.IdentityModel.Tokens;

namespace Blog.Features;

public class AuthenticationService:IAuthenticationService
{
    private readonly AppSettings _appSettings;

    public AuthenticationService(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }
    public Task<string> CreateJwtToken(Author author)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JwtSecret));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var claims = new List<Claim>
                { new("sub", author.AuthorId.ToString()), new("role", UserRole.Default.ToString()) };

            claims.AddRange(author.Roles.Select(role => new Claim("role", role!.Id.ToString())));

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken
                (
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: cred
                )));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return null;
        }    }
}
