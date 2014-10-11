using System;
using System.Collections.Generic;

namespace Dependable
{
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

        public ActivityGroup AnyFailed(Activity next)
        {
            if (next == null) throw new ArgumentNullException("next");
            
            OnAnyFailed = next;
            return this;
        }
        public ActivityGroup AllFailed(Activity next)
        {
            if (next == null) throw new ArgumentNullException("next");

            OnAllFailed = next;
            return this;
        }
    }
}