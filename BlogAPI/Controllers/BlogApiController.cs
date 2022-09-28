using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogAPI.Model;
using BlogAPI.ModelDTO;
using BlogAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlogAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BlogApiController : ControllerBase
    {
        private readonly IBlogService _blogService;

        public BlogApiController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        [HttpGet]
        public Task<List<BlogPost>> GetAllPost()
        {
            return _blogService.GetAllPost();
        }

        [HttpGet("id")]
        public async Task<ActionResult<BlogPost>> GetPostById(int id)
        {
            var res = await _blogService.GetPostById(id);
            if (res==null)
            {
                return BadRequest("No post with such Id");
            }

            return Ok(res);
        }
        
        // [HttpGet("id")]
        // public async Task<ActionResult<List<Author>>> GetPostByAuthor(int id)
        // {
        //     var res = await _blogService.GetPostByAuthor(id);
        //     if (res==null)
        //     {
        //         return BadRequest("No post with such Id");
        //     }
        //     return Ok(res);
        // }
        
        [HttpPost]
        public async Task<ActionResult<BlogPost>> AddPost(NewPostDTO newPost)
        {
            var res =  await _blogService.AddPost(newPost);
            if (res==null)
            {
                return BadRequest("Kindly add an Author to this post");
            }

            return Ok(res);
        }
        
        [HttpPost]
        public async Task<ActionResult<Author>> AddAuthor(NewAuthorDTO newAthor)
        {
            var res = await _blogService.AddAuthor(newAthor);
            if (res==null)
            {
                return BadRequest("Kindly enter a valid Author field");
            }

            return Ok(res);
        }
        
        [HttpPost]
        public async Task<ActionResult<Category>> AddCategory(NewCategoryDTO newCategory)
        {
            var res = await _blogService.AddCategory(newCategory);
            if (res==null)
            {
                return BadRequest("Kindly enter a valid Author field");
            }

            return Ok(res);
        }

        [HttpPut]
        public async Task<ActionResult<BlogPost>> UpdatePost(NewPostDTO updatePost)
        {
            var res= await _blogService.UpdatePost(updatePost);
            if (res==null)
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