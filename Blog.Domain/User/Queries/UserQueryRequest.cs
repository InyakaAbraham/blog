using MediatR;

namespace Blog.Domain.User.Queries;

public class UserQueryRequest: IRequest<UserQueryResponse>
{
    public long AuthorId { get; set; }
}
