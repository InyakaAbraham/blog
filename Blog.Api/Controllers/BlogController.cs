using System.Diagnostics.CodeAnalysis;
using Blog.Api.Dtos;
using Blog.Api.Framework.Attributes;
using Blog.Domain.Blog.Commands;
using Blog.Domain.Blog.Queries;
using Blog.Features;
using Blog.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers;

[Route("api/[controller]/[action]")]
// [Framework.Attributes.Authorize(UserRole.Default)]
[ProducesResponseType(typeof(UserNotAuthenticatedResponseDto), 401)]
[ProducesResponseType(typeof(UserNotAuthorizedResponseDto), 403)]
[ProducesResponseType(typeof(UserInputErrorDto), 400)]
public class BlogController : AbstractController
{
    private readonly IMediator _mediator;
    private readonly IBlogService _blogService;
    private readonly IUserService _userService;
    private readonly IValidator<NewBlogPostDto> _validator;

    public BlogController(IMediator mediator,IBlogService blogService, IUserService userService, IValidator<NewBlogPostDto> validator)
    {
        _mediator = mediator;
        _blogService = blogService;
        _userService = userService;
        _validator = validator;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BlogQueryResponse), 200)]
    public async Task<ActionResult<List<BlogQueryResponse>>> GetAllPosts(CancellationToken cancellationToken )
    {
        var request = new BlogQueryRequest()
        {
          PageParameters =null,
          AuthorId =null,
          AllPost = true,
          Tag = null,
          PostId = null
        };

        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedBlogPostResponseDto), 200)]
    public async Task<ActionResult<List<BlogQueryResponse>>> GetRecentPost(CancellationToken cancellationToken )
    {
        var request = new BlogQueryRequest()
        {
            PageParameters =null,
            AuthorId =null,
            AllPost = false,
            Tag = null,
            PostId = null
        };
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedBlogPostResponseDto), 200)]
    public async Task<ActionResult<List<BlogQueryResponse>>> GetPostByAuthor(long id, PageParameters pageParameters, CancellationToken cancellationToken )
    {
        var request = new BlogQueryRequest()
        {
            PageParameters = pageParameters,
            AuthorId = id,
            AllPost = false,
            Tag = null,
            PostId = null
        };
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    [Framework.Attributes.Authorize(UserRole.Author)]
    [ProducesResponseType(typeof(SuccessResponseDto<BlogPost>), 200)]
    public async Task<ActionResult<List<BlogQueryResponse>>> GetPostById(long id, CancellationToken cancellationToken )
    {
        var request = new BlogQueryRequest()
        {
            PageParameters = null,
            AuthorId = null,
            AllPost = false,
            Tag = null,
            PostId = id
        };
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedBlogPostResponseDto), 200)]
    public async Task<ActionResult<List<BlogQueryResponse>>> GetPostByTag(PageParameters pageParameters, string tag, CancellationToken cancellationToken )
    {
        var request = new BlogQueryRequest()
        {
            PageParameters = pageParameters,
            AuthorId = null,
            AllPost = false,
            Tag = tag,
            PostId = null
        };
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<BlogCommandResponse>> AddPost(BlogCommand blogCommand, CancellationToken cancellationToken)
    {
        var request = new BlogCommand()
        {
            Body = blogCommand.Body,
            Summary = blogCommand.Summary,
            Tags = blogCommand.Tags,
            Title = blogCommand.Title,
            CoverImage = blogCommand.CoverImage,
            CategoryName = blogCommand.CategoryName,
            CreateNew = true
        };
      return Ok( await _mediator.Send(request, cancellationToken));
    }


    [HttpPut]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> UpdatePost([FromForm] NewBlogPostDto updateBlogPost,
        long id)
    {
        var result = await _validator.ValidateAsync(updateBlogPost);

        if (!result.IsValid) return BadRequest(new UserInputErrorDto(result));

        var coverImagePath = await _blogService.UploadFile(updateBlogPost.CoverImage);
        var post = await _blogService.GetPost(id);

        if (post == null) return BadRequest(new UserInputErrorDto("No post with AuthorId"));

        post.Body = updateBlogPost.Body;
        post.Summary = updateBlogPost.Summary;
        post.Tags = updateBlogPost.Tags;
        post.Title = updateBlogPost.Title;
        post.CoverImagePath = coverImagePath;
        post.Updated = DateTime.UtcNow;

        await _blogService.UpdatePost(post);

        return Ok(new EmptySuccessResponseDto("Post Updated Successfully!"));
    }

    [HttpDelete]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> DeletePost(long id)
    {
        await _blogService.DeletePost(id);
        return Ok(new EmptySuccessResponseDto("Post Deleted!"));
    }
}
