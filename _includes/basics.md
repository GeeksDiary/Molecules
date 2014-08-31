## <a name="introduction" class="anchor"></a>Introduction
Running jobs in the background is a problem we come across more often than we would like as developers. It usually starts out as simple as running some code in a separate thread (e.g. ```Task.Run(() => {})```. Then we improve it a little bit by adding a try\catch block around it in case it throws an exception. Soon we are adding more sophistication do more things like retrying and tracking.

In some scenarios we have to run a series of jobs in background. A series is normally started by a single job which creates more jobs based on some logic and those jobs can create more jobs inturn and the story goes on. It may not be desirable or an overkill to re-run all jobs in the series in case of a failure, therefore we have to build state tracking and orchestration mechanics into the application.

Dependable is a .NET library aiming to solve this problem in a more general purpose fashion by providing

- An easy composable programming model to keep us focused on implementation of jobs
- Robust, scalable and extensible runtime 

## <a name="installation" class="anchor"></a>Installation
Core functionality of Dependable is available in a single module which has no dependecies. Simply install the nuget packge and we are set to go.
```sh
install-package dependable
```
## <a name="creating-a-job" class="anchor"></a>Creating a Job
Job is a unit of work that we would like perform in background. Sending an email, generating a PDF, scaling images and calling an external web service are some classic examples. Dependable uses a handful of conventions to recognise jobs. Simplest convention is a class with a method named ```Run``` and return type of ```Task```.

```csharp
public class SendEmail 
{
    public async Task Run()
    {        
    }
}
```

## <a name="creating-a-scheduler" class="anchor"></a>Creating a Scheduler
Mechanics of executing jobs is in Dependable's scheduler. We can use ```DependableConfiguration``` to create one with desired configuration options. Typically we would run this code during application start-up and store this instance at the appdomain level. Invoking ```Scheduler.Start``` will make scheduler wait for jobs and start processing them when they are available.

```csharp
var scheduler = new DependableConfiguration().CreateScheduler();
scheduler.Start();
```

## <a name="scheduling-jobs" class="anchor"></a>Scheduling Jobs
Once we have a job and a scheduler, we can tell Dependable to schedule an instance of that job type using ```Schedule()``` method. When we invoke this method, it queues a request to execute that job at some point later. In other words, calling thread of this method is not going to be blocked until the work is done because the intension is to run it in the background.

```csharp
scheduler.Schedule<SendEmail>();
```

## <a name="passing-arguments-to-a-job" class="anchor"></a>Passing Arguments to a Job
 Typically, jobs will require some state to perform it's designated task correctly. For example, our ```SendEmail``` job may require a recipient address, subject and the body. We can create a type to represent this structure and pass an instance of that type at the time we schedule the job.

```csharp
public class EmailMessage
{
    public string To { get; set; }
    public string Subject { get; set; }
    public string body { get; set; }
}

public class SendEmail
{
    public async Task Run(EmailMessage message) { }    
}

scheduler.Schedule<SendEmail>(new EmailMessage 
    { 
        To = "smith@dependable.org", 
        Subject = "Hello",
        Body = "world"
    });
```

## <a name="retry" class="anchor"></a>Retry
When a job fails, we can configure Dependable to retry it. Retry configuration takes place in two places. Firsly we should tell depdendable how many times the job should be retried and interval used calculate the due time (i.e. the lag between the failure and next execution of ```Run``` method). We can configure these values globally for all jobs or for specific ones or both.

```csharp
// Setting retry settings globally
var scheduler = new DependableConfiguration()
                    .SetDefaultRetryCount(1)
                    .SetDefaultRetryDelay(TimeSpan.FromSeconds(5))
                    .CreateScheduler();

// Setting retry settings for a specific job
var scheduler = new DependableConfiguration()
                    .Job<BackgroundJob>(c => c
                        .WithRetryCount(1)
                        .WithRetryDelay(TimeSpan.FromSeconds(5))                        
                    )
                    .CreateScheduler();
```


The other piece in retry process is what we call Retry Timer. It periodically scans for due items in Dependable internal structures and executes the ready ones. By default this scanning process takes place once every minute. However, we can change that in the configuration.

```csharp
var scheduler = new DependableConfiguration()
                    .SetRetryTimerInterval(TimeSpan.FromSeconds(5))
                    .CreateScheduler();
```

### Detecting retry attempts
Sometimes it's essential for a job to know if it has been executed for the first time or not. This is usually the case we want to make sure executing a job once or many times yields the same result (a.k.a idempotent functions). Our ```Run``` method can receive an argument of type ```Dependable.JobContext``` which contains ```DispatchCount``` property to indicate how many times the job has been executed.

```csharp
public async Task Run(EmailMessage message, JobContext context)
{
    if(context.DispatchCount > 0)
    {   
        // do something special
    }
}    
```

## <a name="poison" class="anchor"></a>Poison
Sometimes no matter how many times we attempt to run a job, it could always end-up failing. This kind of behavior could be a result of a bug in job implementation or prolonged failure in infrastructure such as network connectivity. When maximum number of retry count is reached for a job, Dependable will consider it as a poisoned job. If we want to do something when a job becomes poisoned we could add a function called ```Poison``` which returns a ```bool``` to our job class. We will discuss the meaning of this return value in the next section but for the time being let's say we always return false.

Similar to ```Run``` method, ```Poison``` method can receive optional arguments. If we ask for ```JobContext```, we could use ```Exception``` property for the exception causing the job to poison.

```csharp
// Simple Poison function
public bool Poison() { return false; }

// Poison function with job state
public bool Poison(EmailMessage message) { return false; }

// Poison function with context
public bool Poison(JobContext context) { return false; }

// Poison function with both job state and context
public bool Poison(EmailMessage message, JobContext context) { return false; }
```
One small caveat we have to be aware of when using ```JobContext.Exception``` is that it always contains the exception occurred during last job execution and in rare cicumstances may not be available. Later we will explore Dependable's [tracking]({{ site.url }}tracking.html) feature that can reliably give you complete visibility to job's lifecycle.

## <a name="job-trees" class="anchor"></a>Job Trees
Dependable setup we've seen so far discussed simple jobs we want to run in the background. Truth is things are not always that simple. Sometimes background processing can involve running multiple jobs in a certain order. To make things worse, sometimes we want to compensate for the jobs we completed higher up in the workflow if something goes wrong down below. Well the good news is, you can now let Dependable handle the hairy coordination problem and focus on individual job implementations.

Our ```Run``` method's return type indicates what type of a job it is.

Return Type                       |     Meaning
-----------                             -------
```Task```                        |     Simple job to perform one task
```Task<Awaiter>```               |     Job that does something and waits for another job to finish.
```Task<IEnumerable<Awaiter>>```  |     Job that does something and waits for few other jobs to finish.


```csharp
// Wait for one job
public async Task<Awaiter> LongRunningJob()
{
    return Awaiter.For<AnotherLongRunningJob>();
}

// Wait for more than one job
public async Task<IEnumerable<Awaiter>> LongRunningJob()
{
    return new [] 
        { 
            Awaiter.For<ChildJobA>(), 
            Awaiter.For<ChildJobB>() 
        };  
}
```
One important thing to notice is that when a job returns an ```Task<IEnumerable<Awaiter>>``` those jobs are scheduled and executed parallely. 

Let's take a look at a more concrete example. Imagine we are building a travel booking system and we have the following Job setup to process a booking request.

```csharp
public class BookTrip
{
    public async Task<IEnumerable<Awaiter>> Run(BookingDetails details)
    {
        return new[] 
        {
            Awaiter.For<BookFlight>(details.Flight),
            Awaiter.For<BookHotel>(details.Hotel),
            Awaiter.For<BookCar>(details.Car)
        };
    }
}

public class BookFlight
{
    public async Task Run(Flight flight)
    {
    }
}

public class BookHotel
{
    public async Task Run(Hotel hotel)
    { 
    }
}

public class BookCar
{
    public async Task Run(Car car)
    {    
    }
}
```

## <a name="compensation" class="anchor"></a>Compensation
Now if any one of these bookings fail, we probably would want to cancel the successfully completed bookings (reality of this process is probably lot more involved than that, but let's assume this is the case for the purpose of this discussion).

To notify Dependable that successful jobs should be compensated, ```BookFlight```, ```BookHotel``` and ```BookCar``` jobs can implement the ```Poison``` method and return ```true```.

In addition to that, each one of those jobs will also have a new method called ```Compensate``` with logic for reversing the action it performed. Our modified jobs will look like this:

```csharp
public class BookTrip
{
    public async Task<IEnumerable<Awaiter>> Run(BookingDetails details)
    {
        return new[] 
        {
            Awaiter.For<BookFlight>(details.Flight),
            Awaiter.For<BookHotel>(details.Hotel),
            Awaiter.For<BookCar>(details.Car)
        };
    }
}

public class BookFlight
{
    public async Task Run(Flight flight)
    {
    }

    public book Poison() { return true; }

    public void Compensate(Flight flight)
    {
    }
}

public class BookHotel
{
    public async Task Run(Hotel hotel)
    { 
    }

    public book Poison() { return true; }

    public void Compensate(Hotel hotel)
    {
    }
}

public class BookCar
{
    public async Task Run(Car car)
    {    
    }

    public book Poison() { return true; }

    public void Compensate(Car car)
    {
    }
}
```

Current compensation model in Dependable leaves the decision making process to individual jobs. More sophisticated workflows may require that decision making process to be escalated to jobs in different levels in the tree. As with everything else, Dependable had to start small and it's goal is to provide wider range of options like this as it grows up.