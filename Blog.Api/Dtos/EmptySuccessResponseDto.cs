namespace Blog.Api.Dtos;

public class EmptySuccessResponseDto : SuccessResponseDto<dynamic>
{
    public EmptySuccessResponseDto(string message = "Success") : base(null, message)
    {
    }
}