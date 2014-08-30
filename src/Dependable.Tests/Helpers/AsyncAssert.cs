using System;
using System.Threading.Tasks;
using Xunit;

namespace Dependable.Tests.Helpers
{
    public static class AsyncAssert
    {
        public static async Task<T> Throws<T>(Func<Task> operation) where T : Exception
        {
            Exception exception = null;
            try
            {
                await operation();
            }
            catch(T e)
            {
                exception = e;
            }

            Assert.NotNull(exception);
            Assert.IsType<T>(exception);

            return exception is T ? (T)exception : null;
        }
    }
}