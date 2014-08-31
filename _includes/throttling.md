## <a name="throttling" class="anchor"></a>Throttling
Sometimes jobs we schedule could be resource intensive and we have limited amount of resources to spare. For example, number of CPU intensive jobs we can perform simultaniously is limited to the number of cores we have. As another example, number of concurrent calls we can make to to an external service might be limited by the agreement we have with the service provider.

When configuring jobs, we can specify the maximum number of job instances that Dependable can execute at point in time. 

```csharp
var scheduler = new DependableConfiguration()
                    .Job<LongRunningJob>(c => c.WithMaxWorkers(8))
                    .CreateScheduler();

```

Internally, Dependable has a queuing system to control the execution of jobs. One potential problem with applying a throttle is that if we receive work faster than we can process them, Dependable could exhaust our memory due to increasing queue length. To get around that problem, Dependable sets the max queue length to 1000 jobs by default. As soon as it reaches this limit it will start flushing jobs to the persistence system. You can control this queue length for specific jobs in the configuration.

```csharp
var scheduler = new DependableConfiguration()
                    .Job<LongRunningJob>(c => c.WithMaxQueueLength(2000))
                    .CreateScheduler();
```
