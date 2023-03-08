using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blog.Models;
using Blog.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using Role = Blog.Models.Role;

namespace Blog.Features;

public class UserService : IUserService
{
    private static readonly Random GenerateRandomToken = new();
    private readonly AppSettings _appSettings;
    private readonly IDatabase _database;
    private readonly DataContext _dataContext;
    private readonly IEmailService _emailService;

    public UserService(
        IEmailService emailService,
        IConnectionMultiplexer redis,
        DataContext dataContext,
        AppSettings appSettings
    )
    {
        _emailService = emailService;
        _dataContext = dataContext;
        _appSettings = appSettings;
        _database = redis.GetDatabase();
    }

    public async Task<Author?> CreateUser(Author author)
    {
        try
        {
            author.Roles = new List<Role?>
                { await _dataContext.Roles.SingleOrDefaultAsync(x => x.Id == UserRole.Author) };

            var token = CreateRandomToken();

            await _database.StringSetAsync($"email_verification_otp:{author.EmailAddress}",
                token, TimeSpan.FromDays(365));

            _emailService.Send("to_address@example.com", "Verification Token", $"Your OTP is {token} valid for 20Minutes.");

            _dataContext.Authors.Add(author);
            await _dataContext.SaveChangesAsync();
            return author;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return null;
        }
    }

    public async Task<Author> UpdateAuthor(Author author)
    {
        try
        {
            _dataContext.Authors.Update(author);
            await _dataContext.SaveChangesAsync();
            return author;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return null;
        }
    }

    public async Task<Author?> GetAuthorByEmailAddress(string emailAddress)
    {
        try
        {
            return await _dataContext.Authors.Include(x => x.Roles)
                .SingleOrDefaultAsync(y => y.EmailAddress == emailAddress);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return null;
        }
    }

    public async Task<Author?> GetAuthorByUsername(string userName)
    {
        try
        {
            return await _dataContext.Authors.SingleOrDefaultAsync(u => u.Username == userName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return null;
        }
    }

    public async Task<Author?> GetAuthorById(long authorId)
    {
        try
        {
            return await _dataContext.Authors.Where(a => a.AuthorId == authorId)
                .Include(a => a.BlogPosts)
                .SingleOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return null;
        }

    }

    public async Task<string?> CreatePasswordHash(string password)
    {
        return await Task.FromResult(BCrypt.Net.BCrypt.HashPassword(password));
    }

    public async Task<bool> VerifyPassword(string password, Author author)
    {
        try
        {
            return await Task.FromResult(BCrypt.Net.BCrypt.Verify(password, author.PasswordHash));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ForgotPassword(string emailAddress)
    {
        try
        {
            var author = await GetAuthorByEmailAddress(emailAddress);
            if (author == null) return false;

            var token = CreateRandomToken();

            await _database.StringSetAsync($"email_reset_otp:{emailAddress}",
                token, TimeSpan.FromMinutes(20));
            _emailService.Send("to_address@example.com", "Reset Token", $" Your OTP is {token} valid for 20Minutes.");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ResetPassword(string emailAddress, string token, string password)
    {
        try
        {
            var user = await GetAuthorByEmailAddress(emailAddress);
            if (user == null) return false;

            var validateToken = await _database.StringGetAsync($"email_reset_otp:{emailAddress}");
            if (validateToken != token) return false;

            var passwordHash = await CreatePasswordHash(password);
            user.PasswordHash = passwordHash;
            await UpdateAuthor(user);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ChangePassword(string password, string newPassword, string emailAddress)
    {
        try
        {
            var author = await GetAuthorByEmailAddress(emailAddress);
            var validatePassword = await VerifyPassword(password, author!);

            if (!validatePassword) return false;

            var passwordHash = await CreatePasswordHash(newPassword);

            author!.PasswordHash = passwordHash;

            _dataContext.Authors.Update(author);
            await _dataContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ChangeEmailAddress(string oldEmailAddress, string password)
    {
        try
        {
            var author = await GetAuthorByEmailAddress(oldEmailAddress);
            if (author == null) return false;

            var validateAuthor = await VerifyPassword(password, author);
            if (!validateAuthor) return false;

            var token = CreateRandomToken();

            await _database.StringSetAsync($"email_change_otp:{author.EmailAddress}",
                token, TimeSpan.FromMinutes(20));

            _emailService.Send("to_address@example.com", "Verification Token", $"Your OTP is {token} valid for 20Minutes.");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> VerifyNewEmail(string oldEmailAddress, string newEmailAddress, string token)
    {
        try
        {
            var newEmailAddressCheck = await GetAuthorByEmailAddress(newEmailAddress);
            if (newEmailAddressCheck != null) return false;

            var validateToken = await _database.StringGetAsync($"email_change_otp:{oldEmailAddress}");
            if (validateToken != token) return false;

            var author = await GetAuthorByEmailAddress(oldEmailAddress);
            if (author == null) return false;

            author.EmailAddress = newEmailAddress;

            _dataContext.Authors.Update(author);
            await _dataContext.SaveChangesAsync();

            return true;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return false;
        }
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
        }
    }

    public string CreateRandomToken()
    {
        int CreateRandomNumber(int min, int max)
        {
            lock (GenerateRandomToken)
            {
                return GenerateRandomToken.Next(min, max);
            }
        }

        return CreateRandomNumber(0, 1000000).ToString("D6");
    }

    public async Task<bool> VerifyAuthor(string emailAddress, string token)
    {
        try
        {
            var validateToken = await _database.StringGetAsync($"email_verification_otp:{emailAddress}");
            if (validateToken != token) return false;

            var author = await GetAuthorByEmailAddress(emailAddress);
            if (author == null) return false;

            author.VerifiedAt = DateTime.UtcNow;

            _dataContext.Authors.Update(author);
            await _dataContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return false;
        }
    }
}
