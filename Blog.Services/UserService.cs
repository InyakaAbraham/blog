using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Blog.Api.Blog.Model.Dto;
using Blog.Models;
using Blog.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Blog.Service;

public class UserService:IUserService
{
    private readonly IConfiguration _configuration;
    private readonly DataContext _dataContext;
    
    public UserService(DataContext dataContext,IConfiguration configuration)
    {
        _configuration = configuration;
        _dataContext = dataContext;
    }
    public void CreatPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
    public async Task<User?> RegisterUser(UserDto userDto)
    {
        CreatPasswordHash(userDto.Password,out byte[] passwordHash,out byte[] passwordSalt);
        var req = await _dataContext.Users.FirstOrDefaultAsync(x => x.Username == userDto.UserName);
        if (req==null)
        {
            var user = new User
            {
                Username = userDto.UserName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
            };
            _dataContext.Users.Add(user);
            await _dataContext.SaveChangesAsync();
            return user;
        }

        return null;
    }
  
    
    public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using (var hmac=new HMACSHA512(passwordSalt))
        {
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }
    }
    
    public string CreateToken(User user)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name,user.Username)
        };

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken
        (
            claims:claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials:cred
        );
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }
    public async Task<string?> LoginUser(UserDto userDto)
    {
        var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.Username == userDto.UserName);

        if (user==null)
        {
            return null;
        }

        if (VerifyPasswordHash(userDto.Password,user.PasswordHash,user.PasswordSalt)==false)
        {
            return null;
        }

        string token = CreateToken(user); 
        return token;
    }
}