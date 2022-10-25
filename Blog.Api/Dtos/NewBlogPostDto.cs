using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Blog.Api.Dtos;

public class NewBlogPostDto
{
    [JsonIgnore]public IFormFile CoverImage { get; set; }
    [Required] public string Title { get; set; }
    [Required] public string Summary { get; set; }
    [Required] public string Body { get; set; }
    [Required] public string? Tags { get; set; }
    [Required] public string CategoryName { get; set; }
    [JsonIgnore] public DateTime Updated { get; init; }
}