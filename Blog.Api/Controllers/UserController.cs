using Blog.Api.Dtos;
using Blog.Features;
using Blog.Models;
using Blog.Persistence;
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
    public async Task<ActionResult<User>> Register(RegisterUserRequest registerUserRequest)
    {
        var passwordHash = _blogService.CreatePasswordHash(registerUserRequest.Password);
        var user = await _blogService.GetUserByEmailAddress(registerUserRequest.EmailAddress);
        
        if (user == null && user?.Username!=registerUserRequest.Username)
        {
            var newUser = new User
            {
                EmailAddress = registerUserRequest.EmailAddress,
                Username = registerUserRequest.Username,
                PasswordHash = await passwordHash
            };
            await _blogService.CreateUser(newUser);
            return Ok(newUser);
        }

        return BadRequest("User already exist");
    }

    [HttpPost]
    public async Task<ActionResult<JwtDto>> Login(LoginUserRequest loginUserRequest)
    {
        var user = await _blogService.GetUserByEmailAddress(loginUserRequest.EmailAddress);

        if (user == null) return BadRequest("Wrong username/password");
        if (_blogService.VerifyPassword(loginUserRequest.Password, user) == false)
        {
            return BadRequest("Wrong username/password");
        }
        
        return Ok(new JwtDto{AccessToken = await _blogService.CreateToken(user)});
    }
}