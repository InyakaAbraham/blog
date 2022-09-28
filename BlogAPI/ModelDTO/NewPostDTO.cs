using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Newtonsoft.Json;

namespace BlogAPI.ModelDTO;

public class NewPostDTO
{
    [JsonIgnore]
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
    public int CategoryId { get; set; }

  
    public DateTime Created { get; } = DateTime.UtcNow;
    public DateTime Updated { get; } = DateTime.UtcNow;
}