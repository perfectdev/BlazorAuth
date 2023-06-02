using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAuth.Models;

[Table("Users"), PrimaryKey(nameof(Id))]
public class UserModel : BaseModel {
    [MaxLength(32)] public string Id { get; set; } = Guid.NewGuid().ToString("N");
    [MaxLength(255)] public string FirstName { get; set; } = string.Empty;
    [MaxLength(255)] public string LastName { get; set; } = string.Empty;
    [Required, MaxLength(255)] public string Email { get; set; } = string.Empty;
    [MaxLength(255)] public string PasswordHash { get; set; } = string.Empty;

    public override string ToString() => $"{FirstName} {LastName}";
}
