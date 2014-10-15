using System;
using Dependable.Dispatcher;
using Dependable.Persistence;
using Dependable.Recovery;
using Dependable.Tracking;
using NSubstitute;

namespace Dependable.Tests
{
    public class World
    {
        public World()
        {
            PersistenceStore = Substitute.For<IPersistenceStore>();
            EventStream = Substitute.For<IEventStream>();
            Router = Substitute.For<IJobRouter>();
            MethodBinder = Substitute.For<IMethodBinder>();
            RunningTransition = Substitute.For<IRunningTransition>();
            FailedTransition = Substitute.For<IFailedTransition>();
            EndTransition = Substitute.For<IEndTransition>();
            WaitingForChildrenTransition = Substitute.For<IWaitingForChildrenTransition>();
            PrimitiveStatusChanger = Substitute.For<IPrimitiveStatusChanger>();
            RecoverableAction = Substitute.For<IRecoverableAction>();
            StatusChanger = Substitute.For<IStatusChanger>();            
            Configuration = Substitute.For<IDependableConfiguration>();
            JobQueueFactory = Substitute.For<IJobQueueFactory>();
            ContinuationDispatcher = Substitute.For<IContinuationDispatcher>();
            ActivityToContinuationConverter = Substitute.For<IActivityToContinuationConverter>();
            Dispatcher = Substitute.For<IDispatcher>();

            Now = () => Fixture.Now;

            StubChange<EndTransition>();
            StubChange<WaitingForChildrenTransition>();
            StubChange<FailedTransition>();
            StubChange<RunningTransition>();
        }

        public IPersistenceStore PersistenceStore { get; set; }

        public IEventStream EventStream { get; set; }

        public IJobRouter Router { get; set; }

        public IMethodBinder MethodBinder { get; set; }

        public IRunningTransition RunningTransition { get; set; }

        public IFailedTransition FailedTransition { get; set; }

        public IEndTransition EndTransition { get; set; }

        public IWaitingForChildrenTransition WaitingForChildrenTransition { get; set; }

        public IPrimitiveStatusChanger PrimitiveStatusChanger { get; set; }

        public IRecoverableAction RecoverableAction { get; set; }
        
        public IStatusChanger StatusChanger { get; set; }

        public Func<DateTime> Now { get; set; }

        public JobManagementWrapper NewJob
        {
            get
            {
                return new JobManagementWrapper(new Job(Guid.NewGuid(),
                    typeof (string),
                    "Run",
                    new[] { new object() },                    
                    Fixture.Now), this);
            }
        }

        public IDependableConfiguration Configuration { get; set; }

        public IPersistenceProvider PersistenceProvider { get; set; }
        
        public IJobQueueFactory JobQueueFactory { get; set; }

        public IContinuationDispatcher ContinuationDispatcher { get; set; }

        public IActivityToContinuationConverter ActivityToContinuationConverter { get; set; }

        public IDispatcher Dispatcher { get; set; }

        void StubChange<TSource>()
        {
            PrimitiveStatusChanger.WhenForAnyArgs(c => c.Change<TSource>(null, Arg.Any<JobStatus>()))
                .Do(c => ((Job) c.Args()[0]).Status = (JobStatus) c.Args()[1]);
        }
    }
}