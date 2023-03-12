using System.Text.Json.Serialization;

namespace Blog.Models;

public class RecentActivities
{
    [JsonIgnore] public int Id { get; set; }
    public string Message { get; set; }
    public DateTime Date { get; set; }
    public long AuthorId { get; set; }
}

//create a notification table, add notifications to the table to track recent activities of the current user, then display all notifications 
//of recent activities carried out by the user use database transactions to add the notification only when the activity is completed.
// Login, reset password, change password, change email, add post, update profile, update post, delete post.
