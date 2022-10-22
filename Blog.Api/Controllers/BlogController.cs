using System.Text.Json;
using Blog.Api.Dtos;
using Blog.Features;
using Blog.Models;
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
    public async Task<ActionResult<PagedBlogPostResponseDto>> GetAllPosts([FromQuery] PageParameters pageParameters)
    {
        var post = await _blogService.GetAllPosts(pageParameters);
        return Ok(new PagedBlogPostResponseDto(post,new PaginatedDto<List<BlogPost>>.PageInformation()
        {
            TotalPages = post.TotalPages,
            TotalCount=post.TotalCount,
            CurrentPage=post.CurrentPage,
            PageSize=post.PageSize,
            HasNext=post.HasNext,
            HasPrevious=post.HasPrevious
        }));
    }

    [HttpGet]
    [Attributes.Authorize(UserRole.Author)]
    public async Task<ActionResult<PagedBlogPostResponseDto>> GetPostByAuthor([FromQuery] PageParameters pageParameters)
    {
        var userId = GetContextUserId();
        var post = await _blogService.GetPostByAuthor(userId, pageParameters);
        return Ok(new PagedBlogPostResponseDto(post,new PaginatedDto<List<BlogPost>>.PageInformation()
        {
            HasNext = post.HasNext,
            CurrentPage = post.CurrentPage,
            HasPrevious = post.HasPrevious,
            PageSize = post.PageSize,
            TotalCount = post.TotalCount,
            TotalPages = post.TotalPages
        }));
    }

    [HttpGet("id")]
    [Attributes.Authorize(UserRole.Administrator, UserRole.Moderator)]
    public async Task<ActionResult<SuccessResponseDto<BlogPost>>> GetPostById(long id)
    {
        var blogPost = await _blogService.GetPostById(id);
        if (blogPost == null) return NotFound(new { error = "Not Found :/" });
        return Ok(new SuccessResponseDto<BlogPost>(blogPost));
    }

    [HttpGet("title")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedBlogPostResponseDto>> GetPostByTitle(string title,
        [FromQuery] PageParameters pageParameters)
    {
        var post = await _blogService.GetPostByTitle(title, pageParameters);
        return Ok(new PagedBlogPostResponseDto(post,new PaginatedDto<List<BlogPost>>.PageInformation()
        {
            HasNext = post.HasNext,
            CurrentPage = post.CurrentPage,
            HasPrevious = post.HasPrevious,
            PageSize = post.PageSize,
            TotalCount = post.TotalCount,
            TotalPages = post.TotalPages
        }));
    }

    [HttpPost]
    [Attributes.Authorize(UserRole.Author)]
    public async Task<ActionResult<BlogPost>> AddPost([FromBody]NewPostDto newPost)
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
    public async Task<ActionResult<BlogPost>> UpdatePost([FromBody]NewPostDto updatePost)
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