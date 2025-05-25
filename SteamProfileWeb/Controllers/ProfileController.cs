using Microsoft.AspNetCore.Mvc;
using SteamProfileWeb.ViewModels;
using BusinessLayer.Services.Interfaces;
using BusinessLayer.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BusinessLayer.Models;

namespace SteamProfileWeb.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService userService;
        private readonly IFriendsService friendsService;
        private readonly IUserProfilesRepository userProfileRepository;
        private readonly ICollectionsRepository collectionsRepository;
        private readonly IFeaturesService featuresService;
        private readonly IAchievementsService achievementsService;

        public ProfileController(
            IUserService userService,
            IFriendsService friendsService,
            IUserProfilesRepository userProfileRepository,
            ICollectionsRepository collectionsRepository,
            IFeaturesService featuresService,
            IAchievementsService achievementsService)
        {
            this.userService = userService;
            this.friendsService = friendsService;
            this.userProfileRepository = userProfileRepository;
            this.collectionsRepository = collectionsRepository;
            this.featuresService = featuresService;
            this.achievementsService = achievementsService;
        }

        public IActionResult Index()
        {
            string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = userService.GetUserByIdentifier(userId);
            if (user == null)
                return NotFound();

            var userProfile = userProfileRepository.GetUserProfileByUserId(userId);
            var collections = collectionsRepository.GetLastThreeCollectionsForUser(userId);

            var vm = new ProfileViewModel
            {
                UserIdentifier = user.UserId,
                Username = user.Username,
                Email = user.Email,
                ProfilePhotoPath = ConvertToWebPath(userProfile?.ProfilePicture),
                Biography = userProfile?.Bio ?? "",
                FriendCount = friendsService.GetFriendshipCount(userId),
                GameCollections = collections
            };

            return View(vm);
        }

        /// <summary>
        /// Converts ms-appx:// paths and other formats to web-compatible paths
        /// </summary>
        private string ConvertToWebPath(string profilePicturePath)
        {
            // If path is null or empty, return default
            if (string.IsNullOrEmpty(profilePicturePath))
            {
                return "/Assets/default_avatar.png";
            }

            // If it's already a web path (starts with /), return as-is
            if (profilePicturePath.StartsWith("/"))
            {
                return profilePicturePath;
            }

            // Convert ms-appx:// paths to web paths
            if (profilePicturePath.StartsWith("ms-appx:///"))
            {
                // Remove "ms-appx://" and convert to web path
                var webPath = profilePicturePath.Replace("ms-appx:///", "/");
                return webPath;
            }

            // If it's a relative path, make it absolute
            if (!profilePicturePath.StartsWith("http") && !profilePicturePath.StartsWith("/"))
            {
                return "/" + profilePicturePath;
            }

            // If it's an HTTP URL, return as-is
            if (profilePicturePath.StartsWith("http"))
            {
                return profilePicturePath;
            }

            // Fallback to default if we can't parse it
            return "/Assets/default_avatar.png";
        }
    }
}