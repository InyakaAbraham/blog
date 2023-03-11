using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blog.Features;
using Blog.Models;
using Blog.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Microsoft.IdentityModel.Tokens;
using Role = Blog.Models.Role;

namespace Blog.Domain.User.Commands;

public class UserCommandHandler: IRequestHandler<UserCommand, UserCommandResponse>
{
    private readonly DataContext _dataContext;
    private readonly AppSettings _appSettings;
    private readonly IEmailService _emailService;
    private readonly IDatabase _database;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly Random GenerateRandomToken = new();


    public UserCommandHandler(DataContext dataContext, AppSettings appSettings, IEmailService emailService,         IConnectionMultiplexer redis, IHttpContextAccessor httpContextAccessor)
    {
        _dataContext = dataContext;
        _appSettings = appSettings;
        _emailService = emailService;
        _httpContextAccessor = httpContextAccessor;
        _database = redis.GetDatabase();
    }

    public async Task<UserCommandResponse> Handle(UserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.State == 1)
            {
                var token = CreateRandomToken();

                await _database.StringSetAsync($"email_verification_otp:{request.EmailAddress}",
                    token, TimeSpan.FromDays(365));

                _emailService.Send("to_address@example.com", "Verification Token", $"Your OTP is {token} valid for 20Minutes.");
                var authorByEmail = await GetAuthorByEmailAddress(request.EmailAddress);
                var authorByUsername = await GetAuthorByUsername(request.Username);

                if (authorByEmail == null && authorByUsername == null)
                {
                    var author = new Author
                    {
                        EmailAddress = request.EmailAddress,
                        Roles = new List<Role?>
                        {
                            await _dataContext.Roles.SingleOrDefaultAsync(x => x.Id == UserRole.Author, cancellationToken: cancellationToken)
                        },
                        Description = request.Description,
                        Username = request.Username,
                        CreatedAt = DateTime.UtcNow,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        PasswordHash = await CreatePasswordHash(request.Password),
                        LastLogin = DateTime.Now
                    };

                    _dataContext.Authors.Add(author);
                    await _dataContext.SaveChangesAsync(cancellationToken);
                    return new UserCommandResponse(author);
                }

                throw new ArgumentNullException("usernameOrEmail", "User with username/email already exists");
            }

            if (request.State==2)
            {
                var author = await GetAuthorById(GetContextUserId());
                var authorBuUsername = await GetAuthorByUsername(request.Username);

                if (author == null) throw new ArgumentNullException("author", "Author does not exist");
                if (authorBuUsername != null && request.Username != author.Username) throw new ArgumentNullException("Username", "Username already exist");

                author.Username = request.Username;
                author.FirstName = request.FirstName;
                author.LastName = request.LastName;
                author.Description = request.Description;

                _dataContext.Authors.Update(author);
                await _dataContext.SaveChangesAsync(cancellationToken);
                return new UserCommandResponse(author);
            }

            if (request.State==3)
            {
                try
                {
                    var author = await GetAuthorByEmailAddress(request.EmailAddress);
                    if (author == null) throw new ArgumentNullException("author", "Author does not exist");

                    var token = CreateRandomToken();

                    await _database.StringSetAsync($"email_reset_otp:{request.EmailAddress}",
                        token, TimeSpan.FromMinutes(20));
                    _emailService.Send("to_address@example.com", "Reset Token", $" Your OTP is {token} valid for 20Minutes.");

                    return new UserCommandResponse(author);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception occurred: {ex.Message}");
                    return null;
                }
            }

            if (request.State==4)
            {
                try
                {
                    var user = await GetAuthorByEmailAddress(request.EmailAddress);
                    if (user == null) throw new ArgumentNullException("author", "Author does not exist");

                    var validateToken = await _database.StringGetAsync($"email_reset_otp:{request.EmailAddress}");
                    if (validateToken != request.Token) throw new ArgumentNullException("token", "invalid token");

                    var passwordHash = await CreatePasswordHash(request.Password);
                    user.PasswordHash = passwordHash;

                    _dataContext.Authors.Update(user);
                     await _dataContext.SaveChangesAsync(cancellationToken);

                    return new UserCommandResponse(user);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception occurred: {ex.Message}");
                    return null;
                }
            }

