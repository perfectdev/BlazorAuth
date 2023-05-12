# BlazorAuth

Blazor Auth component work on browser local storage authentification token.

## Install BlazorAuth in your app

> Change the rendering mode of the Blazor components `render-mode="Server"` (in `_Host.cshtml`)

### Add new parameters to `appsettings.json`:
```JSON
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "AuthConnection": "Data Source=Data/auth.db;Pooling=True;"
  },
  "AuthTokenDaysExpired": 1
}
```
1)
```
ConnectionStrings > AuthConnection = 'Connection string for your database'
```
2)
```
AuthTokenDaysExpired = [Days before token expiration time]
```

### Implementing the service in your application (`Program.cs`).

Configure DataProtection service:
```C#
builder.Services.AddDataProtection().UseCryptographicAlgorithms(
    new AuthenticatedEncryptorConfiguration {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
    });
```
Add connection string for Auth-database:
```C#
var authConnectionString = builder.Configuration.GetConnectionString("AuthConnection") 
    ?? throw new InvalidOperationException("Connection string 'AuthConnection' not found.");
```
Extract AuthTokenDaysExpired parameter value from `appsettings.json`:
```C#
int.TryParse(builder.Configuration["AuthTokenDaysExpired"], out var authTokenDaysExpired);
```
Implementing the context of your database:
```C#
builder.Services.AddDbContext<AuthContext>(options => options.UseSqlite(authConnectionString));
```
Implementing the HttpContextAccessor:
```C#
builder.Services.AddHttpContextAccessor();
```
Implementing the BlazoredLocalStorage:
```C#
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<HttpContextAccessor>();
```
Integrate the ClientStorageService with your prefix (optional):
```C#
builder.Services.AddScoped<IClientStorageService, ClientStorageService>(x => 
    new ClientStorageService(x.GetRequiredService<ILocalStorageService>(), 
    "YourAppPrefix"));
```
Integrate the BlazorAuth authentication service with all dependencies into your application:
```C#
builder.Services.AddScoped<IAuthService, AuthService>(x => new AuthService(
                                                          x.GetRequiredService<ILocalStorageService>(),
                                                          x.GetRequiredService<HttpContextAccessor>(),
                                                          x.GetRequiredService<AuthContext>(),
                                                          authTokenDaysExpired));
```

## Use in Blazor components

Implement dependencies in the component:
```C#
@using BlazorAuth;

@inject IClientStorageService ClientStorage
@inject IAuthService Auth
```

Override the OnInitializedAsync method in your component:
```C#
protected override async Task OnInitializedAsync() {
    await Auth.AutoLogon();
    ...
    await base.OnInitializedAsync();
}
```
> `await Auth.AutoLogon()` checks for the presence of an open session on the client's IP and the presence of an active authentication token in the local browser storage (checks for expiration)

### Example `Logon` method call:
```C#
if (!Auth.IsAuthorized) {
    await Auth.Logon(new BlazorAuth.Models.UserModel {
        Email = "your email",
        PasswordHash = "raw password (not hash!)"
    });
    await Auth.AutoLogon();
}
```
### An example of the use of the Blazor component in the markup:
```C#
@using BlazorAuth;

@inject IClientStorageService ClientStorage
@inject IAuthService Auth

<div class="top-row ps-3 navbar navbar-dark">
    <div class="container-fluid">
        <a class="navbar-brand" href="">BlazorServerApp</a>
        <button title="Navigation menu" class="navbar-toggler" @onclick="ToggleNavMenu">
            <span class="navbar-toggler-icon"></span>
        </button>
    </div>
</div>

@if (Auth.IsAuthorized) {
    <div class="@NavMenuCssClass nav-scrollable" @onclick="ToggleNavMenu">
        <nav class="flex-column">
            <div class="nav-item px-3">
                <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
                    <span class="oi oi-home" aria-hidden="true"></span> Home
                </NavLink>
            </div>
            <div class="nav-item px-3">
                <NavLink class="nav-link" href="counter">
                    <span class="oi oi-plus" aria-hidden="true"></span> Counter
                </NavLink>
            </div>
            <div class="nav-item px-3">
                <NavLink class="nav-link" href="fetchdata">
                    <span class="oi oi-list-rich" aria-hidden="true"></span> Fetch data
                </NavLink>
            </div>
        </nav>
    </div>
}

@code {
    private bool collapseNavMenu = true;

    private string? NavMenuCssClass => collapseNavMenu ? "collapse" : null;

    private void ToggleNavMenu() {
        collapseNavMenu = !collapseNavMenu;
    }

    protected override async Task OnInitializedAsync() {
        await Auth.AutoLogon();
        await base.OnInitializedAsync();
    }
}
```