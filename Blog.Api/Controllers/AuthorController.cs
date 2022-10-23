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
            PasswordHash = await passwordHash,
            CreatedAt = DateTime.UtcNow,

        });
        return Ok(new EmptySuccessResponseDto());
    }

    [HttpPatch]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> VerifyUser(string emailAddress, string token)
    {
        var user = await _blogService.GetAuthorByEmailAddress(emailAddress);
        if (user == null)
            return BadRequest(new UserInputErrorDto());

        if (await _blogService.VerifyAuthor(emailAddress, token) == false)
            return BadRequest(new UserInputErrorDto());

        return Ok(new EmptySuccessResponseDto());
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<JwtDto>> Login(LoginAuthorRequest loginAuthorRequest)
    {
        var result = await _loginAuthorRequest.ValidateAsync(loginAuthorRequest);

        if (!result.IsValid) return BadRequest(new UserInputErrorDto(result));
        
        var author = await _blogService.GetAuthorByEmailAddress(loginAuthorRequest.EmailAddress);

        if (author == null || !await _blogService.VerifyPassword(loginAuthorRequest.Password,author))
            return BadRequest(new UserInputErrorDto());

        if (author?.VerifiedAt == null) return BadRequest(new UserInputErrorDto("Not Verified :("));
        
        author.LastLogin=DateTime.UtcNow;
        await _blogService.UpdateAuthor(author);
        
        return Ok(new JwtDto
            (new JwtDto.Credentials { AccessToken = await _blogService.CreateJwtToken(author) }));   }
}