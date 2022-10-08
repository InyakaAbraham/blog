using Blog.Api.Dtos;
using Blog.Features;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class BlogController : ControllerBase
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

    [HttpGet("id")]
    public async Task<ActionResult<BlogPost>> GetPostById(int id)
    {
        var blogPost = await _blogService.GetPostById(id);
        if (blogPost == null) return BadRequest("Not Found");
        return Ok(blogPost);
    }

    [HttpGet("id")]
    public async Task<ActionResult<List<BlogPost>>> GetPostsByAuthor(int id)
    {
        var blogPosts = await _blogService.GetPostsByAuthor(id);
        return Ok(blogPosts);
    }

    [HttpPost]
    public async Task<ActionResult<BlogPost>> AddPost(NewPostDto newPost)
    {
        var author = await _blogService.GetAuthorById(newPost.AuthorId);
        var category = await _blogService.GetCategoryByName(newPost.CategoryName);
        var blogPost = await _blogService.AddPost(new BlogPost
        {
            Title = newPost.Title,
            Body = newPost.Body,
            Summary = newPost.Summary,
            Category = category,
            CategoryName = newPost.CategoryName,
            AuthorId = newPost.AuthorId,
            Tags = newPost.Tags,
            PostId = newPost.Id,
            Created = newPost.Created,
            Updated = newPost.Updated,
            Author = author
        });
        if (blogPost == null) return BadRequest("Kindly add a post in the valid format");
        return Ok(blogPost);
    }

    [HttpPost]
    public async Task<ActionResult<Author?>> AddAuthor(Author newAuthor)
    {
        var author = await _blogService.AddAuthor(newAuthor);
        return Ok(author);
    }

    [HttpPost]
    public async Task<ActionResult<Category>> AddCategory(Category newCategory)
    {
        var category = await _blogService.AddCategory(newCategory);
        return Ok(category);
    }

    [HttpPut]
    public async Task<ActionResult<BlogPost>> UpdatePost(NewPostDto updatePost)
    {
        var author = await _blogService.GetAuthorById(updatePost.Id);
        var category = await _blogService.GetCategoryByName(updatePost.CategoryName);
        var post = await _blogService.GetPostById(updatePost.Id) ?? new BlogPost();
        post.Author = author;
        post.Category = category;
        post.Body = updatePost.Body;
        post.Summary = updatePost.Summary;
        post.Tags = updatePost.Tags;
        post.Title = updatePost.Title;
        post.Created = post.Created;
        post.Updated = DateTime.UtcNow;

        var blogPost = await _blogService.UpdatePost(post);
        if (blogPost == null) return BadRequest("Not Found");
        return Ok(blogPost);
    }

    [HttpDelete("id")]
    public ActionResult<Task> DeletePost(int id)
    {
        return Ok(_blogService.DeletePost(id));
    }
}