using Dependable.Dispatcher;
using NSubstitute;
using Xunit;

namespace Dependable.Tests.Dispatcher
{
    public class JobMutatorFacts
    {
        readonly World _world = new World();

        [Fact]
        public void ShouldChangeStatus()
        {
            var job = _world.NewJob.In(JobStatus.Ready);

            var newJob = _world.NewJobMutation().Mutate<JobMutatorFacts>(job, JobStatus.Running);

            Assert.Equal(JobStatus.Running, newJob.Status);
        }

        [Fact]
        public void ShouldSaveTheJobWithNewStatus()
        {
            var job = _world.NewJob.In(JobStatus.Ready);
            var mutated = _world.NewJobMutation().Mutate<JobMutatorFacts>(job, JobStatus.Running);            
            _world.PersistenceStore.Received(1).Store(mutated);            
        }

        [Fact]
        public void ShouldReturnTheMutatedJob()
        {
            var job = _world.NewJob.In(JobStatus.Ready);

            var mutated = _world.NewJobMutation()
                .Mutate<JobMutatorFacts>(job, JobStatus.Running);

            Assert.NotEqual(job, mutated);
        }
    }

    public static partial class WorldExtensions
    {
        public static JobMutator NewJobMutation(this World world)
        {
            return new JobMutator(world.EventStream, world.PersistenceStore);
        }
    }
}