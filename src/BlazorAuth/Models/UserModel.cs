using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BlazorAuth.Models;

[PrimaryKey(nameof(Id))]
public class UserModel : BaseModel {
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    [Required] public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public override string ToString() => $"{FirstName} {LastName}";
}
