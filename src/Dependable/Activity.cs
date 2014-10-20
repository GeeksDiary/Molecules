using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dependable
{
    public abstract class Activity
    {
        internal readonly IList<ExceptionFilter> ExceptionFiltersList = new List<ExceptionFilter>();

        public Activity Parent { get; internal set; }

        public Activity Next { get; internal set; }

        public bool CanContinueAfterHandlingFailure { get; internal set; }

        public ExceptionFilter[] ExceptionFilters
        {
            get { return ExceptionFiltersList.ToArray(); }
        }

        public Activity ThenContinue()
        {
            CanContinueAfterHandlingFailure = true;
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

            return CreateGroup(items, true);
        }

        public static ActivityGroup Sequence(params Activity[] items)
        {
            if (items == null) throw new ArgumentNullException("items");

            return Sequence(items.AsEnumerable());
        }

        public static ActivityGroup Sequence(IEnumerable<Activity> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            return CreateGroup(items, false);
        }

        static ActivityGroup CreateGroup(IEnumerable<Activity> items, bool isParallel)
        {
            return new ActivityGroup(items.Select(i => i.Root()), isParallel);
        }
    }
}