namespace Molecules.Core.Runtime
{
    public class Configuration
    {
        public AtomProcessor Processor { get; private set; }

        public void UseProcessor(AtomProcessor processor)
        {
            Processor = processor;
        }
    }
}