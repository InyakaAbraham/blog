

using Blog.Api.Blog.Model.Dto;
using Blog.Models;
using Blog.Service;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers
{
    namespace JWTTutorial.Controllers
    {
        [Route("api/[controller]/[action]")]
        [ApiController]

        public class UserController : ControllerBase
        {
            
            private readonly IUserService _userService;
            public UserController(IUserService userService)
            {
                _userService = userService;
            }
            
            [HttpPost]
            public async Task<ActionResult<User>> Register(UserDto userDto)
            {
                var res = await _userService.RegisterUser(userDto);
                if (res==null)
                {
                    return BadRequest("User already exist");
                }
                return Ok(res);
                
            }

            [HttpPost]
            public async Task<ActionResult<string>> Login(UserDto userDto)
            {
                var res = await _userService.LoginUser(userDto);
                if (res==null)
                {
                    return BadRequest("Wrong username/password");
                }
                return Ok(res);
            }
        }
    }
}