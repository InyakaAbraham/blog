using System.ComponentModel.DataAnnotations;

namespace Blog.Api.Blog.Model.Dto;

public class NewPostDto
{
    [Required] public int Id { get; set; }

    [Required] public string Title { get; set; }

    [Required] public string Summary { get; set; }

    [Required] public string Body { get; set; }

    [Required] public string[]? Tags { get; set; }

    [Required] public int AuthorId { get; set; }

    [Required] public string CategoryName { get; set; }

    public DateTime Created { get; init; } = DateTime.UtcNow;
    public DateTime Updated { get; init; } = DateTime.UtcNow;
}