using System.Security.Claims;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Blog.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly IList<Role> _roles;

    public AuthorizeAttribute(params Role[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();

        if (allowAnonymous) return;

        var parsedRoles = context.HttpContext.User.Claims
            .Where(x => x.Type == ClaimTypes.Role)
            .Select(y => Enum.Parse<Role>(y.Value))
            .ToList();


        foreach (var role in _roles)
            if (!parsedRoles.Contains(role))
            {
                context.Result = new JsonResult(new { message = "Missing Privileges" })
                    { StatusCode = StatusCodes.Status403Forbidden };
                break;
            }
    }
}