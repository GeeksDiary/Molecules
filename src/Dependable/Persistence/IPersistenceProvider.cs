namespace Dependable.Persistence
{
    public interface IPersistenceProvider
    {
        IPersistenceStore CreateStore();
    }
}