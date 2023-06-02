using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAuth.Models;

[Table("UserRoles"), PrimaryKey(nameof(Id))]
public class UserRolesModel : BaseModel {
    [MaxLength(32)] public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public UserModel? User { get; set; }
    [MaxLength(32), ForeignKey(nameof(User))] public string UserId { get; set; } = string.Empty;
    public RoleModel? Role { get; set; }
    [MaxLength(32), ForeignKey(nameof(Role))] public string RoleId { get; set; } = string.Empty;

    public override string ToString() => $"{Id}: (UID: {UserId} -> ROLE: {Role})";
}