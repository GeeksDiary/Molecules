using System;
using System.Threading.Tasks;

namespace Dependable.Utilities
{
    public static class TaskUtilities
    {
         public static Task FailFastOnException(this Task task) 
        { 
            task.ContinueWith(c => 
                Environment.FailFast(
                    "An unhandled exception occurred in a background task",  c.Exception), 
                TaskContinuationOptions.OnlyOnFaulted |  TaskContinuationOptions.ExecuteSynchronously); 
            return task; 
        }
    }
}