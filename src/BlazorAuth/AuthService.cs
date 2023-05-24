using BlazorAuth.DbContexts;
using BlazorAuth.Models;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace BlazorAuth;
public class AuthService : IAuthService {
    private const string AuthTokenKey = "AuthToken";
    private int AuthTokenDaysExpired { get; }
    private AuthContext AuthContext { get; }
    private ILocalStorageService ClientStorageService { get; }
    private IHttpContextAccessor HttpContextAccessor { get; }
    public List<UserRolesModel> UserRoles { get; set; }
    public List<UserModel> Users { get; set; }
    public List<RoleModel> Roles { get; set; }
    public List<UserSessionModel> Sessions { get; set; }
    public UserModel User { get; set; }
    public UserSessionModel? UserSession { get; set; }

    public AuthService(ILocalStorageService clientStorageService,
                       HttpContextAccessor httpContextAccessor,
                       AuthContext authContext,
                       int authTokenDaysExpired) {
        ClientStorageService = clientStorageService;
        HttpContextAccessor = httpContextAccessor;
        AuthContext = authContext;
        AuthTokenDaysExpired = authTokenDaysExpired;
        UserRoles = new List<UserRolesModel>();
        Users = new List<UserModel>();
        Roles = new List<RoleModel>();
        Sessions = new List<UserSessionModel>();
        User = new UserModel();
        UserSession = new UserSessionModel();
        AutoLogon();
    }

    public async Task AutoLogon() {
        var authToken = await ClientStorageService.GetItemAsync<string>(AuthTokenKey);
        await ApplyAuthToken(authToken);
    }

    public IPAddress? GetRemoteIpAddress() {
        return HttpContextAccessor.HttpContext?.Connection.RemoteIpAddress;
    }

    private async Task ApplyAuthToken(string authToken) {
        var remoteAddress = GetRemoteIpAddress()?.ToString();

        if (!AuthContext.UserSessions.Any(t => t.AuthToken.Equals(authToken) && t.RemoteAddress.Equals(remoteAddress))) {
            await ClearAuth();
            return;
        }

        UserSession = AuthContext.UserSessions.First(t => t.AuthToken.Equals(authToken) && t.RemoteAddress.Equals(remoteAddress));
        if (UserSession.CheckExpired(AuthTokenDaysExpired)) {
            await ClearAuth();
            return;
        }

        if (!AuthContext.Users.Any(t => t.Id.Equals(UserSession.UserId))) {
            await ClearAuth();
            return;
        }

        User = AuthContext.Users.FirstOrDefault(t => t.Id.Equals(UserSession.UserId))!;
        UserRoles = AuthContext.UserRoles.Where(t => t.UserId.Equals(User.Id)).ToList();
        foreach (var userRole in UserRoles) {
            userRole.Role = AuthContext.Roles.FirstOrDefault(t => t.Id.Equals(userRole.RoleId));
        }

        UserSession.LastUsingTime = DateTime.Now.ToInt();
        await AuthContext.SaveChangesAsync();
    }

    private async Task ClearAuth() {
        User = new UserModel();
        UserRoles.Clear();
        UserSession = new UserSessionModel();
        await ClientStorageService.RemoveItemAsync(AuthTokenKey);
    }

    public string PasswordHash(string password) {
        return PasswordHasher.HashPasswordV3(password);
    }

    public bool ValidatePasswordHash(string password, string dbPassword) {
        return PasswordHasher.VerifyHashedPasswordV3(dbPassword, password);
    }

    public async Task<UserSessionModel?> Logon(UserModel user) {
        UserSession = null;
        var checkUser = AuthContext.Users.FirstOrDefault(t => t.Email.Equals(user.Email));
        var isValidUser = checkUser is not null && ValidatePasswordHash(user.PasswordHash, checkUser.PasswordHash);
        if (!isValidUser)
            return await Task.FromResult(UserSession);
        user.Id = checkUser!.Id;
        UserSession = CreateUserSession(user);
        return await Task.FromResult(UserSession);
    }

    private UserSessionModel CreateUserSession(UserModel user) {
        var userSession = new UserSessionModel {
            AuthToken = Guid.NewGuid().ToString("N"),
            RemoteAddress = GetRemoteIpAddress()!.ToString(),
            UserId = user.Id,
            GeneratedTime = DateTime.Now.ToInt(),
            LastUsingTime = DateTime.Now.ToInt(),
        };
        var checkTime = DateTime.Now.AddDays(-AuthTokenDaysExpired).ToInt();
        var oldestSessions = AuthContext.UserSessions.Where(t => t.GeneratedTime < checkTime).ToList();
        AuthContext.UserSessions.RemoveRange(oldestSessions);
        userSession = AuthContext.Add(userSession).Entity;
        AuthContext.SaveChanges();
        ClientStorageService.SetItemAsync(AuthTokenKey, userSession.AuthToken);
        return userSession;
    }

