using Blog.Api.Dtos;
using Blog.Features;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[AllowAnonymous]
public class AuthorController : AbstractController
{
    private readonly IBlogService _blogService;

    public AuthorController(IBlogService blogService)
    {
        _blogService = blogService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> Register(RegisterAuthorRequest registerAuthorRequest)
    {
        var passwordHash = _blogService.CreatePasswordHash(registerAuthorRequest.Password);
        var user = await _blogService.GetAuthorByEmailAddress(registerAuthorRequest.EmailAddress);
        var username = await _blogService.GetAuthorByUsername(registerAuthorRequest.Username);

        if (user != null || registerAuthorRequest.Username == username?.Username)
            return BadRequest(new UserInputErrorDto());

        await _blogService.CreateUser(new Author
        {
            EmailAddress = registerAuthorRequest.EmailAddress,
            Username = registerAuthorRequest.Username,
            Description = registerAuthorRequest.Description,
            PasswordHash = await passwordHash
        });
        return Ok(new EmptySuccessResponseDto());
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<JwtDto>> Login(LoginAuthorRequest loginAuthorRequest)
    {
        var author = await _blogService.GetAuthorByEmailAddress(loginAuthorRequest.EmailAddress);

        if (author == null) return BadRequest(new { error = "Wrong username/password :/" });
        if (_blogService.VerifyPassword(loginAuthorRequest.Password, author) == false)
            return BadRequest(new { error = "Wrong username/password :/" });

        return Ok(new JwtDto(new JwtDto.Credentials{ AccessToken = await _blogService.CreateToken(author) }));
    }
}