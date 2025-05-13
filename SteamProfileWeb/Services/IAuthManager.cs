namespace SteamProfileWeb.Services;

public interface IAuthManager
{
    Task<bool> LoginAsync(string username, string password);
    Task LogoutAsync();
}