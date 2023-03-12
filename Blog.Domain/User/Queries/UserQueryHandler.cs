using Blog.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Domain.User.Queries;

public class UserQueryHandler : IRequestHandler<UserQueryRequest, UserQueryResponse>
{
    private readonly DataContext _dataContext;

    public UserQueryHandler(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<UserQueryResponse> Handle(UserQueryRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return new UserQueryResponse(await _dataContext.Authors.Where(a => a.AuthorId == request.AuthorId)
                .Include(a => a.BlogPosts)
                .SingleOrDefaultAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"No author with Id {request.AuthorId} : {ex}");
            return null!;
        }
    }
}
