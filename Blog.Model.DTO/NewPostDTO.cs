using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Blog.Model.DTO;

public class NewPostDto
{
   
    [Required]
    public int Id {get; set;}

    [Required] public string Title { get; set; } = string.Empty;

    [Required] public string Summary { get; set; } = string.Empty;
    [Required] 
    public string Body { get; set; } = string.Empty;
    [Required]
    public string[]? Tags { get; set; }
    [Required]
    public int AuthorId { get; set; }

    [Required] 
    public string CategoryName { get; set; } = string.Empty;
    public DateTime Created { get; } = DateTime.UtcNow;
    public DateTime Updated { get; } = DateTime.UtcNow;
}