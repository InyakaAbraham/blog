using Blog.Api.Dtos;
using Blog.Features;
using Blog.Models;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IBlogService _blogService;

    public UserController(IBlogService blogService)
    {
        _blogService = blogService;
    }

    [HttpPost]
    public async Task<ActionResult> Register(RegisterUserRequest request)
    {
        var passwordHash = _blogService.CreatePasswordHash(request.Password);
        var user = await _blogService.GetUserByEmailAddress(request.EmailAddress);

        if (user != null || user?.Username == request.Username)
            return BadRequest("User already exist :(");

        await _blogService.CreateUser(new User
        {
            EmailAddress = request.EmailAddress,
            Username = request.Username,
            PasswordHash = await passwordHash
        });

        return Ok("Registration Successful:)");
    }

    [HttpPost]
    public async Task<ActionResult<JwtDto>> Login(LoginUserRequest loginUserRequest)
    {
        var user = await _blogService.GetUserByEmailAddress(loginUserRequest.EmailAddress);

        if (user == null) return BadRequest("Wrong username/password");
        if (_blogService.VerifyPassword(loginUserRequest.Password, user) == false)
            return BadRequest("Wrong username/password");

        return Ok(new JwtDto { AccessToken = await _blogService.CreateToken(user) });
    }
}