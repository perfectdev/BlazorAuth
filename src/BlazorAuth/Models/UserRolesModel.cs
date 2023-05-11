using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAuth.Models;

[PrimaryKey(nameof(Id))]
public class UserRolesModel : BaseModel {
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UserId { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;
    [NotMapped] public RoleModel? Role { get; set; }

    public override string ToString() => $"{Id}: (UID: {UserId} -> ROLE: {Role})";
}