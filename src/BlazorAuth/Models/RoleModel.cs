using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorAuth.Models;

[Table("Roles"), PrimaryKey(nameof(Id))]
public class RoleModel : BaseModel {
    [MaxLength(32)] public string Id { get; set; } = Guid.NewGuid().ToString("N").ToUpper();
    [MaxLength(255)] public string Name { get; set; } = string.Empty;
    public override string ToString() => $"{Id}: {Name}";
}