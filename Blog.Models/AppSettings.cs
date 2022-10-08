namespace Blog.Models;

public class AppSettings
{
    public string JwtSecret { get; set; }
    public string PostgresDsn { get; set; }
}