using Dependable.Dispatcher;
using NSubstitute;
using Xunit;

namespace Dependable.Tests.Dispatcher
{
    public class PrimitiveStatusChangerFacts
    {
        readonly World _world = new World();

        [Fact]
        public void ShouldChangeStatus()
        {
            var job = _world.NewJob.In(JobStatus.Ready);

            _world.NewPrimitiveStatusChanger().Change<PrimitiveStatusChangerFacts>(job, JobStatus.Running);

            Assert.Equal(JobStatus.Running, job.Status);
        }

        [Fact]
        public void ShouldSaveTheJobWithNewStatus()
        {
            var job = _world.NewJob.In(JobStatus.Ready);

            var persistedStatus = JobStatus.Ready;
            _world.PersistenceStore.When(r => r.Store(job)).Do(c => persistedStatus = ((Dependable.Job)c.Args()[0]).Status);
            
            _world.NewPrimitiveStatusChanger().Change<PrimitiveStatusChangerFacts>(job, JobStatus.Running);
            
            _world.PersistenceStore.Received(1).Store(job);
            Assert.Equal(JobStatus.Running, persistedStatus);
        }

        [Fact]
        public void ShouldReturnTheOldStatus()
        {
            var job = _world.NewJob.In(JobStatus.Ready);

            var oldStatus = _world.NewPrimitiveStatusChanger()
                .Change<PrimitiveStatusChangerFacts>(job, JobStatus.Running);

            Assert.Equal(JobStatus.Ready, oldStatus);
        }
    }

    public static partial class WorldExtensions
    {
        public static PrimitiveStatusChanger NewPrimitiveStatusChanger(this World world)
        {
            return new PrimitiveStatusChanger(world.EventStream, world.PersistenceStore);
        }
    }
}