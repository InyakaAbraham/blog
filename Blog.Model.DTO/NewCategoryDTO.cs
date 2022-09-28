using System.Text.Json.Serialization;

namespace Blog.Model.DTO;

public class NewCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    [JsonIgnore]
    public int PostId { get; set; }
}