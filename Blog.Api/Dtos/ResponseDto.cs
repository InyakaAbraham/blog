using Blog.Models;

namespace Blog.Api.Dtos;

public abstract class ResponseDto<TResultDto>
{
    protected ResponseDto(ResponseCode code, string message, TResultDto data)
    {
        Code = code;
        Message = message;
        Data = data;
    }

    public ResponseCode Code { get; set; }
    public string Message { get; set; }
    public TResultDto Data { get; set; }
}