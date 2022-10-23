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
    private readonly IValidator<ChangePasswordRequest> _changePasswordRequestValidator;
    private readonly IUserService _userService;
    private readonly IValidator<LoginAuthorRequest> _loginAuthorRequest;
    private readonly IValidator<RegisterAuthorRequest> _registerAuthorRequest;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordRequestValidator;



    public AuthorController(
        IBlogService blogService,
        IValidator<LoginAuthorRequest> loginAuthorRequest,
        IValidator<RegisterAuthorRequest> registerAuthorRequest,
        IValidator<ResetPasswordRequest> resetPasswordRequestValidator,
        IValidator<ChangePasswordRequest> changePasswordRequestValidator,
        IUserService userService
    )
    {
        _loginAuthorRequest = loginAuthorRequest;
        _registerAuthorRequest = registerAuthorRequest;
        _resetPasswordRequestValidator = resetPasswordRequestValidator;
        _changePasswordRequestValidator = changePasswordRequestValidator;
        _userService = userService;
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

        var passwordHash = _userService.CreatePasswordHash(registerAuthorRequest.Password);
        var user = await _userService.GetAuthorByEmailAddress(registerAuthorRequest.EmailAddress);
        var username = await _userService.GetAuthorByUsername(registerAuthorRequest.Username);

        if (user != null || registerAuthorRequest.Username == username?.Username)
            return BadRequest(new UserInputErrorDto());

        await _userService.CreateUser(new Author
        {
            EmailAddress = registerAuthorRequest.EmailAddress,
            Username = registerAuthorRequest.Username,
            FirstName = registerAuthorRequest.FirstName,
            LastName = registerAuthorRequest.LastName,
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
        var user = await _userService.GetAuthorByEmailAddress(emailAddress);
        if (user == null)
            return BadRequest(new UserInputErrorDto());

        if (await _userService.VerifyAuthor(emailAddress, token) == false)
            return BadRequest(new UserInputErrorDto());

        return Ok(new EmptySuccessResponseDto());
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<JwtDto>> Login(LoginAuthorRequest loginAuthorRequest)
    {
        var result = await _loginAuthorRequest.ValidateAsync(loginAuthorRequest);

        if (!result.IsValid) return BadRequest(new UserInputErrorDto(result));

        var author = await _userService.GetAuthorByEmailAddress(loginAuthorRequest.EmailAddress);

        if (author == null || !await _userService.VerifyPassword(loginAuthorRequest.Password, author))
            return BadRequest(new UserInputErrorDto());

        if (author?.VerifiedAt == null) return BadRequest(new UserInputErrorDto("Not Verified :("));

        author.LastLogin = DateTime.UtcNow;
        await _userService.UpdateAuthor(author);

        return Ok(new JwtDto
            (new JwtDto.Credentials { AccessToken = await _userService.CreateJwtToken(author) }));
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> ForgotPassword(string emailAddress)
    {
        if (await _userService.ForgotPassword(emailAddress) == false)
            return BadRequest(new UserInputErrorDto());

        return Ok(new EmptySuccessResponseDto());
    }

    [HttpPatch]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> ResetPassword([FromForm] ResetPasswordRequest request)
    {
        var result = await _resetPasswordRequestValidator.ValidateAsync(request);

        if (!result.IsValid) return BadRequest(new UserInputErrorDto(result));

        if (await _userService.ResetPassword(request.emailAddress, request.Token, request.Password) == false)
            return BadRequest(new UserInputErrorDto());

        return Ok(new EmptySuccessResponseDto());
    }

    [HttpPatch]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> ChangePassword(ChangePasswordRequest request)
    {
        var result = await _changePasswordRequestValidator.ValidateAsync(request);

        if (!result.IsValid) return BadRequest(new UserInputErrorDto(result));

        var authorId = GetContextUserId();
        var author = await _userService.GetAuthorById(authorId);

        if (author == null) return BadRequest();

        var response = await _userService.ChangePassword(request.OldPassword, request.Password, request.EmailAddress);

        if (!response)
        {
            return BadRequest(new UserInputErrorDto());
        }

        await _userService.UpdateAuthor(author);
        return Ok(new EmptySuccessResponseDto());

    }
}

