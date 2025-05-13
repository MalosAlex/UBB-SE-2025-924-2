namespace SteamProfileWeb.Services;

public interface IAuthManager
{
    Task<bool> LoginAsync(string username, string password);
    Task<bool> RegisterAsync(string username, string email, string password, bool isDeveloper);
    Task LogoutAsync();
}