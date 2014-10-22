using System;

namespace Dependable.Extensions.Persistence.Sql
{
    public static class ConfigurationExtension
    {
        public static DependableConfiguration UseSqlPersistenceProvider(
            this DependableConfiguration configuration,
            string connectionStringName,
            string instanceName)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            
            if (string.IsNullOrWhiteSpace(connectionStringName))
                throw new ArgumentException("A valid connectionStringName is required.");

            if (string.IsNullOrWhiteSpace(instanceName))
                throw new ArgumentException("A valid instanceName is required.");

            return configuration.UsePersistenceProvider(
                new SqlPersistenceProvider(connectionStringName, instanceName));
        }
    }
}