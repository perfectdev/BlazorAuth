using Microsoft.EntityFrameworkCore;

namespace BlazorAuth.Models;

[PrimaryKey(nameof(Id))]
public class RoleModel : BaseModel {
    public string Id { get; set; } = Guid.NewGuid().ToString("N").ToUpper();
    public string Name { get; set; } = string.Empty;
    public override string ToString() => $"{Id}: {Name}";
}