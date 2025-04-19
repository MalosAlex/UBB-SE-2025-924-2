using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BusinessLayer.Exceptions;
using BusinessLayer.Models;
using BusinessLayer.Repositories;
using BusinessLayer.Repositories.Interfaces;
using BusinessLayer.Services.Interfaces;
using BusinessLayer.Validators;

namespace BusinessLayer.Services
{
    public class FeaturesService : IFeaturesService
    {
        private readonly IFeaturesRepository featuresRepository;
        private readonly IUserService userService;

        public FeaturesService(IFeaturesRepository featuresRepository, IUserService userService)
        {
            this.featuresRepository = featuresRepository;
            this.userService = userService;
        }

        public IUserService UserService => userService;

        public Dictionary<string, List<Feature>> GetFeaturesByCategories()
        {
            var categories = new Dictionary<string, List<Feature>>();
            var allFeatures = featuresRepository.GetAllFeatures(userService.GetCurrentUser().UserId);

            foreach (var feature in allFeatures)
            {
                if (!categories.ContainsKey(feature.Type))
                {
                    categories[feature.Type] = new List<Feature>();
                }
                categories[feature.Type].Add(feature);
            }

            return categories;
        }

        public bool EquipFeature(int userId, int featureId)
        {
            if (!featuresRepository.IsFeaturePurchased(userId, featureId))
            {
                return false;
            }

            var feature = featuresRepository.GetFeaturesByType("frame").FirstOrDefault(feature => feature.FeatureId == featureId);
            if (feature != null)
            {
                featuresRepository.UnequipFeaturesByType(userId, "frame");
            }

            return featuresRepository.EquipFeature(userId, featureId);
        }

        public (bool, string) UnequipFeature(int userId, int featureId)
        {
            if (!featuresRepository.IsFeaturePurchased(userId, featureId))
            {
                return (false, "Feature not purchased");
            }

            bool success = featuresRepository.UnequipFeature(userId, featureId);
            return (success, success ? "Feature unequipped successfully" : "Failed to unequip feature");
        }

        public List<Feature> GetUserEquippedFeatures(int userId)
        {
            return featuresRepository.GetUserFeatures(userId)
                .Where(feature => feature.Equipped)
                .ToList();
        }

        public bool IsFeaturePurchased(int userId, int featureId)
        {
            return featuresRepository.IsFeaturePurchased(userId, featureId);
        }

        public (bool success, string message) PurchaseFeature(int userId, int featureId)
        {
            try
            {
                if (userId <= 0)
                {
                    return (false, "Invalid user ID.");
                }

                if (featureId <= 0)
                {
                    return (false, "Invalid feature ID.");
                }

                // Check if feature already purchased
                if (featuresRepository.IsFeaturePurchased(userId, featureId))
                {
                    return (false, "Feature is already purchased.");
                }

                // Get the feature to validate and check price
                var feature = featuresRepository.GetAllFeatures(userId)
                    .FirstOrDefault(currentFeature => currentFeature.FeatureId == featureId);

                if (feature == null)
                {
                    return (false, "Feature not found.");
                }

                var validationResult = FeaturesValidator.ValidateFeature(feature);
                if (!validationResult.isValid)
                {
                    return (false, validationResult.errorMessage);
                }

                var user = userService.GetUserByIdentifier(userId);
                if (user == null)
                {
                    return (false, "User not found.");
                }

                return (true, $"Successfully purchased {feature.Name} for {feature.Value} points.");
            }
            catch
            {
                return (false, "An error occurred while processing your purchase. Please try again later.");
            }
        }

        public (string profilePicturePath, string bioText, List<Feature> equippedFeatures) GetFeaturePreviewData(int userId, int featureId)
        {
            var equippedFeatures = GetUserEquippedFeatures(userId);

            string profilePicturePath = "ms-appx:///Assets/default-profile.png";
            string bioText = "No bio available";

            try
            {
                var userProfileRepo = new UserProfilesRepository(null);
                var userProfile = userProfileRepo.GetUserProfileByUserId(userId);

                if (userProfile != null)
                {
                    if (!string.IsNullOrEmpty(userProfile.ProfilePicture))
                    {
                        profilePicturePath = userProfile.ProfilePicture;
                        if (!profilePicturePath.StartsWith("ms-appx:///"))
                        {
                            profilePicturePath = $"ms-appx:///{profilePicturePath}";
                        }
                    }
                    if (!string.IsNullOrEmpty(userProfile.Bio))
                    {
                        bioText = userProfile.Bio;
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to get user equipped Features", exception);
            }
            return (profilePicturePath, bioText, equippedFeatures);
        }

        public List<Feature> GetUserFeatures(int userId)
        {
            try
            {
                var features = featuresRepository.GetUserFeatures(userId);
                foreach (var feature in features)
                {
                    var validation = FeaturesValidator.ValidateFeature(feature);
                    if (!validation.isValid)
                    {
                        throw new ValidationException(validation.errorMessage);
                    }
                }

                return features;
            }
            catch (DatabaseOperationException exception)
            {
                throw new DatabaseOperationException($"Failed to retrieve features for user {userId}.", exception);
            }
        }

        public FeaturesRepository GetRepository()
        {
            return featuresRepository as FeaturesRepository;
        }
    }
}
