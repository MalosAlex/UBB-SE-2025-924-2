using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using BusinessLayer.Models.Login;

namespace SteamProfileWeb.Services;

public class AuthManager : IAuthManager
{
    private readonly HttpClient httpClient;
    private readonly IHttpContextAccessor httpContextAccessor;

    public AuthManager(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        httpClient = httpClientFactory.CreateClient("AuthApi");
        this.httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> LoginAsync(string emailOrUsername, string password)
    {
        var loginModel = new { EmailOrUsername = emailOrUsername, Password = password };
        var response = await httpClient.PostAsJsonAsync("Auth/Login", loginModel);

        if (!response.IsSuccessStatusCode)
            return false;

        var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (content == null || content.User == null || string.IsNullOrEmpty(content.Token))
            return false;

        var user = content.User;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("AccessToken", content.Token)
        };

        var identity = new ClaimsIdentity(claims, "SteamWebApi");
        var principal = new ClaimsPrincipal(identity);

        // Use consistent scheme name
        await httpContextAccessor.HttpContext.SignInAsync("SteamWebApi", principal);
        return true;
    }
    
    public async Task<bool> RegisterAsync(string username, string email, string password, bool isDeveloper)
    {
        var registerModel = new 
        {
            Username = username,
            Email = email,
            Password = password,
            IsDeveloper = isDeveloper
        };
        var response = await httpClient.PostAsJsonAsync("Auth/Register", registerModel);
        if (!response.IsSuccessStatusCode)
            return false;
        
        return true;
    }

    public async Task LogoutAsync()
    {
        // Use same scheme for logout
        await httpContextAccessor.HttpContext.SignOutAsync("SteamWebApi");
    }
}