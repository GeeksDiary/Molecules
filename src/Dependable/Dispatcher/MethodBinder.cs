using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dependable.Dispatcher
{
    public interface IMethodBinder
    {
        Task<JobResult> Run(object instance, Job job);
        void Poison(object instance, object state, JobContext jobContext);
    }

    public class MethodBinder : IMethodBinder
    {
        static readonly IEnumerable<Type> ValidReturnTypes = new[] {typeof (Task), typeof (Task<Activity>) };

        public async Task<JobResult> Run(object instance, Job job)
        {
            var type = instance.GetType();
            var runMethod = type.GetMethod(job.Method);

            if (runMethod == null || !ValidReturnTypes.Contains(runMethod.ReturnType))
            {
                throw new InvalidOperationException(
                    string.Format("Type {0} does not have a matching Run method.", type.FullName));
            }

            var result = runMethod.Invoke(instance, job.Arguments);
            if (result == null)
                return null;

            if (runMethod.ReturnType == typeof (Task))
            {
                await (Task) result;
                return new JobResult();
            }

            return new JobResult(await (Task<Activity>) result);            
        }

        public void Poison(object instance, object state, JobContext jobContext)
        {
            var poisonMethod = instance.GetType().GetMethod("Poison");

            object[] arguments;
            if (poisonMethod == null || poisonMethod.ReturnType == typeof(void) ||
                !TryBindArguments(poisonMethod, state, jobContext, out arguments))
            {
                return;
            }

            poisonMethod.Invoke(instance, arguments);
        }

        static bool TryBindArguments(MethodInfo methodInfo, object state, JobContext context, out object[] arguments)
        {
            arguments = null;

            var parameters = methodInfo.GetParameters();
            if (parameters.Length > 2)
                return false;

            var tempArguments = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                if (parameter.ParameterType.IsInstanceOfType(state))
                    tempArguments[i] = state;
                else if (parameter.ParameterType == typeof (JobContext))
                    tempArguments[i] = context;
                else
                    return false;
            }

            arguments = tempArguments;
            return true;
        }
    }
}