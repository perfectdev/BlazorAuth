using BlazorAuth.Models;
using System.Net;

namespace BlazorAuth;

public interface IAuthService {
    List<UserRolesModel> UserRoles { get; set; }
    List<RoleModel> Roles { get; set; }
    List<UserModel> Users { get; set; }
    UserModel User { get; set; }
    UserSessionModel UserSession { get; set; }
    bool IsAuthorized { get; }
    Task AutoLogon();
    Task<UserSessionModel?> Logon(UserModel user);
    Task<UserModel?> Register(UserModel user);
    Task Logoff();
    IPAddress? GetRemoteIpAddress();
    void LoadUsers();
    void LoadRoles();
    void SaveUser(UserModel user);
    void DeleteUser(UserModel user);
    List<UserRolesModel> GetUserRoles(UserModel user);
    List<UserRolesModel> CheckUserRole(UserModel user, RoleModel role);
    void BindUserRole(UserModel user, RoleModel role);
    void UnbindUserRole(UserModel user, RoleModel role);
}