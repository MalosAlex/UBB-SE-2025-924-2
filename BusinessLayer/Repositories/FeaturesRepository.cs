using System.Data;
using BusinessLayer.Data;
using BusinessLayer.Models;
using Microsoft.Data.SqlClient;
using BusinessLayer.Exceptions;
using BusinessLayer.Repositories.Interfaces;
using BusinessLayer.DataContext;

namespace BusinessLayer.Repositories
{
    public class FeaturesRepository : IFeaturesRepository
    {
        private readonly ApplicationDbContext context;

        public FeaturesRepository(ApplicationDbContext newContext)
        {
            context = newContext ?? throw new ArgumentNullException(nameof(newContext));
        }

        public List<Feature> GetAllFeatures(int userId)
        {
            return context.Features
                .Select(f => new Feature
                {
                    FeatureId = f.FeatureId,
                    Name = f.Name,
                    Value = f.Value,
                    Description = f.Description,
                    Type = f.Type,
                    Source = f.Source,
                    Equipped = context.FeatureUsers.Any(fu => fu.UserId == userId && fu.FeatureId == f.FeatureId && fu.Equipped)
                })
                .ToList();
        }

        public List<Feature> GetFeaturesByType(string type)
        {
            return context.Features
                .Where(f => f.Type == type)
                .OrderByDescending(f => f.Value)
                .ToList();
        }

        public List<Feature> GetUserFeatures(int userIdentifier)
        {
            return context.Features
                .GroupJoin(
                    context.FeatureUsers.Where(fu => fu.UserId == userIdentifier),
                    f => f.FeatureId,
                    fu => fu.FeatureId,
                    (f, featureUsers) => new { f, featureUsers })
                .SelectMany(
                    x => x.featureUsers.DefaultIfEmpty(),
                    (x, fu) => new Feature
                    {
                        FeatureId = x.f.FeatureId,
                        Name = x.f.Name,
                        Value = x.f.Value,
                        Description = x.f.Description,
                        Type = x.f.Type,
                        Source = x.f.Source,
                        Equipped = fu != null && fu.Equipped
                    })
                .OrderBy(f => f.Type)
                .ThenByDescending(f => f.Value)
                .ToList();
        }

        public bool EquipFeature(int userIdentifier, int featureIdentifier)
        {
            var featureUser = context.FeatureUsers
                .FirstOrDefault(fu => fu.UserId == userIdentifier && fu.FeatureId == featureIdentifier);
            if (featureUser == null)
            {
                return false; // Feature not purchased
            }
            featureUser.Equipped = true;
            context.SaveChanges();
            return true;
        }

        public bool UnequipFeature(int userIdentifier, int featureIdentifier)
        {
            var featureUser = context.FeatureUsers
                                        .FirstOrDefault(fu => fu.UserId == userIdentifier && fu.FeatureId == featureIdentifier);
            if (featureUser == null)
            {
                return false; // Feature not purchased
            }
            featureUser.Equipped = false;
            context.SaveChanges();
            return true;
        }

        public bool UnequipFeaturesByType(int userIdentifier, string featureType)
        {
            var all = context.FeatureUsers
                                .Where(fu => fu.UserId == userIdentifier && fu.Feature.Type == featureType);
            foreach (var fu in all)
            {
                fu.Equipped = false;
            }
            context.SaveChanges();
            return true;
        }

        public bool IsFeaturePurchased(int userIdentifier, int featureIdentifier)
        {
            return context.FeatureUsers.Any(fu => fu.UserId == userIdentifier && fu.FeatureId == featureIdentifier);
        }

        public bool AddUserFeature(int userIdentifier, int featureIdentifier)
        {
            if (IsFeaturePurchased(userIdentifier, featureIdentifier))
            {
                return false;
            }
            context.FeatureUsers.Add(new FeatureUser
            {
                UserId = userIdentifier,
                FeatureId = featureIdentifier,
                Equipped = false
            });
            context.SaveChanges();
            return true;
        }
    }
}
