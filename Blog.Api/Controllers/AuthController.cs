using Blog.Model.DTO;
using Blog.Models;
using Blog.Service;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers
{
    namespace JWTTutorial.Controllers
    {
        [Route("api/[controller]/[action]")]
        [ApiController]

        public class AuthController : ControllerBase
        {
            private readonly IBlogService _blogService;
            public AuthController(IBlogService blogService)
            {
                _blogService = blogService;
            }
            
            [HttpPost]
            public async Task<ActionResult<User>> Register(UserDto userDto)
            {
                var res = await _blogService.RegisterUser(userDto);
                if (res==null)
                {
                    return BadRequest("User already exist");
                }
                return Ok(res);
                
            }

            [HttpPost]
            public async Task<ActionResult<string>> LoginUser(UserDto userDto)
            {
                var res = await _blogService.LoginUser(userDto);
                if (res==null)
                {
                    return BadRequest("Wrong username/password");
                }
                return Ok(res);
            }
            
            
            
            
        }
    }
}