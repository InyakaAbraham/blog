using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Blog.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Blog.Features;

public class UserService : IUserService
{
    private readonly IConfiguration _configuration;

    public UserService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void CreatPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }


    public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512(passwordSalt))
        {
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }
    }

    public string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken
        (
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: cred
        );
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }
}