namespace Molecules.Core.Runtime
{
    public static class AtomChargerFactory
    {
        public static AtomCharger Create()
        {
            return new InvocationCharger();
        }
    }
}