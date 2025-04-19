using System.Data;
using BusinessLayer.Data;
using BusinessLayer.Models;
using Microsoft.Data.SqlClient;
using BusinessLayer.Exceptions;
using BusinessLayer.Repositories.Interfaces;

namespace BusinessLayer.Repositories
{
    public class FeaturesRepository : IFeaturesRepository
    {
        private readonly IDataLink dataLink;

        private const string FeatureIdentifierString = "feature_id";
        private const string NameString = "name";
        private const string ValueString = "value";
        private const string DescriptionString = "description";
        private const string TypeString = "type";
        private const string SourceString = "source";

        public FeaturesRepository(IDataLink datalink)
        {
            dataLink = datalink ?? throw new ArgumentNullException(nameof(datalink));
        }

        public List<Feature> GetAllFeatures(int userId)
        {
            try
            {
                var features = new List<Feature>();
                var parameters = new SqlParameter[]
                {
            new SqlParameter("@userId", userId)
                };

                var dataTable = dataLink.ExecuteReader("GetAllFeatures", parameters);

                foreach (DataRow row in dataTable.Rows)
                {
                    features.Add(new Feature
                    {
                        FeatureId = Convert.ToInt32(row[FeatureIdentifierString]),
                        Name = row[NameString].ToString(),
                        Value = Convert.ToInt32(row[ValueString]),
                        Description = row[DescriptionString].ToString(),
                        Type = row[TypeString].ToString(),
                        Source = row[SourceString].ToString()
                    });
                }

                return features;
            }
            catch (DatabaseOperationException exception)
            {
                throw new DatabaseOperationException("Failed to retrieve features.", exception);
            }
        }

        public List<Feature> GetFeaturesByType(string type)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@type", type)
                };

                var features = new List<Feature>();
                var dataTable = dataLink.ExecuteReader("GetFeaturesByType", parameters);

                foreach (DataRow row in dataTable.Rows)
                {
                    features.Add(new Feature
                    {
                        FeatureId = Convert.ToInt32(row[FeatureIdentifierString]),
                        Name = row[NameString].ToString(),
                        Value = Convert.ToInt32(row[ValueString]),
                        Description = row[DescriptionString].ToString(),
                        Type = row[TypeString].ToString(),
                        Source = row[SourceString].ToString()
                    });
                }

                return features;
            }
            catch (DatabaseOperationException exception)
            {
                throw new DatabaseOperationException($"Failed to retrieve features of type {type}.", exception);
            }
        }

        public List<Feature> GetUserFeatures(int userIdentifier)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@userIdentifier", userIdentifier)
                };

                var features = new List<Feature>();
                var dataTable = dataLink.ExecuteReader("GetUserFeatures", parameters);

                foreach (DataRow row in dataTable.Rows)
                {
                    features.Add(new Feature
                    {
                        FeatureId = Convert.ToInt32(row[FeatureIdentifierString]),
                        Name = row[NameString].ToString(),
                        Value = Convert.ToInt32(row[ValueString]),
                        Description = row[DescriptionString].ToString(),
                        Type = row[TypeString].ToString(),
                        Source = row[SourceString].ToString()
                    });
                }

                return features;
            }
            catch (DatabaseOperationException exception)
            {
                throw new DatabaseOperationException($"Failed to retrieve features for user {userIdentifier}.", exception);
            }
        }

        public bool EquipFeature(int userIdentifier, int featureIdentifier)
        {
            try
            {
                // Check if the relationship exists
                var checkParameters = new SqlParameter[]
                {
                    new SqlParameter("@userIdentifier", userIdentifier),
                    new SqlParameter("@featureIdentifier", featureIdentifier)
                };

                var relationshipTable = dataLink.ExecuteReader("GetFeatureUserRelationship", checkParameters);

                if (relationshipTable.Rows.Count > 0)
                {
                    // Update existing relationship
                    var updateParameters = new SqlParameter[]
                    {
                        new SqlParameter("@userId", userIdentifier),
                        new SqlParameter("@featureId", featureIdentifier),
                        new SqlParameter("@equipped", 1)
                    };

                    dataLink.ExecuteNonQuery("UpdateFeatureUserEquipStatus", updateParameters);
                    return true;
                }

                return false; // Feature is not purchased
            }
            catch (DatabaseOperationException exception)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to equip feature {featureIdentifier} for user {userIdentifier}: {exception.Message}");
                return false;
            }
        }

        public bool UnequipFeature(int userIdentifier, int featureIdentifier)
        {
            try
            {
                // Check if the relationship exists
                var checkParameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", userIdentifier),
                    new SqlParameter("@featureId", featureIdentifier)
                };

                var relationshipTable = dataLink.ExecuteReader("GetFeatureUserRelationship", checkParameters);

                if (relationshipTable.Rows.Count > 0)
                {
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@userId", userIdentifier),
                        new SqlParameter("@featureId", featureIdentifier),
                        new SqlParameter("@equipped", 0)
                    };

                    dataLink.ExecuteNonQuery("UpdateFeatureUserEquipStatus", parameters);
                    return true;
                }

                return false; // Feature is not purchased
            }
            catch (DatabaseOperationException exception)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to unequip feature {featureIdentifier} for user {userIdentifier}: {exception.Message}");
                return false;
            }
        }

        public bool UnequipFeaturesByType(int userIdentifier, string featureType)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", userIdentifier),
                    new SqlParameter("@featureType", featureType)
                };

                dataLink.ExecuteNonQuery("UnequipFeaturesByType", parameters);
                return true;
            }
            catch (DatabaseOperationException)
            {
                return false;
            }
        }

        public bool IsFeaturePurchased(int userIdentifier, int featureIdentifier)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", userIdentifier),
                    new SqlParameter("@featureId", featureIdentifier)
                };

                var relationshipTable = dataLink.ExecuteReader("GetFeatureUserRelationship", parameters);
                return relationshipTable.Rows.Count > 0;
            }
            catch (DatabaseOperationException exception)
            {
                throw new DatabaseOperationException($"Failed to check feature purchase status.", exception);
            }
        }

        public bool AddUserFeature(int userIdentifier, int featureIdentifier)
        {
            try
            {
                // First check if the user already has this feature
                if (IsFeaturePurchased(userIdentifier, featureIdentifier))
                {
                    return false;
                }

                // Add the feature to the user's purchased features
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", userIdentifier),
                    new SqlParameter("@featureId", featureIdentifier),
                    new SqlParameter("@equipped", false) // New features are unequipped by default
                };

                dataLink.ExecuteNonQuery("AddUserFeature", parameters);
                return true;
            }
            catch (DatabaseOperationException)
            {
                return false;
            }
        }
    }
}
