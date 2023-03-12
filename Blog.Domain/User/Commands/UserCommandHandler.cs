using Blog.Features;
using Blog.Models;
using Blog.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Role = Blog.Models.Role;

namespace Blog.Domain.User.Commands;

public class UserCommandHandler : IRequestHandler<UserCommand, UserCommandResponse>
{
    private static readonly Random GenerateRandomToken = new();
    private readonly IDatabase _database;
    private readonly DataContext _dataContext;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;


    public UserCommandHandler(DataContext dataContext, IEmailService emailService, IConnectionMultiplexer redis, IHttpContextAccessor httpContextAccessor)
    {
        _dataContext = dataContext;
        _emailService = emailService;
        _httpContextAccessor = httpContextAccessor;
        _database = redis.GetDatabase();
    }

    public async Task<UserCommandResponse> Handle(UserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.State == 1)
                try
                {
                    string token = CreateRandomToken();

                    await _database.StringSetAsync($"email_verification_otp:{request.EmailAddress}",
                        token, TimeSpan.FromDays(365));

                    _emailService.Send("to_address@example.com", "Verification Token", $"Your OTP is {token} valid for 20Minutes.");
                    if (request.EmailAddress != null)
                    {
                        Author? authorByEmail = await GetAuthorByEmailAddress(request.EmailAddress);
                        if (request.Username != null)
                        {
                            Author? authorByUsername = await GetAuthorByUsername(request.Username);

                            if (authorByEmail == null && authorByUsername == null)
                            {
                                if (request.Password != null)
                                {
                                    var author = new Author
                                    {
                                        EmailAddress = request.EmailAddress,
                                        Roles = new List<Role?>
                                        {
                                            await _dataContext.Roles.SingleOrDefaultAsync(x => x.Id == UserRole.Author, cancellationToken),
                                        },
                                        Description = request.Description,
                                        Username = request.Username,
                                        CreatedAt = DateTime.UtcNow,
                                        FirstName = request.FirstName,
                                        LastName = request.LastName,
                                        PasswordHash = await CreatePasswordHash(request.Password),
                                        LastLogin = DateTime.Now,
                                    };

                                    _dataContext.Authors.Add(author);
                                    await _dataContext.SaveChangesAsync(cancellationToken);
                                    return new UserCommandResponse(author);
                                }
                            }
                        }
                    }
                    throw new ArgumentNullException(nameof(request), $"An exception was thrown in register");
                }
                catch (Exception ex)
                {
                    throw new ArgumentNullException(nameof(request), $"An exception was thrown in register: {ex}");
                }

            if (request.State == 2)
            {
                try
                {
                    Author? author = await GetAuthorById(GetContextUserId());
                    if (author == null) throw new ArgumentNullException(nameof(request), "Author does not exist");

                    if (request.Username != null)
                    {
                        Author? authorBuUsername = await GetAuthorByUsername(request.Username);
                        if (authorBuUsername != null && request.Username != author.Username) throw new ArgumentNullException(nameof(request), "Username already exist");
                    }

                    author.Username = request.Username;
                    author.FirstName = request.FirstName;
                    author.LastName = request.LastName;
                    author.Description = request.Description;

                    _dataContext.Authors.Update(author);
                    await _dataContext.SaveChangesAsync(cancellationToken);
                    return new UserCommandResponse(author);
                }
                catch (Exception ex)
                {
                    throw new ArgumentNullException(nameof(request), $"An exception was thrown in update user: {ex}");
                }
            }

