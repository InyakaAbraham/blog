namespace Blog.Models;

public class AppSettings
{
    public string JwtSecret { get; set; }
    public string PostgresDsn { get; set; }
    public string EmailFrom { get; set; }
    public string SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpUser { get; set; }
    public string SmtpPass { get; set; }
}