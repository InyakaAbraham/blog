using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Blog.Api.Dtos;

public class NewPostDto
{
    [JsonIgnore][Required] public int PostId { get; set; }

    [Required] public string Title { get; set; }

    [Required] public string Summary { get; set; }

    [Required] public string Body { get; set; }

    [Required] public string[]? Tags { get; set; }

    [Required] public int AuthorId { get; set; }

    [Required] public string CategoryName { get; set; }

    [JsonIgnore]public DateTime Created { get; init; }
    [JsonIgnore]public DateTime Updated { get; init; }
}