    public async Task<UserModel?> Register(UserModel user) {
        if (AuthContext.Users.Any(t => t.Email.Equals(user.Email)))
            return await Task.FromResult<UserModel?>(null);

        var regUser = new UserModel {
            Id = Guid.NewGuid().ToString("N"),
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PasswordHash = PasswordHasher.HashPasswordV3(user.PasswordHash)
        };
        AuthContext.Users.Add(regUser);
        await AuthContext.SaveChangesAsync();

        return await Task.FromResult(regUser);
    }

    public async Task Logoff() {
        if (!IsAuthorized)
            return;
        AuthContext.UserSessions.Remove(UserSession);
        await AuthContext.SaveChangesAsync();
        await ClearAuth();
    }

    public bool IsAuthorized => !string.IsNullOrWhiteSpace(UserSession?.UserId);

    public void LoadUsers() {
        Users = AuthContext.Users.ToList();
    }

    public void LoadSessions(bool enrichSession = false) {
        Sessions = AuthContext.UserSessions
            .Select(t =>
                new UserSessionModel {
                    Id = t.Id,
                    UserId = t.UserId,
                    RemoteAddress = t.RemoteAddress,
                    GeneratedTime = t.GeneratedTime,
                    LastUsingTime = t.LastUsingTime,
                }
            ).ToList();
        if (enrichSession) {
            LoadUsers();
            foreach (var session in Sessions) {
                session.User = Users.FirstOrDefault(t => t.Id.Equals(session.UserId));
            }
        }
    }

    public void SaveUser(UserModel user) {
        if (string.IsNullOrWhiteSpace(user.Id))
            return;
        if (AuthContext.Users.Any(t => t.Id.Equals(user.Id))) {
            AuthContext.Users.Update(user);
        } else {
            AuthContext.Users.Add(user);
        }

        AuthContext.SaveChanges();
    }

    public void DeleteUser(UserModel user) {
        var deletedUserRoles = AuthContext.UserRoles.Where(t => t.UserId.Equals(user.Id));
        AuthContext.UserRoles.RemoveRange(deletedUserRoles);
        AuthContext.Users.Remove(user);
        AuthContext.SaveChanges();
    }

    public void LoadRoles() {
        Roles = AuthContext.Roles.ToList();
    }

    public List<UserRolesModel> GetUserRoles(UserModel user) {
        var foundRoles = AuthContext.UserRoles.Where(t => t.UserId.Equals(user.Id)).ToList();
        foreach (var userRole in foundRoles) {
            userRole.Role = Roles.FirstOrDefault(t => t.Id.Equals(userRole.RoleId));
        }

        return foundRoles;
    }

    public List<UserRolesModel> CheckUserRole(UserModel user, RoleModel role) => AuthContext.UserRoles.Where(t => t.UserId.Equals(user.Id) && t.RoleId.Equals(role.Id)).ToList();

    public void BindUserRole(UserModel user, RoleModel role) {
        if (!AuthContext.Users.Any(t => t.Id.Equals(user.Id)) ||
            !AuthContext.Roles.Any(t => t.Id.Equals(role.Id)))
            return;
        var foundUserRoles = AuthContext.UserRoles.FirstOrDefault(t => t.UserId.Equals(user.Id) && t.RoleId.Equals(role.Id));
        if (foundUserRoles is not null)
            return;
        foundUserRoles = new UserRolesModel {
            Id = Guid.NewGuid().ToString("N"),
            UserId = user.Id,
            RoleId = role.Id
        };
        AuthContext.UserRoles.Add(foundUserRoles);
        AuthContext.SaveChanges();
    }

    public void UnbindUserRole(UserModel user, RoleModel role) {
        if (!AuthContext.Users.Any(t => t.Id.Equals(user.Id)) ||
            !AuthContext.Roles.Any(t => t.Id.Equals(role.Id)))
            return;
        var foundUserRoles = AuthContext.UserRoles.FirstOrDefault(t => t.UserId.Equals(user.Id) && t.RoleId.Equals(role.Id));
        if (foundUserRoles is null)
            return;
        AuthContext.UserRoles.Remove(foundUserRoles);
        AuthContext.SaveChanges();
    }
}
