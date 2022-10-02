using Blog.Api.Dtos;
using Blog.Features;
using Blog.Models;
using Blog.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly DataContext _dataContext;

    private readonly IUserService _userService;

    public UserController(DataContext dataContext, IUserService userService)
    {
        _userService = userService;
        _dataContext = dataContext;
    }

    [HttpPost]
    public async Task<ActionResult<User>> Register(UserDto userDto)
    {
        _userService.CreatPasswordHash(userDto.Password, out var passwordHash, out var passwordSalt);
        var req = await _dataContext.Users.FirstOrDefaultAsync(x => x.Username == userDto.UserName);
        if (req == null)
        {
            var user = new User
            {
                Username = userDto.UserName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };
            _dataContext.Users.Add(user);
            await _dataContext.SaveChangesAsync();
            return Ok(user);
        }

        return BadRequest("User with username already exist");
    }

    [HttpPost]
    public async Task<ActionResult<string>> Login(UserDto userDto)
    {
        var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.Username == userDto.UserName);

        if (user == null) return BadRequest("Wrong username/password");

        if (_userService.VerifyPasswordHash(userDto.Password, user.PasswordHash, user.PasswordSalt) == false)
            return BadRequest("Wrong username/password");

        var token = _userService.CreateToken(user);
        return Ok(token);
    }
}