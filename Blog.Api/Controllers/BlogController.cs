using Blog.Api.Dtos;
using Blog.Features;
using Blog.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers;

[Route("api/[controller]/[action]")]
[Framework.Attributes.Authorize(UserRole.Default)]
[ProducesResponseType(typeof(UserNotAuthenticatedResponseDto), 401)]
[ProducesResponseType(typeof(UserNotAuthorizedResponseDto), 403)]
[ProducesResponseType(typeof(UserInputErrorDto), 400)]
public class BlogController : AbstractController
{
    private readonly IBlogService _blogService;
    private readonly IUserService _userService;
    private readonly IValidator<NewBlogPostDto> _validator;

    public BlogController(IBlogService blogService, IUserService userService, IValidator<NewBlogPostDto> validator)
    {
        _blogService = blogService;
        _userService = userService;
        _validator = validator;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedBlogPostResponseDto), 200)]
    public async Task<ActionResult<PagedBlogPostResponseDto>> GetAllPosts([FromQuery] PageParameters pageParameters)
    {
        var post = await _blogService.GetAllPosts(pageParameters);
        return Ok(new PagedBlogPostResponseDto(post, new PaginatedDto<List<BlogPostResponse>>.PageInformation
        {
            TotalPages = post.TotalPages,
            TotalCount = post.TotalCount,
            CurrentPage = post.CurrentPage,
            PageSize = post.PageSize,
            HasNext = post.HasNext,
            HasPrevious = post.HasPrevious
        }));
    }

    [HttpGet("{id}")]
    [Framework.Attributes.Authorize(UserRole.Author)]
    [ProducesResponseType(typeof(PagedBlogPostResponseDto), 200)]
    public async Task<ActionResult<PagedBlogPostResponseDto>> GetPostByAuthor([FromQuery] PageParameters pageParameters,long id)
    {
        var post = await _blogService.GetPostByAuthor(pageParameters, id);
        return Ok(new PagedBlogPostResponseDto(post, new PaginatedDto<List<BlogPostResponse>>.PageInformation
        {
            HasNext = post.HasNext,
            CurrentPage = post.CurrentPage,
            HasPrevious = post.HasPrevious,
            PageSize = post.PageSize,
            TotalCount = post.TotalCount,
            TotalPages = post.TotalPages
        }));
    }

    [HttpGet("{id}")]
    [Framework.Attributes.Authorize(UserRole.Administrator, UserRole.Moderator)]
    [ProducesResponseType(typeof(SuccessResponseDto<BlogPost>), 200)]
    public async Task<ActionResult<SuccessResponseDto<BlogPost>>> GetPostById(long id)
    {
        var blogPost = await _blogService.GetPostById(id);
        if (blogPost == null) return NotFound(new { error = "Not Found :/" });
        return Ok(new SuccessResponseDto<BlogPost>(blogPost));
    }

    [HttpGet("{title}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedBlogPostResponseDto), 200)]
    public async Task<ActionResult<PagedBlogPostResponseDto>> GetPostByTitle(string title,
        [FromQuery] PageParameters pageParameters)
    {
        var post = await _blogService.GetPostByTitle(title, pageParameters);
        return Ok(new PagedBlogPostResponseDto(post, new PaginatedDto<List<BlogPostResponse>>.PageInformation
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
    [Framework.Attributes.Authorize(UserRole.Author)]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> AddPost([FromForm] NewBlogPostDto newBlogPost)
    {
        var result = await _validator.ValidateAsync(newBlogPost);

        if (!result.IsValid) return BadRequest(new UserInputErrorDto(result));

        var authorId = GetContextUserId();
        var author = await _userService.GetAuthorById(authorId);
        var coverImagePath = await _blogService.UploadFile(newBlogPost.CoverImage);
        var category = await _blogService.AddCategory(new Category
        {
            CategoryName = newBlogPost.CategoryName
        });
        var blogPost = await _blogService.AddPost(new BlogPost
        {
            CoverImagePath = coverImagePath,
            Title = newBlogPost.Title,
            Body = newBlogPost.Body,
            Summary = newBlogPost.Summary,
            Category = category,
            CategoryName = category?.CategoryName,
            AuthorId = authorId,
            Tags = newBlogPost.Tags,
            Author = author,
            Updated = newBlogPost.Updated,
            Created = DateTime.UtcNow
        });

        if (blogPost == null) return BadRequest(new UserInputErrorDto("Enter post in valid format :("));

        return Ok(new EmptySuccessResponseDto("Post Created :)"));
    }


    [HttpPatch]
    [Framework.Attributes.Authorize(UserRole.Administrator, UserRole.Moderator)]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> UpdatePost([FromForm] NewBlogPostDto updateBlogPost,
        long id)
    {
        var result = await _validator.ValidateAsync(updateBlogPost);

        if (!result.IsValid) return BadRequest(new UserInputErrorDto(result));

        var coverImagePath = await _blogService.UploadFile(updateBlogPost.CoverImage);
        var post = await _blogService.GetPostById(id);
        
        if (post == null) return BadRequest(new UserInputErrorDto("No post with Id"));
        
        post.Body = updateBlogPost.Body;
        post.Summary = updateBlogPost.Summary;
        post.Tags = updateBlogPost.Tags;
        post.Title = updateBlogPost.Title;
        post.CoverImagePath = coverImagePath;
        post.Updated = DateTime.UtcNow;

        await _blogService.UpdatePost(post);

        return Ok(new EmptySuccessResponseDto("Post updated :)"));
    }

    [HttpDelete("{id}")]
    [Framework.Attributes.Authorize(UserRole.Administrator)]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> DeletePost(long id)
    {
        await _blogService.DeletePost(id);
        return Ok(new EmptySuccessResponseDto("Post Deleted :("));
    }
}