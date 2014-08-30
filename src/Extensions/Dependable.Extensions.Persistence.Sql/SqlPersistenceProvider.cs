using System;
using Dependable.Persistence;

namespace Dependable.Extensions.Persistence.Sql
{
    public class SqlPersistenceProvider : IPersistenceProvider
    {
        readonly string _connectionStringName;
        readonly string _instanceName;

        public SqlPersistenceProvider(string connectionStringName, string instanceName)
        {
            if(string.IsNullOrWhiteSpace(connectionStringName))
                throw new ArgumentException("A valid connectionStringName is required.");

            if(string.IsNullOrWhiteSpace(instanceName))
                throw new ArgumentException("A valid instanceName is required.");

            _connectionStringName = connectionStringName;
            _instanceName = instanceName;
        }

        public IPersistenceStore CreateStore()
        {
            return new SqlPersistenceStore(_connectionStringName, _instanceName);
        }
    }

    public static class SqlRepositoryProviderExtensions
    {
        public static DependableConfiguration UseSqlPersistenceProvider(
            this DependableConfiguration configuration,
            string connectionStringName,
            string instanceName)
        {
            return
                configuration.UsePersistenceProvider(new SqlPersistenceProvider(connectionStringName, instanceName));
        }
    }
}