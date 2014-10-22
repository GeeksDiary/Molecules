using System;
using System.IO;
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
            StatusChanger = Substitute.For<IStatusChanger>();
            Configuration = Substitute.For<IDependableConfiguration>();
            JobQueueFactory = Substitute.For<IJobQueueFactory>();
            ContinuationDispatcher = Substitute.For<IContinuationDispatcher>();
            ActivityToContinuationConverter = Substitute.For<IActivityToContinuationConverter>();
            Dispatcher = Substitute.For<IDispatcher>();
            ContinuationLiveness = Substitute.For<IContinuationLiveness>();
            JobCoordinator = Substitute.For<IJobCoordinator>();

            Now = () => Fixture.Now;

            JobMutator = Substitute.For<IJobMutator>()
                .Stub<ContinuationDispatcher>(this)
                .Stub<EndTransition>(this)
                .Stub<FailedTransition>(this)
                .Stub<JobQueue>(this)
                .Stub<RunningTransition>(this)
                .Stub<StatusChanger>(this)
                .Stub<WaitingForChildrenTransition>(this)
                .Stub<Scheduler>(this);
            
            InitializeRecoverableAction();
        }

        void InitializeRecoverableAction()
        {
            RecoverableAction = Substitute.For<IRecoverableAction>();
            RecoverableAction.WhenForAnyArgs(a => a.Run(null)).Do(c =>
            {
                var args = c.Args();
                try
                {
                    ((Action) args[0])();
                }
                catch (Exception)
                {
                    var recoveryAction = args[1] ?? args[0];
                    ((Action) recoveryAction)();
                }

                if (args[2] != null)
                    ((Action) args[2])();
            });
        }

        public IPersistenceStore PersistenceStore { get; set; }

        public IEventStream EventStream { get; set; }

        public IJobRouter Router { get; set; }

        public IMethodBinder MethodBinder { get; set; }

        public IRunningTransition RunningTransition { get; set; }

        public IFailedTransition FailedTransition { get; set; }

        public IEndTransition EndTransition { get; set; }

        public IWaitingForChildrenTransition WaitingForChildrenTransition { get; set; }

        public IJobMutator JobMutator { get; set; }

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
                    new[] {new object()},
                    Fixture.Now), this);
            }
        }

        public IDependableConfiguration Configuration { get; set; }

        public IPersistenceProvider PersistenceProvider { get; set; }

        public IJobQueueFactory JobQueueFactory { get; set; }

        public IContinuationDispatcher ContinuationDispatcher { get; set; }

        public IActivityToContinuationConverter ActivityToContinuationConverter { get; set; }

        public IDispatcher Dispatcher { get; set; }

        public IContinuationLiveness ContinuationLiveness { get; set; }

        public IJobCoordinator JobCoordinator { get; set; }
    }
}