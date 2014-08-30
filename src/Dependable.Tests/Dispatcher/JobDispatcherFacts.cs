//using System;
//using System.Threading.Tasks;
//using NSubstitute;
//using Phoenix.Dependencies;
//using Phoenix.Diagnostics;
//using Phoenix.Dispatcher;
//using Phoenix.Formatting;
//using Phoenix.Tests.Helpers;
//using Xunit;

//namespace Phoenix.Tests.Phoenix.Dispatcher
//{
//    public class JobDispatcherFacts
//    {
//        readonly Job _jobDescription = new Job(typeof(string), "b", "c", Fixture.Now);

//        public JobDispatcherFacts()
//        {
//            _jobDescription.Running();
//        }

//        [Fact]
//        public async Task DeserializesTheMessageUsingMessageFormatter()
//        {
//            var f = new JobDispatcherFixture();

//            await f.Subject.Dispatch(_jobDescription);

//            f.MessageFormatter.Received(1).Deserialize(_jobDescription.Arguments);
//        }

//        [Fact]
//        public async Task AsksForTheCorrectHandlerType()
//        {
//            var f = new JobDispatcherFixture();

//            await f.Subject.Dispatch(_jobDescription);
//        }

//        [Fact]
//        public async Task CreatesANewDepenencyScope()
//        {
//            var f = new JobDispatcherFixture();

//            await f.Subject.Dispatch(_jobDescription);

//            f.DependencyResolver.Received(1).BeginScope();
//        }

//        [Fact]
//        public async Task InvokesHandlerCompletionOnSuccess()
//        {
//            var f = new JobDispatcherFixture();

//            await f.Subject.Dispatch(_jobDescription);

//            f.EndTransition.Received(1).Run(_jobDescription, f.Job);
//        }

//        [Fact]
//        public async Task DoesNotInvokeHandlerCompletionOnException()
//        {
//            var f = new JobDispatcherFixture();

//            var t = new ManuallyCompletedTask();
//            var exception = new Exception("doh");

//            t.QueueFail(exception);

//            f.Job.Run(null).ReturnsForAnyArgs(t.Task);

//            await f.Subject.Dispatch(_jobDescription);

//            f.EndTransition.DidNotReceiveWithAnyArgs().Run(_jobDescription, f.Job);
//            f.ErrorHandlingPolicy.Received(1).RetryOrPoison(_jobDescription, exception);
//        }

//        [Fact]
//        public async Task DoesNotInvokeHandlerCompletionBeforeHandlerCompletes()
//        {
//            var f = new JobDispatcherFixture();

//            var t = new ManuallyCompletedTask();

//            f.Job.Run(null).ReturnsForAnyArgs(t.Task);

//            var dt = f.Subject.Dispatch(_jobDescription);

//            f.EndTransition.DidNotReceiveWithAnyArgs().Run(_jobDescription, f.Job);

//            t.QueueComplete();

//            await dt;

//            f.EndTransition.Received(1).Run(_jobDescription, f.Job);
//        }

//        [Fact]
//        public async Task ThrowsDispatcherExceptionWhenTheresNoMatchingHandler()
//        {
//            var f = new JobDispatcherFixture();

//            await AsyncAssert.Throws<DispatcherException>(() => f.Subject.Dispatch(_jobDescription));            
//        }

//        [Fact]
//        public async Task ThrowsDispatcherExceptionWhenHandlerReturnsNull()
//        {
//            var f = new JobDispatcherFixture();

//            f.Job.Run(null).ReturnsForAnyArgs((Task)null);

//            await AsyncAssert.Throws<DispatcherException>(() => f.Subject.Dispatch(_jobDescription));
//        }
//    }

//    public class JobDispatcherFixture : IFixture<Dispatcher>
//    {
//        public IDependencyResolver DependencyResolver { get; private set; }

//        public IDependencyScope DependencyScope { get; set; }

//        public IMessageFormatter MessageFormatter { get; set; }

//        public IErrorHandlingPolicy ErrorHandlingPolicy { get; set; }

//        public ILogger Logger { get; set; }

//        public IJob Job { get; set; }

//        public ICoordinator Coordinator { get; set; }

//        public JobDispatcherFixture()

//        {
//            DependencyScope = Substitute.For<IDependencyScope>();
//            MessageFormatter = Substitute.For<IMessageFormatter>();
//            ErrorHandlingPolicy = Substitute.For<IErrorHandlingPolicy>();
//            Logger = Substitute.For<ILogger>();
//            Job = Substitute.For<IJob, IJobManagement>();

//            MessageFormatter.Deserialize(null).ReturnsForAnyArgs("message");
//            DependencyScope.GetService(typeof(IJob)).Returns(Job);
//            Job.Run(null).ReturnsForAnyArgs(Task.FromResult(new object()));
//        }

//        public Dispatcher Subject
//        {
//            get
//            {
//                DependencyResolver = Substitute.For<IDependencyResolver>();
//                DependencyResolver.BeginScope().Returns(DependencyScope);

//                return new Dispatcher();
//            }
//        }
//    }
//}