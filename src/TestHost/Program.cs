using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dependable;
using Dependable.Dispatcher;
using Dependable.Extensions.Persistence.Sql;
using Dependable.Tracking;

namespace TestHost
{
    class Program
    {
        static IScheduler _scheduler;

        static void Main()
        {
            DependableJobsTable.Create(ConfigurationManager.ConnectionStrings["Default"].ConnectionString);

            _scheduler = new DependableConfiguration()
                .SetDefaultRetryCount(1)
                .SetDefaultRetryDelay(TimeSpan.FromSeconds(1))
                .SetRetryTimerInterval(TimeSpan.FromSeconds(1))                
                .UseSqlPersistenceProvider("Default", "TestHost")
                .UseConsoleEventLogger(EventType.JobStatusChanged | EventType.Exception | EventType.Activity)
                .Activity<Greet>(c => c.WithMaxQueueLength(1).WithMaxWorkers(1))
                .CreateScheduler();

            _scheduler.Start();

            _scheduler.Schedule(Activity.Run<Greet>(g => g.Run("alice", "cooper")));
            _scheduler.Schedule(Activity.Run<Greet>(g => g.Run("bob", "jane")));
            _scheduler.Schedule(Activity.Run<Greet>(g => g.Run("kyle", "simpson")));
            _scheduler.Schedule(Activity.Run<Greet>(g => g.Run("andrew", "matthews")));

            var sequence = Activity
                .Sequence(
                    Activity.Run<Greet>(g => g.Run("a", "b")),
                    Activity.Run<Greet>(g => g.Run("c", "d")))
                .ExceptionFilter<LoggingFilter>((c, f) => f.Log(c, "hey"));

            sequence.AnyFailed(Activity.Run<Greet>(g => g.Run("e", "f")));

            _scheduler.Schedule(sequence);
            _scheduler.Schedule(
                Activity.Run<Greet>(g => g.Run("c", "d"))
                .ExceptionFilter<LoggingFilter>((c, f) => f.Log(c, "ouch"))
                .Failed<Greet>(g => g.Run("a", "b")));

            var person = new Person { FirstName = "Allen", LastName = "Jones" };
            _scheduler.Schedule(Activity.Run<GreetEx>(a => a.Run(person)));

            _scheduler.Schedule(Activity.Run<DueSchedule>(a => a.Run()));

            var parallel = Activity.Parallel(
                Activity.Run<Greet>(g => g.Run("a", "b")),
                Activity.Run<Greet>(g => g.Run("d", "e")));

            _scheduler.Schedule(parallel);

            _scheduler.Schedule(
                Activity
                    .Run<Greet>(g => g.Run("buddhike", "de silva"))
                    .ExceptionFilter<LoggingFilter>((c, f) => f.Log(c, "something was wrong")));

            _scheduler.Schedule(Activity.Run<GreetMany>(a => a.Run(new[] { "a", "b" })));

            var group = Activity
                .Parallel(
                Activity.Run<Greet>(a => a.Run("a", "b")),
                Activity.Run<Greet>(a => a.Run("a", "b")),
                Activity.Run<Greet>(a => a.Run("a", "b")))
                .ExceptionFilter<LoggingFilter>((c, f) => f.Log(c, "something went wrong"));

            _scheduler.Schedule(group.AnyFailed<Greet>(a => a.Run("a", "b")));

            _scheduler.Schedule(
                Activity.Run<GreetMany>(g => g.Run(new[] { "c" }))
                .ExceptionFilter<LoggingFilter>((c, f) => f.Log(c, "interesting")));

            for (var i = 0; i < 1000000; i++)
            {
                var nameA = "a" + i;
                var nameB = "b" + i;
                var activity = Activity.Run<GreetMany>(a => a.Run(new[] { nameA, nameB }));
                Task.Run(() => _scheduler.Schedule(activity));
            }
            Console.ReadLine();
        }
    }

    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class GreetEx
    {
        public async Task Run(Person person)
        {
            Console.WriteLine("Hello {0} {1}", person.FirstName, person.LastName);
        }
    }

    public class Greet
    {
// ReSharper disable once CSharpWarnings::CS1998
        public async Task Run(string firstName, string lastName)
        {
            Console.WriteLine("hello {0} {1}", firstName, lastName);
            Thread.Sleep(5000);

            if (firstName == "c")
                throw new Exception("la la la");
        }
    }

    public class GreetMany
    {
// ReSharper disable once CSharpWarnings::CS1998
        public async Task<Activity> Run(IEnumerable<string> names)
        {
            return Activity.Parallel(names.Select(n => Activity.Run<Greet>(g => g.Run(n, n))));
        }
    }

    public class DueSchedule
    {
        public async Task<Activity> Run()
        {
            return Activity.Sequence(Activity.Run<ApplicationList>(a => a.Download()),
                Activity.Run<ApplicationList>(a => a.DownloadEachItem()));
        }
    }

    public class ApplicationList
    {
// ReSharper disable once CSharpWarnings::CS1998
        public async Task Download()
        {
            Console.WriteLine("Download List");
        }

        public async Task<Activity> DownloadEachItem()
        {
            Console.WriteLine("Download Each Item");

            return Activity.Sequence(
                    Activity.Run<ApplicationDetails>(a => a.Download()),
                    Activity.Run<ApplicationDetails>(a => a.Convert()),
                    Activity.Run<ApplicationDetails>(a => a.Notify())
                );
        }
    }

    public class ApplicationDetails
    {
        public async Task Download()
        {
            Console.WriteLine("Download One");
        }

        public async Task Convert()
        {
            Console.WriteLine("Convert");            
        }

        public async Task Notify()
        {
            Console.WriteLine("Notify");            
        }
    }

    public class LoggingFilter
    {
        public void Log(ExceptionContext context, string message)
        {
            Console.WriteLine(context.ActivityType);
        }
    }
}