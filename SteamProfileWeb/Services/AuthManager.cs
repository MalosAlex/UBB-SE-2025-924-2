using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using BusinessLayer.Models.Login;
using BusinessLayer.Models;
using BusinessLayer.Services;

namespace SteamProfileWeb.Services;

/// <summary>
/// Manages authentication operations by calling the external Auth API and setting local cookie state.
/// </summary>
public class AuthManager : IAuthManager
{
    private readonly HttpClient httpClient;
    private readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// Constructs the AuthManager with HTTP client factory and HTTP context accessor.
    /// </summary>
    public AuthManager(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        this.httpClient = httpClientFactory.CreateClient("AuthApi");
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
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

        // Create identity with the "SteamWebApi" cookie scheme
        var identity = new ClaimsIdentity(claims, "SteamWebApi");
        var principal = new ClaimsPrincipal(identity);
        await httpContextAccessor.HttpContext.SignInAsync("SteamWebApi", principal);
        return true;
    }

    /// <inheritdoc />
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
        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc />
    public async Task LogoutAsync() 
    { 
        // Sign out using the same cookie scheme
        await httpContextAccessor.HttpContext.SignOutAsync("SteamWebApi");
    }
}

