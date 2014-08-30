using System;
using System.Collections.Generic;

namespace Dependable.Persistence
{
    public class DelegatingPersistenceStore : IPersistenceStore
    {
        readonly IPersistenceProvider _persistenceProvider;

        public DelegatingPersistenceStore(IPersistenceProvider persistenceProvider)
        {
            if (persistenceProvider == null) throw new ArgumentNullException("persistenceProvider");
            _persistenceProvider = persistenceProvider;
        }

        public Job Load(Guid id)
        {
            using (var persistenceStore = _persistenceProvider.CreateStore())
            {
                return persistenceStore.Load(id);
            }
        }

        public Job LoadBy(Guid correlationId)
        {
            using (var persistenceStore = _persistenceProvider.CreateStore())
            {
                return persistenceStore.LoadBy(correlationId);
            }
        }

        public IEnumerable<Job> LoadBy(JobStatus status)
        {
            using (var persistenceStore = _persistenceProvider.CreateStore())
            {
                return persistenceStore.LoadBy(status);
            }
        }

        public void Store(Job job)
        {
            using (var repository = _persistenceProvider.CreateStore())
            {
                repository.Store(job);
            }
        }

        public void Store(IEnumerable<Job> jobs)
        {
            using (var repository = _persistenceProvider.CreateStore())
            {
                repository.Store(jobs);
            }
        }

        public IEnumerable<Job> LoadSuspended(Type type, int max)
        {
            using (var persistenceStore = _persistenceProvider.CreateStore())
            {
                return persistenceStore.LoadSuspended(type, max);
            }
        }

        public int CountSuspended(Type type)
        {
            using (var persistenceStore = _persistenceProvider.CreateStore())
            {
                return persistenceStore.CountSuspended(type);
            }
        }

        public void Dispose()
        {
        }
    }
}