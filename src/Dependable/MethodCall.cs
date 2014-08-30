using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dependable
{
    public class MethodCall
    {
        public string Name { get; set; }

        public object[] Arguments { get; set; }
    }

    public static class MethodCallExtensions
    {
        public static MethodCall ToMethodCall(this LambdaExpression lambda)
        {
            var method = lambda.Body as MethodCallExpression;
            if (method == null)
                throw new ArgumentException("Only method call expressions are allowed.");

            return new MethodCall
            {
                Name = method.Method.Name,
                Arguments = method.Arguments.Select(a => a.Evaluate()).ToArray()
            };
        }
    }
}