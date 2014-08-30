namespace Dependable.Persistence
{
    public class InMemoryPersistenceProvider : IPersistenceProvider
    {
        readonly InMemoryPersistenceStore _store = new InMemoryPersistenceStore();
        public IPersistenceStore CreateStore()
        {
            return _store;
        }
    }
}