            if (request.State==5)
            {
                try
                {
                    var author = await GetAuthorByEmailAddress(request.EmailAddress);
                    var currentAuthor = await GetAuthorById(GetContextUserId());
                    var validatePassword = await VerifyPassword(request.Password, author!);

                    if (!validatePassword) throw new ArgumentNullException("password", "Invalid username/password");

                    if (author==currentAuthor)
                    {
                        var passwordHash = await CreatePasswordHash(request.NewPassword);

                        author!.PasswordHash = passwordHash;

                        _dataContext.Authors.Update(author);
                        await _dataContext.SaveChangesAsync(cancellationToken);

                        return new UserCommandResponse(author);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception occurred: {ex.Message}");
                    return null;
                }
            }

            if (request.State==6)
            {
                try
                {
                    var author = await GetAuthorByEmailAddress(request.EmailAddress);
                    if (author == null) throw new ArgumentNullException("author", "Author does not exist");

                    var validateAuthor = await VerifyPassword(request.Password, author);
                    if (!validateAuthor) throw new ArgumentNullException("author", "Invalid username/password");

                    var token = CreateRandomToken();

                    await _database.StringSetAsync($"email_change_otp:{author.EmailAddress}",
                        token, TimeSpan.FromMinutes(20));

                    _emailService.Send("to_address@example.com", "Verification Token", $"Your OTP is {token} valid for 20Minutes.");

                    return new UserCommandResponse(author);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception occurred: {ex.Message}");
                    return null;
                }
            }

            if (request.State==7)
            {
                try
                {
                    var newEmailAddressCheck = await GetAuthorByEmailAddress(request.EmailAddress);
                    if (newEmailAddressCheck != null) throw new ArgumentNullException("author", "Author does not exist");

                    var validateToken = await _database.StringGetAsync($"email_change_otp:{request.OldEmailAddress}");
                    if (validateToken != request.Token) throw new ArgumentNullException("author", "Invalid token");

                    var author = await GetAuthorByEmailAddress(request.EmailAddress);
                    if (author == null) throw new ArgumentNullException("author", "Author does not exist");

                    author.EmailAddress = request.EmailAddress;

                    _dataContext.Authors.Update(author);
                    await _dataContext.SaveChangesAsync(cancellationToken);

                    return new UserCommandResponse(author);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"An exception occurred: {ex.Message}");
                    return null;
                }
            }

            if (request.State==8)
            {
                try
                {
                    var validateToken = await _database.StringGetAsync($"email_verification_otp:{request.EmailAddress}");
                    if (validateToken != request.Token) throw new ArgumentNullException("author", "Invalid token");

                    var author = await GetAuthorByEmailAddress(request.EmailAddress);
                    if (author == null) throw new ArgumentNullException("author", "Author does not exist");

                    author.VerifiedAt = DateTime.UtcNow;

                    _dataContext.Authors.Update(author);
                    await _dataContext.SaveChangesAsync(cancellationToken);

                    return new UserCommandResponse(author);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception occurred: {ex.Message}");
                    return null;
                }
            }

            if (request.State==9)
            {
                var author = await GetAuthorByEmailAddress(request.EmailAddress);
                if (author == null || !await VerifyPassword(request.Password, author))
                    throw new ArgumentNullException("author", "Invalid email/password");

                if (author.VerifiedAt == null) throw new ArgumentNullException("author", "User not verified");

                author.LastLogin = DateTime.UtcNow;
                 _dataContext.Update(author);
                 await _dataContext.SaveChangesAsync(cancellationToken);

              Console.WriteLine(CreateJwtToken(author));
              return new UserCommandResponse(author);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return null;
        }

        return null;
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

    private string CreateRandomToken()
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

    private async Task<string?> CreatePasswordHash(string password)
    {
        return await Task.FromResult(BCrypt.Net.BCrypt.HashPassword(password));
    }

    private async Task<Author?> GetAuthorByEmailAddress(string emailAddress)
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

    private async Task<Author?> GetAuthorByUsername(string userName)
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

    private async Task<Author?> GetAuthorById(long id)
    {
        try
        {
            return await _dataContext.Authors.SingleOrDefaultAsync(u => u.AuthorId == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return null;
        }
    }

    private long GetContextUserId()
    {
        return long.Parse(_httpContextAccessor.HttpContext!.User.Claims.First(i => i.Type == "sub").Value);
    }

    private async Task<bool> VerifyPassword(string password, Author author)
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
}
