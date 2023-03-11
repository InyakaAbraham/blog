using System.Text.Json.Serialization;
using MediatR;

namespace Blog.Domain.User.Commands;

public class UserCommand : IRequest<UserCommandResponse>
{
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string? LastName { get; set; }
    public string Description { get; set; }
    public string? EmailAddress { get; set; }
    public string? Password { get; set; }
    [JsonIgnore] public string? OldEmailAddress { get; set; }
    [JsonIgnore] public string? NewPassword { get; set; }
    [JsonIgnore] public string? Token { get; set; }
    [JsonIgnore] public int? State { get; set; }
}
