# This kind of expression is not supported
Methods used to stitch activities together such as Activity.Run<T>() and Activity.Then<T>() sometimes throw this exception if it cannot understand the expression you provided. 

When this method is invoked, it evaluates the given expression to extract the arguments to be passed into the activity. This evaluation routine only supports small subset of expressions available in C#. To remedy the situation you could assign the output of problematic expression to a local variable and use that instead.

<div class="example-caption">
    This will throw an exception
</div>
```csharp
public void StartWorkflow(string name)
{
    Action.Run<A>(a => a.Run(string.Format("Hello {0}", message)));
}
```
<div class="example-caption">
    This will not
</div>
```csharp
public void StartWorkflow(string name)
{
    var message = string.Format("Hello {0}", name);
    Action.Run<A>(a => a.Run(message));
}
```