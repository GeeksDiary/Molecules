using Molecules.Core.Depenencies;

namespace Molecules.Core.Runtime
{
    public class MoleculesHost
    {        
        public static Configuration Configuration { get; }

        static MoleculesHost()
        {
            Configuration = new Configuration();
            Configuration.UseProcessor(new InvocationProcessor());
            Configuration.UseDependencyResolver(new DefaultDependencyResolver());
        }
    }
}