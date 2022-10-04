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
    public async Task<ActionResult<List<BlogPost>>> GetAllPosts()
    {
        var blogPosts = await _blogService.GetAllPosts();
        return Ok($"Successful\n{blogPosts}");
    }

    [HttpGet("id")]
    public async Task<ActionResult<BlogPost>> GetPostById(int id)
    {
        var blogPost = await _blogService.GetPostById(id);
        if (blogPost == null) return BadRequest("No post with such Id");

        return Ok($"Successful\nBelow is the post with id {id}\n{blogPost}");
    }

    [HttpGet("id")]
    public async Task<ActionResult<List<BlogPost>>> GetPostsByAuthor(int id)
    {
        var blogPosts = await _blogService.GetPostsByAuthor(id);
        if (blogPosts == null) return BadRequest("No author with such Id");
        return Ok($"Successful\nBelow is a list of post(s) by author with is {id}\n{blogPosts}");
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

        return Ok($"Successful\nBelow is the newly created BlogPost\n{blogPost}");
    }

    [HttpPost]
    public async Task<ActionResult<Author>> AddAuthor(Author newAuthor)
    {
        var author = await _blogService.AddAuthor(newAuthor);
        if (author == null) return BadRequest("Kindly enter a valid Author field");

        return Ok("Successful\nBelow is the newly added Author\n"+author);
    }

    [HttpPost]
    public async Task<ActionResult<Category>> AddCategory(Category newCategory)
    {
        var category = await _blogService.AddCategory(newCategory);

        if (category == null) return BadRequest("Kindly enter a valid Author field");

        return Ok("Successful\nBelow is the newly added Category\n"+category);
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

        return Ok("Successful\nBelow is the updated BlogPost\n"+blogPost);
    }

    [HttpDelete("id")]
    public ActionResult<Task> DeletePost(int id)
    {
        return Ok("Post deleted successfully"+_blogService.DeletePost(id));
    }
}