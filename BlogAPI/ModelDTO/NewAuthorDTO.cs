namespace BlogAPI.ModelDTO;

public class NewAuthorDTO
{
    public int Id {get; set;}
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}