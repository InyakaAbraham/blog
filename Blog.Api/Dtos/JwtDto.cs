namespace Blog.Api.Dtos;

public class JwtDto:SuccessResponseDto<JwtDto.Credentials>
{
    public class Credentials
    {
        public string AccessToken { get; set; }
    }

    public JwtDto(Credentials data) : base(data)
    {
    }
}