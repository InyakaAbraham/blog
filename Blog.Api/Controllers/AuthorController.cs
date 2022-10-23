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
    private readonly IValidator<ChangeEmailAddressRequestDto> _changeEmailAddressRequestDto;
    private readonly IValidator<LoginAuthorRequest> _loginAuthorRequest;
    private readonly IValidator<RegisterAuthorRequest> _registerAuthorRequest;
    private readonly IValidator<UpdateAuthorRequestDto> _updateAuthorRequestDto;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordRequestValidator;
    private readonly IUserService _userService;


    public AuthorController(
        IValidator<LoginAuthorRequest> loginAuthorRequest,
        IValidator<RegisterAuthorRequest> registerAuthorRequest,
        IValidator<ResetPasswordRequest> resetPasswordRequestValidator,
        IValidator<ChangePasswordRequest> changePasswordRequestValidator,
        IUserService userService, IValidator<UpdateAuthorRequestDto> updateAuthorRequestDto, 
        IValidator<ChangeEmailAddressRequestDto> changeEmailAddressRequestDto)
    {
        _loginAuthorRequest = loginAuthorRequest;
        _registerAuthorRequest = registerAuthorRequest;
        _resetPasswordRequestValidator = resetPasswordRequestValidator;
        _changePasswordRequestValidator = changePasswordRequestValidator;
        _userService = userService;
        _updateAuthorRequestDto = updateAuthorRequestDto;
        _changeEmailAddressRequestDto = changeEmailAddressRequestDto;
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> Register(RegisterAuthorRequest registerAuthorRequest)
    {
        var result = await _registerAuthorRequest.ValidateAsync(registerAuthorRequest);

        if (!result.IsValid) return BadRequest(new UserInputErrorDto(result));

        var passwordHash = _userService.CreatePasswordHash(registerAuthorRequest.Password);
        var user = await _userService.GetAuthorByEmailAddress(registerAuthorRequest.EmailAddress);
        var username = await _userService.GetAuthorByUsername(registerAuthorRequest.Username);

        if (user != null || registerAuthorRequest.Username == username?.Username)
            return BadRequest(new UserInputErrorDto("User with email/username already exists :("));

        await _userService.CreateUser(new Author
        {
            EmailAddress = registerAuthorRequest.EmailAddress,
            Username = registerAuthorRequest.Username,
            FirstName = registerAuthorRequest.FirstName,
            LastName = registerAuthorRequest.LastName,
            Description = registerAuthorRequest.Description,
            PasswordHash = await passwordHash,
            CreatedAt = DateTime.UtcNow
        });
        return Ok(new EmptySuccessResponseDto("Registration Successfully :)"));
    }

    [HttpPatch]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> VerifyUser(string emailAddress, string token)
    {
        var author = await _userService.GetAuthorByEmailAddress(emailAddress);
        if (author == null)
            return BadRequest(new UserInputErrorDto("Kindly enter a registered email :("));

        if (await _userService.VerifyAuthor(emailAddress, token) == false)
            return BadRequest(new UserInputErrorDto("Invalid Token :("));

        return Ok(new EmptySuccessResponseDto("Account Verified :)"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<JwtDto>> Login(LoginAuthorRequest loginAuthorRequest)
    {
        var result = await _loginAuthorRequest.ValidateAsync(loginAuthorRequest);

        if (!result.IsValid) return BadRequest(new UserInputErrorDto(result));

        var author = await _userService.GetAuthorByEmailAddress(loginAuthorRequest.EmailAddress);

        if (author == null || !await _userService.VerifyPassword(loginAuthorRequest.Password, author))
            return BadRequest(new UserInputErrorDto("Incorrect username/password"));

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
            return BadRequest(new UserInputErrorDto("If email is registered, you will receive an OTP :)"));

        return Ok(new EmptySuccessResponseDto("If email is registered, you will receive an OTP :)"));
    }

    [HttpPatch]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> ResetPassword([FromForm] ResetPasswordRequest request)
    {
        var result = await _resetPasswordRequestValidator.ValidateAsync(request);

        if (!result.IsValid) return BadRequest(new UserInputErrorDto(result));

        if (await _userService.ResetPassword(request.emailAddress, request.Token, request.Password) == false)
            return BadRequest(new UserInputErrorDto("Invalid email/token :("));

        return Ok(new EmptySuccessResponseDto("Password Reset Successful"));
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

        if (author == null) return BadRequest("Kindly login to change password :)");

        var response = await _userService.ChangePassword(request.OldPassword, request.Password, request.EmailAddress);

        if (!response) return BadRequest(new UserInputErrorDto("Check entry field and try again :("));

        await _userService.UpdateAuthor(author);
        return Ok(new EmptySuccessResponseDto("Password changed Successfully"));
    }
    
    [HttpPatch]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> ChangeEmailAddress(string oldEmailAddress,string password)
    {
        var response = await _userService.ChangeEmailAddress(oldEmailAddress, password);

        if (!response) return BadRequest(new UserInputErrorDto("Incorrect email/password :("));

        return Ok(new EmptySuccessResponseDto("You can proceed to changing your Email Address :)"));
    }
    
    [HttpPatch]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> VerifyEmailChange(ChangeEmailAddressRequestDto request,string token)
    {
        var result = await _changeEmailAddressRequestDto.ValidateAsync(request);

        if (!result.IsValid) return BadRequest(new UserInputErrorDto(result));

        var response = await _userService.VerifyNewEmail(request.oldEmailAddress, request.newEmailAddress, token);

        if (!response) return BadRequest(new UserInputErrorDto("Check entry and try again:("));

        return Ok(new EmptySuccessResponseDto("Email Address changed Successfully :)"));
    }
    
    [HttpPatch]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> UpdateUser(UpdateAuthorRequestDto request)
    {
        var result = await _updateAuthorRequestDto.ValidateAsync(request);

        if (!result.IsValid) return BadRequest(new UserInputErrorDto(result));

        var authorId = GetContextUserId();
        var author = await _userService.GetAuthorById(authorId);
        var usernameCheck = await _userService.GetAuthorByUsername(request.Username);

        if (author == null) return BadRequest("Kindly login to edit profile :)");
        if (usernameCheck != null) return BadRequest("Username already Exists");
        
        author.Username = request.Username;
        author.FirstName = request.FirstName;
        author.LastName = request.LastName;
        author.Description = request.Description;
        
        await _userService.UpdateAuthor(author);
        return Ok(new EmptySuccessResponseDto("Profile Updated Successfully"));
    }
}