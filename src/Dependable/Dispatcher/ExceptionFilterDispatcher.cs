using System;
using Dependable.Dependencies;
using Dependable.Utilities;

namespace Dependable.Dispatcher
{
    public interface IExceptionFilterDispatcher
    {
        void Dispatch(Job job, Exception exception, JobContext context, IDependencyScope scope);
    }

    public class ExceptionFilterDispatcher : IExceptionFilterDispatcher
    {
        public void Dispatch(Job job, Exception exception, JobContext context, IDependencyScope scope)
        {
            if (job == null) throw new ArgumentNullException("job");
            if (exception == null) throw new ArgumentNullException("exception");
            if (context == null) throw new ArgumentNullException("context");
            if (scope == null) throw new ArgumentNullException("scope");

            foreach (var filter in job.ExceptionFilters)
            {
                try
                {
                    DispatchAndHandleException(filter, exception, context, scope);
                }
                catch (Exception e)
                {
                    if (e.IsFatal())
                        throw;
                }
            }
        }

        void DispatchAndHandleException(ExceptionFilter filter, Exception exception, JobContext context, IDependencyScope scope)
        {
            var instance = scope.GetService(filter.Type);

            foreach (var argument in filter.Arguments)
            {
                var exceptionContext = argument as ExceptionContext;
                if (exceptionContext != null)
                {
                    exceptionContext.Exception = exception;
                    exceptionContext.DispatchCount = context.DispatchCount;
                }
            }

            var method = filter.Type.GetMethod(filter.Method);
            method.Invoke(instance, filter.Arguments);
        }
    }
}