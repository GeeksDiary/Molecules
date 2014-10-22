using System;
using System.Collections.Generic;
using System.Linq;
using Dependable.Dispatcher;
using NSubstitute;
using Xunit;

namespace Dependable.Tests
{
    public static class JobMutatorExtension
    {
        public static Queue<Mutation> Mutations(this IJobMutator jobMutator, Job job)
        {
            if (jobMutator == null) throw new ArgumentNullException("JobMutator");
            if (job == null) throw new ArgumentNullException("job");

            var calls = from call in jobMutator.ReceivedCalls()
                let arguments = call.GetArguments()
                where call.GetMethodInfo().Name == "Mutate" && ((Job) arguments[0]).Id == job.Id
                select new Mutation
                {
                    JobStatus = (JobStatus?) arguments[1],
                    DispatchCount = (int?) arguments[2],
                    RetryOn = (DateTime?) arguments[3],
                    Continuation = (Continuation) arguments[4],
                    Suspended = (bool?) arguments[5]
                };

            return new Queue<Mutation>(calls);
        }

        public static void Verify(this Queue<Mutation> mutations, params JobStatus[] statuses)
        {
            if (mutations == null) throw new ArgumentNullException("mutations");
            if (statuses == null) throw new ArgumentNullException("statuses");

            mutations.Verify(statuses.Select(s => new Mutation {JobStatus = s}).ToArray());
        }

        public static void Verify(this Queue<Mutation> mutations, params Mutation[] expectations)
        {
            if (mutations == null) throw new ArgumentNullException("mutations");
            if (expectations == null) throw new ArgumentNullException("expectations");

            foreach (var mutation in expectations)
            {
                var received = mutations.Dequeue();

                Assert.Equal(mutation.JobStatus, received.JobStatus);
                Assert.Equal(mutation.DispatchCount, received.DispatchCount);
                Assert.Equal(mutation.RetryOn, received.RetryOn);
                Assert.Equal(mutation.Continuation, received.Continuation);
                Assert.Equal(mutation.Suspended, received.Suspended);
            }

            Assert.Empty(mutations);
        }

        public static IJobMutator Stub<T>(this IJobMutator jobMutator, World world)
        {
            if (jobMutator == null) throw new ArgumentNullException("JobMutator");
            if (world == null) throw new ArgumentNullException("world");

            jobMutator.Mutate<T>(null).ReturnsForAnyArgs(c =>
            {
                var args = c.Args();
                var job = (Job) args[0];
                
                var newJob = new Job(job.Id, job.Type, job.Method, job.Arguments, job.CreatedOn, job.RootId,
                job.ParentId,
                job.CorrelationId, (JobStatus?)args[1] ?? job.Status, (int?)args[2] ?? job.DispatchCount,
                (DateTime?)args[3] ?? job.RetryOn, job.ExceptionFilters, (Continuation)args[4] ?? job.Continuation,
                (bool?)args[5] ?? job.Suspended);

                world.PersistenceStore.Load(job.Id).Returns(newJob);

                return newJob;
            });

            return jobMutator;
        }
    }
}