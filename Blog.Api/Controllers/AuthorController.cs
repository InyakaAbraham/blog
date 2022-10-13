using Blog.Api.Dtos;
using Blog.Features;
using Blog.Models;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthorController : AbstractController
{
    private readonly IBlogService _blogService;

    public AuthorController(IBlogService blogService)
    {
        _blogService = blogService;
    }

    [HttpPost]
    public async Task<ActionResult> Register(RegisterAuthorRequest registerAuthorRequest)
    {
        var passwordHash = _blogService.CreatePasswordHash(registerAuthorRequest.Password);
        var user = await _blogService.GetAuthorByEmailAddress(registerAuthorRequest.EmailAddress);
        var username = await _blogService.GetAuthorByUsername(registerAuthorRequest.Username);

        if (user != null || registerAuthorRequest.Username == username?.Username)
            return BadRequest(new { error = "User already exist :(" });

        await _blogService.CreateUser(new Author
        {
            EmailAddress = registerAuthorRequest.EmailAddress,
            Username = registerAuthorRequest.Username,
            Description = registerAuthorRequest.Description,
            PasswordHash = await passwordHash
        });
        return Ok(new { result = "Registration Successful :)" });
    }

    [HttpPost]
    public async Task<ActionResult<JwtDto>> Login(LoginAuthorRequest loginAuthorRequest)
    {
        var author = await _blogService.GetAuthorByEmailAddress(loginAuthorRequest.EmailAddress);

        if (author == null) return BadRequest(new { error = "Wrong username/password :/" });
        if (_blogService.VerifyPassword(loginAuthorRequest.Password, author) == false)
            return BadRequest(new { error = "Wrong username/password :/" });

        return Ok(new JwtDto { AccessToken = await _blogService.CreateToken(author) });
    }
}