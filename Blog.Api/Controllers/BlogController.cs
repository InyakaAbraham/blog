using Blog.Api.Dtos;
using Blog.Features;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class BlogController : AbstractController
{
    private readonly IBlogService _blogService;

    public BlogController(IBlogService blogService)
    {
        _blogService = blogService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<BlogPost>>> GetAllPosts()
    {
        return Ok(await _blogService.GetAllPosts());
    }
 
    [HttpGet]
    [Attributes.Authorize(Role.Author)]
    public async Task<List<BlogPost?>> GetPostByAuthor()
    {
        var userId = GetContextUserId();
        return await _blogService.GetPostByAuthor(userId);
    }
    
    [HttpGet("id")]
    [Attributes.Authorize(Role.Administrator, Role.Moderator)]
    public async Task<ActionResult<BlogPost>> GetPostById(long id)
    {
        var blogPost = await _blogService.GetPostById(id);
        if (blogPost == null) return NotFound("Not Found");
        return Ok(blogPost);
    }

    [HttpGet("title")]
    [AllowAnonymous]
    public async Task<ActionResult<List<BlogPost>>> GetPostByTitle(string title)
    {
        return Ok(await _blogService.GetPostByTitle(title));
    }

    [HttpPost]
    [Attributes.Authorize(Role.Author)]
    public async Task<ActionResult<BlogPost>> AddPost(NewPostDto newPost)
    {
        var author = await _blogService.GetAuthorById(newPost.AuthorId);
        var category = await _blogService.GetCategoryByName(newPost.CategoryName);
        var authorId = GetContextUserId();
        var blogPost = await _blogService.AddPost(new BlogPost
        {
            Title = newPost.Title,
            Body = newPost.Body,
            Summary = newPost.Summary,
            Category = category,
            CategoryName = newPost.CategoryName,
            AuthorId = authorId,
            Tags = newPost.Tags,
            Author = author
        });
        if (blogPost == null) return BadRequest("Kindly add a post in the valid format");
        return Ok(blogPost);
    }


    [HttpPut]
    [Attributes.Authorize(Role.Administrator, Role.Moderator)]
    public async Task<ActionResult<BlogPost>> UpdatePost(NewPostDto updatePost)
    {
        var post = await _blogService.GetPostById(updatePost.PostId) ?? new BlogPost();
        post.Author = post.Author;
        post.Category = post.Category;
        post.Body = updatePost.Body;
        post.Summary = updatePost.Summary;
        post.Tags = updatePost.Tags;
        post.Title = updatePost.Title;
        post.Created = post.Created;
        post.Updated = DateTime.UtcNow;

        var blogPost = await _blogService.UpdatePost(post);
        if (blogPost == null) return NotFound("Not Found");
        return Ok(blogPost);
    }

    [HttpDelete("id")]
    [Attributes.Authorize(Role.Administrator)]
    public async Task<ActionResult> DeletePost(long id)
    {
        await _blogService.DeletePost(id);
        return Ok("Deleted :(");
    }
}