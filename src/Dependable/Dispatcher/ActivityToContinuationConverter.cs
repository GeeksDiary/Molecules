using System;
using System.Collections.Generic;
using System.Linq;

namespace Dependable.Dispatcher
{
    public interface IActivityToContinuationConverter
    {
        ConvertedActivity Convert(Activity activity, Job parent);
    }

    public class ActivityToContinuationConverter : IActivityToContinuationConverter
    {
        readonly Func<DateTime> _now;

        public ActivityToContinuationConverter(Func<DateTime> now)
        {
            if (now == null) throw new ArgumentNullException("now");
            _now = now;
        }

        public ConvertedActivity Convert(Activity activity, Job parent)
        {
            var root = activity.Root();
            return ConvertCore(root, parent, parent.ExceptionFilters);
        }

        ConvertedActivity ConvertCore(
            Activity activity, 
            Job parent, 
            ExceptionFilter[] inheritedExceptionFilters)
        {
            if (activity == null)
                return new ConvertedActivity(null, Enumerable.Empty<Job>());

            var singleAwait = activity as SingleActivity;

            return singleAwait != null
                ? ConvertSingleActivityToContinuation(singleAwait, parent, inheritedExceptionFilters)
                : ConvertActivityGroupToContinuation((ActivityGroup) activity, parent, inheritedExceptionFilters);
        }

        ConvertedActivity ConvertSingleActivityToContinuation(
            SingleActivity singleActivity,
            Job parent,
            ExceptionFilter[] inheritedExceptionFilters)
        {
            var job = new Job(Guid.NewGuid(),
                singleActivity.Type,
                singleActivity.Name,
                singleActivity.Arguments,
                _now(),
                rootId: parent.RootId,
                parentId: parent.Id,
                status: JobStatus.Created,
                exceptionFilters: inheritedExceptionFilters.Concat(singleActivity.ExceptionFilters).ToArray());

            var jobs = new List<Job>(new[] {job});

            var continuation = new Continuation
            {
                Id = job.Id,
                Type = ContinuationType.Single,
                ContinueAfterHandlingFailure = singleActivity.CanContinueAfterHandlingFailure
            };

            var onNext = ConvertCore(singleActivity.Next, parent, inheritedExceptionFilters);
            continuation.Next = onNext.Continuation;
            jobs.AddRange(onNext.Jobs);

            var onFailed = ConvertCore(singleActivity.OnFailed, parent, inheritedExceptionFilters);
            continuation.OnAllFailed = onFailed.Continuation;
            jobs.AddRange(onFailed.Jobs);

            return new ConvertedActivity(continuation, jobs.AsEnumerable());
        }

        ConvertedActivity ConvertActivityGroupToContinuation(
            ActivityGroup activityGroup, 
            Job parent,
            IEnumerable<ExceptionFilter> inheritedExceptionFilters)
        {
            var jobs = Enumerable.Empty<Job>();

            /*
             *  Activities in a group inherit exception filters defined 
             *  in the group as well as the ones inherited by the group.
             */
            var effectiveExceptionFilters = inheritedExceptionFilters.Concat(activityGroup.ExceptionFilters).ToArray();

            var convertedItems =
                activityGroup.Items.Select(item => ConvertCore(item, parent, effectiveExceptionFilters)).ToArray();

            jobs = convertedItems.Aggregate(jobs, (current, converted) => current.Concat(converted.Jobs));

            var continuation = new Continuation
            {
                Type = activityGroup.IsParallel ? ContinuationType.Parallel : ContinuationType.Sequence,
                ContinueAfterHandlingFailure = activityGroup.CanContinueAfterHandlingFailure,
                Children = convertedItems.Select(c => c.Continuation)
            };

            var onNext = ConvertCore(activityGroup.Next, parent, effectiveExceptionFilters);
            continuation.Next = onNext.Continuation;
            jobs = jobs.Concat(onNext.Jobs);

            var onAnyFailed = ConvertCore(activityGroup.OnAnyFailed, parent, effectiveExceptionFilters);
            continuation.OnAnyFailed = onAnyFailed.Continuation;
            jobs = jobs.Concat(onAnyFailed.Jobs);

            var onAllFailed = ConvertCore(activityGroup.OnAllFailed, parent, effectiveExceptionFilters);
            continuation.OnAllFailed = onAllFailed.Continuation;
            jobs = jobs.Concat(onAllFailed.Jobs);

            return new ConvertedActivity(continuation, jobs);
        }
    }

    public class ConvertedActivity
    {
        public ConvertedActivity(Continuation continuation, IEnumerable<Job> jobs)
        {
            if (jobs == null) throw new ArgumentNullException("jobs");

            Continuation = continuation;
            Jobs = jobs;
        }

        public Continuation Continuation { get; set; }

        public IEnumerable<Job> Jobs { get; set; }
    }
}