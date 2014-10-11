## <a name="sql-server-persistence-extension" class="anchor"></a>Persistence
Dependable has a built-in volatile persistence store. This is great if we don't want to recover workflows after a system failure. If we want our workflows to resume even after an event like that we could use an extension to persist their state to an external storage. Just like Tracking system, Persistence system in Dependable is also extensible. It's built with document based storage in mind. At the moment we can use SQL server based storage by using SQL server persistance extension.

### Installation via nuget

```sh
install-package dependable.extensions.persistence.sql
````

### Configuration
Once we add a reference to the package we can create the persistence table using the following helper method

```csharp
DependableJobsTable.Create("connectionstring");
```

After that change dependable configuration to use Sql persistence.

```csharp
var scheduler = new DependableConfiguration()
                    .UseSqlRepositoryProvider("ConnectionStringConfigurationName", "InstanceA")
                    .CreateScheduler();
```

Second argument to UseSqlRepositoryProvider has a special purpose. Dependable relies on the fact that each scheduler has it's own storage. This could introduce problems in environments where you have a shared database for multiple systems running workflows. You can get around this problem by specifying a unique instance name.
