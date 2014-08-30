using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dependable.Dispatcher;

namespace Dependable
{
    public abstract class Activity
    {
        internal readonly IList<ExceptionFilter> ExceptionFiltersList = new List<ExceptionFilter>();

        public Activity Parent { get; internal set; }

        public Activity Next { get; internal set; }

        public bool CanContinue { get; internal set; }

        public ExceptionFilter[] ExceptionFilters
        {
            get { return ExceptionFiltersList.ToArray(); }
        }

        public Activity ThenContinue()
        {
            CanContinue = true;
            return this;
        }

        public static SingleActivity Run<T>(Expression<Func<T, Task>> func)
        {
            if (func == null) throw new ArgumentNullException("func");

            var methodCall = func.ToMethodCall();
            return new SingleActivity(typeof (T), methodCall.Name, methodCall.Arguments);
        }

        public static SingleActivity Run<T>(Expression<Func<T, Task<Activity>>> func)
        {
            if (func == null) throw new ArgumentNullException("func");

            var methodCall = func.ToMethodCall();
            return new SingleActivity(typeof(T), methodCall.Name, methodCall.Arguments);
        }

        public static ActivityGroup Parallel(params Activity[] items)
        {
            if (items == null) throw new ArgumentNullException("items");

            return Parallel(items.AsEnumerable());
        }

        public static ActivityGroup Parallel(IEnumerable<Activity> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            return new ActivityGroup(items, true);
        }

        public static ActivityGroup Sequence(params Activity[] items)
        {
            if (items == null) throw new ArgumentNullException("items");

            return Sequence(items.AsEnumerable());
        }


        public static ActivityGroup Sequence(IEnumerable<Activity> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            return new ActivityGroup(items, false);
        }
    }

    public class SingleActivity : Activity
    {
        internal SingleActivity(Type type, string name, object[] arguments)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (arguments == null) throw new ArgumentNullException("arguments");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A valid name is required.");

            Type = type;
            Name = name;
            Arguments = arguments;
        }

        public Type Type { get; private set; }

        public string Name { get; private set; }

        public object[] Arguments { get; private set; }

        public Activity OnFailed { get; private set; }

        public SingleActivity WhenFailed(Activity next)
        {
            if (next == null) throw new ArgumentNullException("next");
            OnFailed = next;
            return this;
        }
    }

    public class ActivityGroup : Activity
    {
        internal ActivityGroup(IEnumerable<Activity> items, bool isParallel)
        {
            if (items == null) throw new ArgumentNullException("items");

            Items = items;
            IsParallel = isParallel;
        }

        public IEnumerable<Activity> Items { get; private set; }

        public Activity OnAllFailed { get; private set; }

        public Activity OnAnyFailed { get; private set; }

        public bool IsParallel { get; private set; }

        public ActivityGroup WhenAnyFailed(Activity next)
        {
            if (next == null) throw new ArgumentNullException("next");
            
            OnAnyFailed = next;
            return this;
        }
        public ActivityGroup WhenAllFailed(Activity next)
        {
            if (next == null) throw new ArgumentNullException("next");

            OnAllFailed = next;
            return this;
        }
    }

    public static class ActivityExtensions
    {
        public static SingleActivity Then<T>(this Activity first, Expression<Func<T, Task>> func)
        {
            return ThenCore<T>(first, func);            
        }

        public static SingleActivity Then<T>(this Activity first, Expression<Func<T, Task<Activity>>> func)
        {
            return ThenCore<T>(first, func);
        }

        static SingleActivity ThenCore<T>(Activity first, LambdaExpression func)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (func == null) throw new ArgumentNullException("func");

            var method = func.ToMethodCall();
            var next = new SingleActivity(typeof(T), method.Name, method.Arguments) { Parent = first };
            first.Next = next;

            return next;
        }

        public static Activity Root(this Activity activity)
        {
            return activity.Parent == null ? activity : activity.Parent.Root();
        }

        public static SingleActivity WithExceptionFilter<TFilter>(this SingleActivity activity, 
            Expression<Action<ExceptionContext, TFilter>> filter)
        {
            if (activity == null) throw new ArgumentNullException("activity");
            if (filter == null) throw new ArgumentNullException("filter");

            activity.ExceptionFiltersList.Add(ExceptionFilter.From(filter));
            return activity;
        }

        public static ActivityGroup WithExceptionFilter<TFilter>(this ActivityGroup activityGroup,
            Expression<Action<ExceptionContext, TFilter>> filter)
        {
            if (activityGroup == null) throw new ArgumentNullException("activityGroup");
            if (filter == null) throw new ArgumentNullException("filter");

            activityGroup.ExceptionFiltersList.Add(ExceptionFilter.From(filter));
            return activityGroup;
        }
    }
}