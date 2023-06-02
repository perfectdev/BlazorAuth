using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAuth.Models;

[Table("UserSessions"), PrimaryKey(nameof(Id))]
public class UserSessionModel : BaseModel {
    [MaxLength(32)] public int Id { get; set; }
    [MaxLength(255)] public string RemoteAddress { get; set; } = string.Empty;
    [MaxLength(2048)] public string AuthToken { get; set; } = string.Empty;
    public UserModel? User { get; set; }
    [MaxLength(32), ForeignKey(nameof(User))] public string UserId { get; set; } = string.Empty;
    public long GeneratedTime { get; set; }
    public bool CheckExpired(int daysExpired) => Id.Equals(0) || DateTime.Now.AddDays(-daysExpired).ToInt() > GeneratedTime;
    public long LastUsingTime { get; set; }
}