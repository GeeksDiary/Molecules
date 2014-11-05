using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dependable.Dispatcher;

namespace Dependable
{
    public static class ActivityExtensions
    {
        public static SingleActivity Then<T>(this Activity first, Expression<Func<T, Task>> func)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (func == null) throw new ArgumentNullException("func");

            return ThenCore<T>(first, func);
        }

        public static SingleActivity Then<T>(this Activity first, Expression<Func<T, Task<Activity>>> func)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (func == null) throw new ArgumentNullException("func");

            return ThenCore<T>(first, func);
        }

        public static Activity Then(this Activity first, Activity next)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            next.Parent = first;
            first.Next = next;

            return next;
        }

        static SingleActivity ThenCore<T>(Activity first, LambdaExpression func)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (func == null) throw new ArgumentNullException("func");

            var method = func.ToMethodCall();
            var next = new SingleActivity(typeof(T), method.Name, method.Arguments);

            return (SingleActivity)first.Then(next);
        }

        public static Activity Root(this Activity activity)
        {
            return activity.Parent == null ? activity : activity.Parent.Root();
        }

        public static SingleActivity Failed<T>(this SingleActivity activity,
            Expression<Func<T, Task>> func)
        {
            if (activity == null) throw new ArgumentNullException("activity");
            
            return activity.Failed(Activity.Run(func));
        }

        public static SingleActivity Failed<T>(this SingleActivity activity,
            Expression<Func<T, Task<Activity>>> func)
        {
            if (activity == null) throw new ArgumentNullException("activity");

            return activity.Failed(Activity.Run(func));
        }

        public static ActivityGroup AnyFailed<T>(this ActivityGroup activity,
            Expression<Func<T, Task>> func)
        {
            if (activity == null) throw new ArgumentNullException("activity");

            return activity.AnyFailed(Activity.Run(func));
        }

        public static ActivityGroup AnyFailed<T>(this ActivityGroup activity,
            Expression<Func<T, Task<Activity>>> func)
        {
            if (activity == null) throw new ArgumentNullException("activity");

            return activity.AnyFailed(Activity.Run(func));
        }

        public static ActivityGroup AllFailed<T>(this ActivityGroup activity,
            Expression<Func<T, Task>> func)
        {
            if (activity == null) throw new ArgumentNullException("activity");

            return activity.AllFailed(Activity.Run(func));
        }

        public static ActivityGroup AllFailed<T>(this ActivityGroup activity,
            Expression<Func<T, Task<Activity>>> func)
        {
            if (activity == null) throw new ArgumentNullException("activity");

            return activity.AllFailed(Activity.Run(func));
        }

        public static SingleActivity ExceptionFilter<TFilter>(this SingleActivity activity, 
            Expression<Action<ExceptionContext, TFilter>> filter)
        {
            if (activity == null) throw new ArgumentNullException("activity");
            if (filter == null) throw new ArgumentNullException("filter");

            activity.ExceptionFiltersList.Add(Dependable.ExceptionFilter.From(filter));
            return activity;
        }

        public static ActivityGroup ExceptionFilter<TFilter>(this ActivityGroup activityGroup,
            Expression<Action<ExceptionContext, TFilter>> filter)
        {
            if (activityGroup == null) throw new ArgumentNullException("activityGroup");
            if (filter == null) throw new ArgumentNullException("filter");

            activityGroup.ExceptionFiltersList.Add(Dependable.ExceptionFilter.From(filter));
            return activityGroup;
        }

        internal static bool IsEmptyGroup(this Activity activity)
        {
            if (activity == null) throw new ArgumentNullException("activity");
            var group = activity as ActivityGroup;

            return group != null && !group.Items.Any();
        }
    }
}