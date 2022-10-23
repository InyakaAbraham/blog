using Blog.Api.Dtos;
using Blog.Features;
using Blog.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[AllowAnonymous]
[ProducesResponseType(typeof(UserInputErrorDto), 400)]
public class AuthorController : AbstractController
{
    private readonly IBlogService _blogService;
    private readonly IValidator<LoginAuthorRequest> _loginAuthorRequest;
    private readonly IValidator<RegisterAuthorRequest> _registerAuthorRequest;


    public AuthorController(IBlogService blogService, IValidator<LoginAuthorRequest> loginAuthorRequest, IValidator<RegisterAuthorRequest> registerAuthorRequest)
    {
        _blogService = blogService;
        _loginAuthorRequest = loginAuthorRequest;
        _registerAuthorRequest = registerAuthorRequest;
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> Register(RegisterAuthorRequest registerAuthorRequest)
    {
        var result = await _registerAuthorRequest.ValidateAsync(registerAuthorRequest);
       
        if (!result.IsValid)
        {
            return BadRequest(new UserInputErrorDto(result));
        }
        
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
        var result = await _loginAuthorRequest.ValidateAsync(loginAuthorRequest);
       
        if (!result.IsValid)
        {
            return BadRequest(new UserInputErrorDto(result));
        }
       
        var author = await _blogService.GetAuthorByEmailAddress(loginAuthorRequest.EmailAddress);

        if (author == null) return BadRequest(new { error = "Wrong username/password :/" });
        if (_blogService.VerifyPassword(loginAuthorRequest.Password, author) == false)
            return BadRequest(new { error = "Wrong username/password :/" });

        return Ok(new JwtDto(new JwtDto.Credentials{ AccessToken = await _blogService.CreateToken(author) }));
    }
}