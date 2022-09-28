using Blog.Model.DTO;
using Blog.Models;
using Blog.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class BlogApiController : ControllerBase
    {
        private readonly IBlogService _blogService;

        public BlogApiController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        [HttpGet,AllowAnonymous]
        public Task<List<BlogPost>> GetAllPost()
        {
            return _blogService.GetAllPost();
        }

        [HttpGet("id")]
        public async Task<ActionResult<BlogPost>> GetPostById(int id)
        {
            var res = await _blogService.GetPostById(id);
            if (res == null)
            {
                return BadRequest("No post with such Id");
            }

            return Ok(res);
        }

        [HttpGet("id")]
        public async Task<ActionResult<List<BlogPost>>> GetPostByAuthor(int id)
        {
            var res = await _blogService.GetPostByAuthor(id);
            if (res==null)
            {
                return BadRequest("No author with such Id");
            }
            return Ok(res);
        }

        [HttpPost]
        public async Task<ActionResult<BlogPost>> AddPost(NewPostDto newPost)
        {
            var res = await _blogService.AddPost(newPost);
            if (res == null)
            {
                return BadRequest("Kindly add an Author to this post");
            }

            return Ok(res);
        }

        [HttpPost]
        public async Task<ActionResult<Author>> AddAuthor(NewAuthorDto newAthor)
        {
            var res = await _blogService.AddAuthor(newAthor);
            if (res == null)
            {
                return BadRequest("Kindly enter a valid Author field");
            }

            return Ok(res);
        }

        [HttpPost]
        public async Task<ActionResult<Category>> AddCategory(NewCategoryDto newCategory)
        {
            var res = await _blogService.AddCategory(newCategory);
            if (res == null)
            {
                return BadRequest("Kindly enter a valid Author field");
            }

            return Ok(res);
        }

        [HttpPut]
        public async Task<ActionResult<BlogPost>> UpdatePost(NewPostDto updatePost)
        {
            var res = await _blogService.UpdatePost(updatePost);
            if (res == null)
            {
                return BadRequest("No post with such id");
            }

            return Ok(res);
        }

        [HttpDelete("id")]
        public Task DeletePost(int id)
        {
            return _blogService.DeletePost(id);
        }
    }
}
