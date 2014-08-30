using System;
using System.Linq;
using System.Linq.Expressions;
using Dependable.Dispatcher;

namespace Dependable
{
    public class ExceptionFilter
    {
        public Type Type { get; set; }

        public string Method { get; set; }

        public object[] Arguments { get; set; }

        public static ExceptionFilter From<T>(Expression<Action<ExceptionContext, T>> filter)
        {
            var method = filter.Body as MethodCallExpression;
            if (method == null)
                throw new ArgumentException("Only method call expressions are allowed.");

            return new ExceptionFilter
            {
                Method = method.Method.Name,
                Type = method.Method.DeclaringType,
                Arguments = method.Arguments.Select(a => a == filter.Parameters[0] ? new ExceptionContext() : a.Evaluate()).ToArray()
            };
        }
    }
}