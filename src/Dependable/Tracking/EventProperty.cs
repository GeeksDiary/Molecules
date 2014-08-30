using System;
using System.Collections.Generic;

namespace Dependable.Tracking
{
    public static class EventProperty
    {
        public static readonly Func<Job, KeyValuePair<string, object>> JobSnapshot =
            v => new KeyValuePair<string, object>("JobSnapshot", new JobSnapshot(v));

        public static readonly Func<JobStatus, KeyValuePair<string, object>> FromStatus = BuildStatus("FromStatus");
        public static readonly Func<JobStatus, KeyValuePair<string, object>> ToStatus = BuildStatus("ToStatus");

        public static readonly Func<JobStatus, KeyValuePair<string, object>> PersistedStatus =
            BuildStatus("PersistedStatus");

        static Func<JobStatus, KeyValuePair<string, object>> BuildStatus(string name)
        {
            return v => new KeyValuePair<string, object>(name, v);
        }

        public static readonly Func<string, KeyValuePair<string, object>> ActivityName =
            v => new KeyValuePair<string, object>("Name", v);

        public static Func<string, object, KeyValuePair<string, object>> Named =
            (k, v) => new KeyValuePair<string, object>(k, v);
    }
}