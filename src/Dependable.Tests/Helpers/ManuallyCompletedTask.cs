using System;
using System.Threading.Tasks;

namespace Dependable.Tests.Helpers
{
    public class ManuallyCompletedTask
    {
        readonly TaskCompletionSource<object> _taskCompletionSource = new TaskCompletionSource<object>();

        public Task Task
        {
            get { return _taskCompletionSource.Task; }
        }

        public void Complete()
        {
            _taskCompletionSource.SetResult(new object());
        }

        public void Fail(Exception exception)
        {
            _taskCompletionSource.SetException(exception);
        }
    }
}