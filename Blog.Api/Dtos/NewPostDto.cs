using System.ComponentModel.DataAnnotations;

namespace Blog.Api.Dtos;

public class NewPostDto
{
    [Required] public int PostId { get; set; }

    [Required] public string Title { get; set; }

    [Required] public string Summary { get; set; }

    [Required] public string Body { get; set; }

    [Required] public string[]? Tags { get; set; }

    [Required] public int AuthorId { get; set; }

    [Required] public string CategoryName { get; set; }

    public DateTime Created { get; init; }
    public DateTime Updated { get; init; }
}