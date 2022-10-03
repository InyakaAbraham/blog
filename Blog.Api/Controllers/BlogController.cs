using Blog.Api.Dtos;
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
        return _blogService.GetAllPosts();
    }

    [HttpGet("id")]
    public async Task<ActionResult<BlogPost>> GetPostById(int id)
    {
        var res = await _blogService.GetPostById(id);
        if (res == null) return BadRequest("No post with such Id");

        return Ok(res);
    }

    [HttpGet("id")]
    public async Task<ActionResult<List<BlogPost>>> GetPostsByAuthor(int id)
    {
        var res = await _blogService.GetPostsByAuthor(id);
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

        var blogPost = await _blogService.AddPost(post);

        if (blogPost == null) return BadRequest("Kindly add a post in the valid format");

        return Ok(blogPost);
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
        var category = await _blogService.AddCategory(newCategory);

        if (category == null) return BadRequest("Kindly enter a valid Author field");

        return Ok(category);
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

        var post = await _dataContext.BlogPosts.FindAsync(updatePost.Id) ?? new BlogPost();
        post.Author = author;
        post.Category = category;
        post.Body = updatePost.Body;
        post.Summary = updatePost.Summary;
        post.Tags = updatePost.Tags;
        post.Title = updatePost.Title;
        post.Created = post.Created;
        post.Updated = DateTime.UtcNow;

        var blogPost = await _blogService.UpdatePost(post);

        if (blogPost == null) return BadRequest("No post with such id");

        return Ok(blogPost);
    }

    [HttpDelete("id")]
    public Task DeletePost(int id)
    {
        return _blogService.DeletePost(id);
    }
}