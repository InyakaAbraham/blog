using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlogAPI.Model;

public class Category
{
    [Key]
    [JsonIgnore]
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    [JsonIgnore]
    public List<BlogPost>? BlogPosts { get; set; }
}