using System;
using System.Threading.Tasks;
using Dependable.Recovery;
using Xunit;

namespace Dependable.Tests.Recovery
{
    public class RecoverableActionFacts
    {
        readonly World _world = new World();

        [Fact]
        public void ShouldExecuteTheProvidedAction()
        {
            var a = 0;

            _world.NewRecoverableAction().Run(() => a = 1);
            
            Assert.Equal(1, a);
        }

        [Fact]
        public async Task ShouldRetryProvidedAction()
        {
            var a = 0;
            var tcs = new TaskCompletionSource<object>();
            var recoverableAction = _world.NewRecoverableAction();

            recoverableAction.Run(() =>
            {
                if (a++ == 0) throw new Exception("doh");
                tcs.SetResult(null);
            });

            recoverableAction.Monitor();
            await tcs.Task;

            Assert.Equal(2, a);
        }

        [Fact]
        public async Task ShouldRetryProvidedRetryAction()
        {
            var a = 0;
            var tcs = new TaskCompletionSource<object>();
            var recoverableAction = _world.NewRecoverableAction();

            recoverableAction.Run(() =>
            {
                if (a++ == 0) throw new Exception("doh");                
            }, 
                () =>
                {
                    a++;
                    tcs.SetResult(null);    
                });

            recoverableAction.Monitor();
            await tcs.Task;

            Assert.Equal(2, a);
        }
    }

    public static class WorldExtensions
    {
        public static IRecoverableAction NewRecoverableAction(this World world)
        {
            return new RecoverableAction(world.Configuration, world.EventStream);
        }
    }
}