using Microsoft.EntityFrameworkCore;

namespace BlazorAuth.Models;

[PrimaryKey(nameof(Id))]
public class UserSessionModel : BaseModel {
    public int Id { get; set; }
    public string RemoteAddress { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public long GeneratedTime { get; set; }
    public bool CheckExpired(int daysExpired) => Id.Equals(0) || DateTime.Now.AddDays(-daysExpired).ToInt() > GeneratedTime;
}