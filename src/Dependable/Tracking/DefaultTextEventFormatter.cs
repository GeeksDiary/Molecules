using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dependable.Tracking
{
    public class DefaultTextEventFormatter
    {
        static readonly string[] WellKnownProperties = {"Source", "EventTime", "Name", "ThreadId"};

        static readonly Dictionary<EventType, Action<StringBuilder, Dictionary<string, object>>> Formatters =
            new Dictionary<EventType, Action<StringBuilder, Dictionary<string, object>>>
            {
                {EventType.Activity, Activity},
                {EventType.Exception, Exception},
                {EventType.JobStatusChanged, JobStatusChanged},
                {EventType.JobStatusChangeRejected, JobStatusChangeRejected},
                {EventType.JobAbandoned, JobAbandoned}
            };

        public string Format(EventType type, Dictionary<string, object> data)
        {
            var builder = new StringBuilder();

            AppendPart(builder, type.ToString().ToUpper());
            AppendPart(builder, ((Type) data["Source"]).Name);
            AppendPart(builder, ((DateTime) data["EventTime"]).ToString("hh:mm:ss"));

            if (Formatters.ContainsKey(type))
                Formatters[type](builder, data);

            builder.AppendLine();

            return builder.ToString();
        }

        static void Activity(StringBuilder builder, Dictionary<string, object> data)
        {
            AppendPart(builder, data["Name"]);
            foreach (var k in data.Keys.Except(WellKnownProperties))
                AppendPart(builder, k + ":" + data[k]);
        }

        static void JobStatusChanged(StringBuilder builder, Dictionary<string, object> data)
        {
            WriteJobSnapshot(builder, data);
        }

        static void JobAbandoned(StringBuilder builder, Dictionary<string, object> data)
        {
            AppendPart(builder, data["Reason"]);
            WriteJobSnapshot(builder, data);
        }

        static void WriteJobSnapshot(StringBuilder builder, Dictionary<string, object> data)
        {
            var snapshot = (JobSnapshot) data["JobSnapshot"];

            AppendPart(builder, snapshot.Type.FullName);
            AppendPart(builder, snapshot.Method);
            AppendPart(builder, snapshot.Id.ToString());
            AppendPart(builder, data["FromStatus"]);
            AppendPart(builder, data["ToStatus"]);
        }

        static void JobStatusChangeRejected(StringBuilder builder, Dictionary<string, object> data)
        {
            JobStatusChanged(builder, data);
            AppendPart(builder, data["PersistedStatus"]);
        }

        static void Exception(StringBuilder builder, Dictionary<string, object> data)
        {
            var exception = (Exception) data["Exception"];

            if (exception is TargetInvocationException)
                exception = exception.InnerException;

            AppendPart(builder, exception.TargetSite.DeclaringType);
            AppendPart(builder, exception.TargetSite);
            AppendPart(builder, exception.Message);
        }

        static void AppendPart(StringBuilder builder, object part)
        {
            builder.Append(part);
            builder.Append(" ");
        }
    }
}