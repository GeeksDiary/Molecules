using System.Linq;
using Xunit;

namespace Dependable.Core.Tests
{
    public class MapTests
    {
        [Fact]
        public async void MapsSourceToDestination()
        {
            var r = await Atom.Of(() => new[] { 1, 2, 3 }.AsEnumerable()).Map(i => i * 2).Charge();
            Assert.Equal(new [] { 2, 4, 6}, r);
        }        
    }
}