using System.Text.Json.Serialization;
using Blog.Api.Dtos;
using Blog.Features;
using Blog.Models;
using Blog.Models.Enums;
using Blog.Models.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers;

[Route("api/[controller]/[action]")]
[Attributes.Authorize(UserRole.Default)]
public class BlogController : AbstractController
{
    private readonly IBlogService _blogService;

    public BlogController(IBlogService blogService)
    {
        _blogService = blogService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<BlogPost>>> GetAllPosts([FromQuery] BlogParameters blogParameters)
    {
        var post = await _blogService.GetAllPosts(blogParameters);
        var metadata = new
        {
            post.TotalCount,
            post.CurrentPage,
            post.PageSize,
            post.HasNext,
            post.HasPrevious
        };
        Response.Headers.Add("X-Pagination",metadata.ToString());
        return Ok(post);
    }

    [HttpGet]
    [Attributes.Authorize(UserRole.Author)]
    public async Task<List<BlogPost>> GetPostByAuthor([FromQuery] BlogParameters blogParameters)
    {
        var userId = GetContextUserId();
        return await _blogService.GetPostByAuthor(userId, blogParameters);
    }

    [HttpGet("id")]
    [Attributes.Authorize(UserRole.Administrator, UserRole.Moderator)]
    public async Task<ActionResult<BlogPost>> GetPostById(long id)
    {
        var blogPost = await _blogService.GetPostById(id);
        if (blogPost == null) return NotFound(new { error = "Not Found :/" });
        return Ok(blogPost);
    }

    [HttpGet("title")]
    [AllowAnonymous]
    public async Task<ActionResult<List<BlogPost>>> GetPostByTitle(string title,
        [FromQuery] BlogParameters blogParameters)
    {
        return Ok(await _blogService.GetPostByTitle(title, blogParameters));
    }

    [HttpPost]
    [Attributes.Authorize(UserRole.Author)]
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
        if (blogPost == null) return BadRequest(new { error = "Kindly enter a post in the right format" });
        return Ok(blogPost);
    }


    [HttpPut]
    [Attributes.Authorize(UserRole.Administrator, UserRole.Moderator)]
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
        if (blogPost == null) return NotFound(new { error = "Not Found :/" });
        return Ok(blogPost);
    }

    [HttpDelete("id")]
    [Attributes.Authorize(UserRole.Administrator)]
    public async Task<ActionResult> DeletePost(long id)
    {
        await _blogService.DeletePost(id);
        return Ok(new { error = "Post Deleted :(" });
    }
}