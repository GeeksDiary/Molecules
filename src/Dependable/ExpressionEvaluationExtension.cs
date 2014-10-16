using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dependable
{
    public static class ExpressionEvaluationExtension
    {
        public static object Evaluate(this Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant)
                return ((ConstantExpression)expression).Value;

            var newObject = expression as NewExpression;
            if (newObject != null)
                return newObject.Constructor.Invoke(newObject.Arguments.Select(Evaluate).ToArray());

            var newArray = expression as NewArrayExpression;
            if (newArray != null)
            {
                var array = Array.CreateInstance(newArray.Type.GetElementType(), newArray.Expressions.Count);
                for (var i = 0; i < newArray.Expressions.Count; i++)
                    array.SetValue(newArray.Expressions[i].Evaluate(), i);

                return array;
            }

            var memberAccess = expression as MemberExpression;
            if (memberAccess != null)
            {
                var lhs = memberAccess.Expression.Evaluate();

                var field = memberAccess.Member as FieldInfo;
                if (field != null)
                    return field.GetValue(lhs);

                var property = memberAccess.Member as PropertyInfo;
                if (property != null)
                    return property.GetValue(lhs);
            }

            throw new InvalidOperationException(string.Format("This kind of expression is not supported - {0}. " + 
                "Try assigning it to a local variable first. " + 
                "For more information, visit - " + 
                "http://dependableproject.github.io/dependable/exceptions/expression-not-supported.html",
                expression));
        }
    }
}