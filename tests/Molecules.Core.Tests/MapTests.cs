using System.Linq;
using Xunit;

namespace Molecules.Core.Tests
{
    public class MapTests
    {
        [Fact]
        public async void MapsSourceToDestination()
        {
            var r = await Atom.Func(() => new[] { 1, 2, 3 }
            .AsEnumerable())
            .Map(i => i.Input * 2)
            .Invoker()
            .Charge();

            Assert.Equal(new [] { 2, 4, 6}, r);
        }        
    }
}