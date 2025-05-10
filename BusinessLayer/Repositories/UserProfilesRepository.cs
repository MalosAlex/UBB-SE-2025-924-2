using System;
using System.Data;
using BusinessLayer.Data;
using BusinessLayer.Models;
using Microsoft.Data.SqlClient;
using BusinessLayer.Repositories.Interfaces;
using BusinessLayer.Exceptions;
using BusinessLayer.DataContext;

namespace BusinessLayer.Repositories
{
    public class UserProfilesRepository : IUserProfilesRepository
    {
        private readonly ApplicationDbContext context;

        public UserProfilesRepository(ApplicationDbContext newContext)
        {
            this.context = newContext ?? throw new ArgumentNullException(nameof(context));
        }

        public UserProfile? GetUserProfileByUserId(int userId)
        {
            return context.UserProfiles.SingleOrDefault(up => up.UserId == userId);
        }

        public UserProfile? UpdateProfile(UserProfile profile)
        {
            var existing = context.UserProfiles.Find(profile.ProfileId)
                ?? throw new RepositoryException($"Profile with ID {profile.ProfileId} not found.");
            existing.ProfilePicture = profile.ProfilePicture;
            existing.Bio = profile.Bio;
            existing.LastModified = DateTime.UtcNow;
            context.SaveChanges();
            return existing;
        }

        public UserProfile? CreateProfile(int userId)
        {
            var profile = new UserProfile { UserId = userId };
            context.UserProfiles.Add(profile);
            context.SaveChanges();
            return profile;
        }

        public void UpdateProfileBio(int userId, string bio)
        {
            var existing = context.UserProfiles.SingleOrDefault(up => up.UserId == userId)
                ?? throw new RepositoryException($"Profile with user ID {userId} not found.");
            existing.Bio = bio;
            existing.LastModified = DateTime.UtcNow;
            context.SaveChanges();
        }

        public void UpdateProfilePicture(int userId, string picture)
        {
            var existing = context.UserProfiles.SingleOrDefault(up => up.UserId == userId)
                ?? throw new RepositoryException($"Profile with user ID {userId} not found.");
            existing.ProfilePicture = picture;
            existing.LastModified = DateTime.UtcNow;
            context.SaveChanges();
        }
    }
}