            if (request.State == 3)
            {
                try
                {
                    if (request.EmailAddress != null)
                    {
                        Author? author = await GetAuthorByEmailAddress(request.EmailAddress);
                        if (author == null) throw new ArgumentNullException(nameof(request), "Author does not exist");

                        string token = CreateRandomToken();

                        await _database.StringSetAsync($"email_reset_otp:{request.EmailAddress}",
                            token, TimeSpan.FromMinutes(20));
                        _emailService.Send("to_address@example.com", "Reset Token", $" Your OTP is {token} valid for 20Minutes.");

                        return new UserCommandResponse(author);
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentNullException(nameof(request), $"An exception was thrown in forgot password: {ex}");
                }
            }

            if (request.State == 4)
            {
                try
                {
                    if (request.EmailAddress != null)
                    {
                        Author? user = await GetAuthorByEmailAddress(request.EmailAddress);
                        if (user == null) throw new ArgumentNullException(nameof(request), "Author does not exist");

                        RedisValue validateToken = await _database.StringGetAsync($"email_reset_otp:{request.EmailAddress}");
                        if (validateToken != request.Token) throw new ArgumentNullException(nameof(request), "invalid token");

                        if (request.Password != null)
                        {
                            string? passwordHash = await CreatePasswordHash(request.Password);
                            user.PasswordHash = passwordHash;
                        }

                        _dataContext.Authors.Update(user);
                        await _dataContext.SaveChangesAsync(cancellationToken);

                        return new UserCommandResponse(user);
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentNullException(nameof(request), $"An exception was thrown in reset password: {ex}");
                }
            }

            if (request.State == 5)
            {
                try
                {
                    if (request.EmailAddress != null)
                    {
                        Author? author = await GetAuthorByEmailAddress(request.EmailAddress);
                        Author? currentAuthor = await GetAuthorById(GetContextUserId());
                        bool validatePassword = request.Password != null && await VerifyPassword(request.Password, author!);

                        if (!validatePassword) throw new ArgumentNullException(nameof(request), "Invalid username/password");

                        if (author == currentAuthor)
                        {
                            if (request.NewPassword != null)
                            {
                                string? passwordHash = await CreatePasswordHash(request.NewPassword);

                                author!.PasswordHash = passwordHash;
                            }

                            if (author != null)
                            {
                                _dataContext.Authors.Update(author);
                                await _dataContext.SaveChangesAsync(cancellationToken);

                                return new UserCommandResponse(author);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentNullException(nameof(request), $"An exception was thrown in change password: {ex}");
                }
            }

            if (request.State == 6)
            {
                try
                {
                    if (request.EmailAddress != null)
                    {
                        Author? author = await GetAuthorByEmailAddress(request.EmailAddress);
                        if (author == null) throw new ArgumentNullException(nameof(request), "Author does not exist");

                        bool validateAuthor = request.Password != null && await VerifyPassword(request.Password, author);
                        if (!validateAuthor) throw new ArgumentNullException(nameof(request), "Invalid username/password");

                        string token = CreateRandomToken();

                        await _database.StringSetAsync($"email_change_otp:{author.EmailAddress}",
                            token, TimeSpan.FromMinutes(20));

                        _emailService.Send("to_address@example.com", "Verification Token", $"Your OTP is {token} valid for 20Minutes.");

                        return new UserCommandResponse(author);
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentNullException(nameof(request), $"An exception was thrown in change email: {ex}");
                }
            }

            if (request.State == 7)
            {
                try
                {
                    if (request.EmailAddress != null)
                    {
                        Author? newEmailAddressCheck = await GetAuthorByEmailAddress(request.EmailAddress);
                        if (newEmailAddressCheck != null) throw new ArgumentNullException(nameof(request), "Author does not exist");
                    }

                    RedisValue validateToken = await _database.StringGetAsync($"email_change_otp:{request.OldEmailAddress}");
                    if (validateToken != request.Token) throw new ArgumentNullException(nameof(request), "Invalid token");

                    if (request.EmailAddress != null)
                    {
                        Author? author = await GetAuthorByEmailAddress(request.EmailAddress);
                        if (author == null) throw new ArgumentNullException(nameof(request), "Author does not exist");

                        author.EmailAddress = request.EmailAddress;

                        _dataContext.Authors.Update(author);
                        await _dataContext.SaveChangesAsync(cancellationToken);

                        return new UserCommandResponse(author);
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentNullException(nameof(request), $"An exception was thrown in verify email: {ex}");
                }
            }

            if (request.State == 8)
            {
                try
                {
                    RedisValue validateToken = await _database.StringGetAsync($"email_verification_otp:{request.EmailAddress}");
                    if (validateToken != request.Token) throw new ArgumentNullException(nameof(request), "Invalid token");

                    if (request.EmailAddress != null)
                    {
                        Author? author = await GetAuthorByEmailAddress(request.EmailAddress);
                        if (author == null) throw new ArgumentNullException(nameof(request), "Author does not exist");

                        author.VerifiedAt = DateTime.UtcNow;

                        _dataContext.Authors.Update(author);
                        await _dataContext.SaveChangesAsync(cancellationToken);

                        return new UserCommandResponse(author);
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentNullException(nameof(request), $"An exception was thrown in verify user: {ex}");
                }
            }

            if (request.State == 9)
            {
                if (request.EmailAddress != null)
                {
                    Author? author = await GetAuthorByEmailAddress(request.EmailAddress);
                    if (request.Password != null && (author == null || !await VerifyPassword(request.Password, author)))
                        throw new ArgumentNullException(nameof(request), "Invalid email/password");

                    if (author?.VerifiedAt == null) throw new ArgumentNullException(nameof(request), "User not verified");

                    author.LastLogin = DateTime.UtcNow;
                    _dataContext.Update(author);
                    await _dataContext.SaveChangesAsync(cancellationToken);

                    return new UserCommandResponse(author);
                }
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentNullException(nameof(request), $"An exception was thrown in user command handler: {ex}");
        }
        return null!;
    }

    private string CreateRandomToken()
    {
        int CreateRandomNumber(int min, int max)
        {
            lock (UserCommandHandler.GenerateRandomToken)
            {
                return UserCommandHandler.GenerateRandomToken.Next(min, max);
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
            throw new ArgumentNullException(nameof(emailAddress), $"An exception was thrown in get author by email: {ex}");
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
            throw new ArgumentNullException(nameof(userName), $"An exception was thrown in get author by username: {ex}");
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
            throw new ArgumentNullException(nameof(id), $"An exception was thrown in get author by id: {ex}");
        }
    }

    private long GetContextUserId()
    {
        return long.Parse(_httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(i => i.Type == "sub").Value);
    }

    private async Task<bool> VerifyPassword(string password, Author author)
    {
        try
        {
            return await Task.FromResult(BCrypt.Net.BCrypt.Verify(password, author.PasswordHash));
        }
        catch (Exception ex)
        {
            throw new ArgumentNullException(nameof(password), $"An exception was thrown in verify password: {ex}");
        }
    }
}
