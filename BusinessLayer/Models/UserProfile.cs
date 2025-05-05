namespace BusinessLayer.Models
{
    public class UserProfile
    {
        public int ProfileId { get; set; }
        public int UserId { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }
        public DateTime LastModified { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
    public class Friend
    {
        private string profilePhotoPath = string.Empty;

        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string ProfilePhotoPath
        {
            get => string.IsNullOrEmpty(profilePhotoPath) ? "ms-appx:///Assets/default_avatar.png" : profilePhotoPath;
            set => profilePhotoPath = value;
        }
    }

    public class FriendRequest
    {
        private string profilePhotoPath = string.Empty;

        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string ProfilePhotoPath
        {
            get => string.IsNullOrEmpty(profilePhotoPath) ? "ms-appx:///Assets/default_avatar.png" : profilePhotoPath;
            set => profilePhotoPath = value;
        }

        public string ReceiverUsername { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; } = DateTime.Now;
    }
}