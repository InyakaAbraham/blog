namespace Blog.Models;

public class Role
{
    public UserRole Id { get; set; }
    public List<Author> Authors { get; set; }
}