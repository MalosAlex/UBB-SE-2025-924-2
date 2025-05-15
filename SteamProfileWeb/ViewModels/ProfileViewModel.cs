using BusinessLayer.Models;

namespace SteamProfileWeb.ViewModels
{
    public class ProfileViewModel
    {
        public int UserIdentifier { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string ProfilePhotoPath { get; set; }
        public string Biography { get; set; }
        public int FriendCount { get; set; }
        public decimal MoneyBalance { get; set; }
        public int PointsBalance { get; set; }
        public List<Collection> GameCollections { get; set; } = new();
        public string ErrorMessage { get; set; }
        public AchievementWithStatus FriendshipsAchievement { get; set; }
    }
}
