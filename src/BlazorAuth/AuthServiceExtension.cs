namespace BlazorAuth;

public static class AuthServiceExtension {
    public static bool Roles(this IAuthService? authService, IEnumerable<string> roles) {
        return (bool)authService?.UserRoles.Any(t => roles.Contains(t.RoleId));
    }

    public static bool IsAdmin(this IAuthService? authService) {
        return Roles(authService, new[] { "admin" });
    }

    public static long ToInt(this DateTime dt) {
        return long.Parse(dt.ToString("yyyyMMddHHmmss"));
    }

    public static DateTime ToDateTime(this long i) {
        var s = $"{i}";
        return i == 0
            ? DateTime.MinValue
            : new DateTime(
                int.Parse(s[..4]),
                int.Parse(s.Substring(4, 2)),
                int.Parse(s.Substring(6, 2)),
                int.Parse(s.Substring(8, 2)),
                int.Parse(s.Substring(10, 2)),
                int.Parse(s.Substring(12, 2))
            );
    }
}