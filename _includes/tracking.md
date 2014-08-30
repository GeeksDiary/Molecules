### Tracking
You don't want to kick off background work and hope for the best. Dependable provides a comprehensive view of your job execution with an extensible framework to get that information in a monitoring system of your like. We will look at these extensibility options in a later section and for now you can enable built-in console based monitoring in the configuration.

```csharp
var scheduler = new DependableConfiguration()
                    .UseConsoleEventLogger()
                    .CreateScheduler();                    
``` 

Sometimes events emitted by Dependable could be too verbose and you may be only interested in certain kind of events. You can configure ```ConsoleEventLogger``` to log just the events you are intersted in.

```csharp
var scheduler = new DependableConfiguration()
                    .UseConsoleEventLogger(EventType.JobStatusChanged | EventType.Exception)
                    .CreateScheduler();                    
``` 