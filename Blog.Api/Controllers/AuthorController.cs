using Blog.Api.Dtos;
using Blog.Domain.User.Commands;
using Blog.Domain.User.Queries;
using Blog.Features;
using Blog.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[AllowAnonymous]
[ProducesResponseType(typeof(UserInputErrorDto), 400)]
public class AuthorController : AbstractController
{
    private readonly IMediator _mediator;
    private readonly IAuthenticationService _authenticationService;

    public AuthorController(
        IMediator mediator, IAuthenticationService authenticationService)
    {
        _mediator = mediator;
        _authenticationService = authenticationService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<UserCommandResponse>> Register(UserCommand userCommand, CancellationToken cancellationToken)
    {
        var request = new UserCommand
        {
            EmailAddress = userCommand.EmailAddress,
            FirstName = userCommand.FirstName,
            LastName = userCommand.LastName,
            Description = userCommand.Description,
            Password = userCommand.Password,
            Username = userCommand.Username,
            State = 1,
        };

        return Ok(await _mediator.Send(request, cancellationToken));
    }

    [HttpPatch]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<UserCommandResponse>> UpdateUser(UserCommand userCommand, CancellationToken cancellationToken)
    {
        var request = new UserCommand
        {
            FirstName = userCommand.FirstName,
            LastName = userCommand.LastName,
            Description = userCommand.Description,
            Username = userCommand.Username,
            State = 2,
        };

        return Ok(await _mediator.Send(request, cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<UserCommandResponse>> ForgotPassword(string emailAddress, CancellationToken cancellationToken)
    {
        var request = new UserCommand
        {
            EmailAddress = emailAddress,
            State = 3,
        };

        return Ok(await _mediator.Send(request, cancellationToken));
    }

    [HttpPatch]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> ResetPassword(string email, string token, string password, CancellationToken cancellationToken)
    {
        var request = new UserCommand
        {
            EmailAddress = email,
            Password = password,
            Token = token,
            State = 4,
        };

        return Ok(await _mediator.Send(request, cancellationToken));
    }

    [HttpPatch]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> ChangePassword(string email, string oldPassword, string newPassword, CancellationToken cancellationToken)
    {
        var request = new UserCommand
        {
            EmailAddress = email,
            Password = oldPassword,
            NewPassword = newPassword,
            State = 5,
        };

        return Ok(await _mediator.Send(request, cancellationToken));
    }

    [HttpPatch]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> ChangeEmailAddress(string oldEmailAddress, string password, CancellationToken cancellationToken)
    {
        var request = new UserCommand
        {
            EmailAddress = oldEmailAddress,
            Password = password,
            State = 6,
        };

        return Ok(await _mediator.Send(request, cancellationToken));
    }

    [HttpPatch]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> VerifyEmailChange(string oldEmailAddress, string newEmailAddress,
        string token, CancellationToken cancellationToken)
    {
        var request = new UserCommand
        {
            EmailAddress = newEmailAddress,
            OldEmailAddress = oldEmailAddress,
            Token = token,
            State = 7,
        };

        return Ok(await _mediator.Send(request, cancellationToken));
    }

    [HttpPatch]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<EmptySuccessResponseDto>> VerifyUser(string emailAddress, string token, CancellationToken cancellationToken)
    {
        var request = new UserCommand
        {
            EmailAddress = emailAddress,
            Token = token,
            State = 8,
        };

        return Ok(await _mediator.Send(request, cancellationToken));
    }


    [HttpPost]
    [ProducesResponseType(typeof(EmptySuccessResponseDto), 200)]
    public async Task<ActionResult<JwtDto>> Login(string email, string password, CancellationToken cancellationToken)
    {
        var request = new UserCommand
        {
            EmailAddress = email,
            Password = password,
            State = 9,
        };

        var response = await _mediator.Send(request, cancellationToken);

        return Ok(new JwtDto
        (new JwtDto.Credentials
        {
          AccessToken = await _authenticationService.CreateJwtToken(new Author()
          {
              AuthorId = response.AuthorId,
              Roles = response.Roles
          }),
        }));
    }

    [HttpGet]
    [ProducesResponseType(typeof(SuccessResponseDto<Author>), 200)]
    public async Task<ActionResult<UserCommandResponse>> GetAuthorById(long id, CancellationToken cancellationToken)
    {
        var request = new UserQueryRequest
        {
            AuthorId = id,
        };
        return Ok(await _mediator.Send(request, cancellationToken));
    }
}
