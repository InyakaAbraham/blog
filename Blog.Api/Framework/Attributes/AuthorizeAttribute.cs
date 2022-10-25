using Blog.Api.Dtos;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Blog.Api.Framework.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly List<UserRole> _roles;

    public AuthorizeAttribute(UserRole firstRole, params UserRole[] otherRoles)
    {
        _roles = new List<UserRole>();
        _roles.Add(firstRole);
        _roles.AddRange(otherRoles);
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();

        if (allowAnonymous) return;

        var rawUserId = context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "sub");

        if (rawUserId is null)
            context.Result = new JsonResult(new UserNotAuthenticatedResponseDto())
                { StatusCode = StatusCodes.Status401Unauthorized };

        var parsedRoles = context.HttpContext.User.Claims
            .Where(x => x.Type == "role")
            .Select(y => Enum.Parse<UserRole>(y.Value))
            .ToList();


        foreach (var role in _roles)
            if (!parsedRoles.Contains(role))
            {
                context.Result = new JsonResult(new UserNotAuthorizedResponseDto())
                    { StatusCode = StatusCodes.Status403Forbidden };
                break;
            }
    }
}