namespace Blog.Api.Dtos;

public class JwtDto : SuccessResponseDto<JwtDto.Credentials>
{
    public JwtDto(Credentials data) : base(data)
    {
    }

    public class Credentials
    {
        public string AccessToken { get; set; }
    }
}