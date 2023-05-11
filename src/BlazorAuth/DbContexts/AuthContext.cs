using BlazorAuth.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorAuth.DbContexts;

public class AuthContext : DbContext {
    public AuthContext(DbContextOptions<AuthContext> context) : base(context) {
        Database.EnsureCreated();
    }

    public DbSet<UserModel> Users { get; set; }
    public DbSet<RoleModel> Roles { get; set; }
    public DbSet<UserRolesModel> UserRoles { get; set; }
    public DbSet<UserSessionModel> UserSessions { get; set; }
}