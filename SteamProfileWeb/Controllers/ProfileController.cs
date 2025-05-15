using Microsoft.AspNetCore.Mvc;
using SteamProfileWeb.ViewModels;
using BusinessLayer.Services.Interfaces;
using BusinessLayer.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;

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

        public IActionResult Index(int userId)
        {
            var user = userService.GetUserByIdentifier(userId);
            if (user == null)
                return NotFound();

            var userProfile = userProfileRepository.GetUserProfileByUserId(userId);
            var collections = collectionsRepository.GetLastThreeCollectionsForUser(userId);
            var currentUserId = userService.GetCurrentUser().UserId;
            var isFriend = friendsService.AreUsersFriends(currentUserId, userId);

            // Equipped features
            var equippedFeatures = featuresService.GetUserEquippedFeatures(userId);
            string GetFeatureSource(string type) =>
                equippedFeatures.FirstOrDefault(feature => feature.Type.ToLower() == type && feature.Equipped)?.Source ?? "/images/default-profile.png";
            bool HasEquipped(string type) =>
                equippedFeatures.Any(feature => feature.Type.ToLower() == type && feature.Equipped);

            var vm = new ProfileViewModel
            {
                UserIdentifier = user.UserId,
                Username = user.Username,
                Email = user.Email,
                ProfilePhotoPath = userProfile?.ProfilePicture ?? "/images/default-profile.png",
                Biography = userProfile?.Bio ?? "",
                FriendCount = friendsService.GetFriendshipCount(userId),
                GameCollections = collections,
                IsFriend = isFriend,
                FriendButtonText = isFriend ? "Unfriend" : "Add Friend",
                FriendshipsAchievement = achievementsService.GetAchievementsWithStatusForUser(userId)
                    .FirstOrDefault(achievement => achievement.Achievement.AchievementType == "Friendships"),
                EquippedFrameSource = GetFeatureSource("frame"),
                EquippedHatSource = GetFeatureSource("hat"),
                EquippedPetSource = GetFeatureSource("pet"),
                EquippedEmojiSource = GetFeatureSource("emoji"),
                EquippedBackgroundSource = GetFeatureSource("background"),
                HasEquippedFrame = HasEquipped("frame"),
                HasEquippedHat = HasEquipped("hat"),
                HasEquippedPet = HasEquipped("pet"),
                HasEquippedEmoji = HasEquipped("emoji"),
                HasEquippedBackground = HasEquipped("background")
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult ToggleFriendship(int userId)
        {
            var currentUserId = userService.GetCurrentUser().UserId;
            var isFriend = friendsService.AreUsersFriends(currentUserId, userId);

            if (isFriend)
            {
                var friendshipId = friendsService.GetFriendshipIdentifier(currentUserId, userId);
                if (friendshipId.HasValue)
                    friendsService.RemoveFriend(friendshipId.Value);
            }
            else
            {
                friendsService.AddFriend(currentUserId, userId);
            }

            return RedirectToAction("Index", new { userId });
        }
    }
}
