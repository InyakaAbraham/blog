using Blog.Api.Blog.Model.Dto;
using Blog.Features;
using Blog.Models;
using Blog.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class BlogController : ControllerBase
{
    private readonly IBlogService _blogService;
    private readonly DataContext _dataContext;

    public BlogController(DataContext dataContext, IBlogService blogService)
    {
        _blogService = blogService;
        _dataContext = dataContext;
    }

    [HttpGet]
    [AllowAnonymous]
    public Task<List<BlogPost>> GetAllPost()
    {
        return _blogService.GetAllPost();
    }

    [HttpGet("id")]
    public async Task<ActionResult<BlogPost>> GetPostById(int id)
    {
        var res = await _blogService.GetPostById(id);
        if (res == null) return BadRequest("No post with such Id");

        return Ok(res);
    }

    [HttpGet("id")]
    public async Task<ActionResult<List<BlogPost>>> GetPostByAuthor(int id)
    {
        var res = await _blogService.GetPostByAuthor(id);
        if (res == null) return BadRequest("No author with such Id");
        return Ok(res);
    }

    [HttpPost]
    public async Task<ActionResult<BlogPost>> AddPost(NewPostDto newPost)
    {
        var author = await _dataContext.Authors.Where(x => x.AuthorId == newPost.AuthorId)
            .Include(x => x.BlogPosts)
            .FirstOrDefaultAsync();

        var category = await _dataContext.Categories
            .Where(x => x.CategoryName.ToUpper() == newPost.CategoryName.ToUpper())
            .FirstOrDefaultAsync();

        BlogPost? res = null;
        var post = new BlogPost
        {
            Title = newPost.Title,
            Body = newPost.Body,
            Summary = newPost.Summary,
            Category = category,
            Author = author,
            CategoryName = newPost.CategoryName,
            AuthorId = newPost.AuthorId,
            Tags = newPost.Tags,
            PostId = newPost.Id,
            Created = newPost.Created,
            Updated = newPost.Updated
        };
        res = await _blogService.AddPost(post);
        if (res == null) return BadRequest("Kindly add a post in the valid format");
        return Ok(res);
    }

    [HttpPost]
    public async Task<ActionResult<Author>> AddAuthor(Author newAuthor)
    {
        var res = await _blogService.AddAuthor(newAuthor);
        if (res == null) return BadRequest("Kindly enter a valid Author field");

        return Ok(res);
    }

    [HttpPost]
    public async Task<ActionResult<Category>> AddCategory(Category newCategory)
    {
        var res = await _blogService.AddCategory(newCategory);
        if (res == null) return BadRequest("Kindly enter a valid Author field");

        return Ok(res);
    }

    [HttpPut]
    public async Task<ActionResult<BlogPost>> UpdatePost(NewPostDto updatePost)
    {
        var author = await _dataContext.Authors.Where(x => x.AuthorId == updatePost.AuthorId)
            .Include(x => x.BlogPosts)
            .FirstOrDefaultAsync();
        var category = await _dataContext.Categories
            .Where(x => x.CategoryName.ToUpper() == updatePost.CategoryName.ToUpper())
            .FirstOrDefaultAsync();

        BlogPost? res = null;

        var post = await _dataContext.BlogPosts.FindAsync(updatePost.Id);
        post.Author = author;
        post.Category = category;
        post.Body = updatePost.Body;
        post.Summary = updatePost.Summary;
        post.Tags = updatePost.Tags;
        post.Title = updatePost.Title;
        post.Created = post.Created;
        post.Updated = DateTime.UtcNow;

        res = await _blogService.UpdatePost(post);
        if (res == null) return BadRequest("No post with such id");

        return Ok(res);
    }

    [HttpDelete("id")]
    public Task DeletePost(int id)
    {
        return _blogService.DeletePost(id);
    }
}