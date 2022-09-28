using System.Text.Json.Serialization;

namespace BlogAPI.ModelDTO;

public class NewCategoryDTO
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    [JsonIgnore]
    public int PostId { get; set; }
}