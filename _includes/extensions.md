## <a name="sql-server-persistence-extension" class="anchor"></a>Persistence
Dependable has a built-in volatile persistence store. This is great if you don't want to recover jobs after a system failure. If you want your jobs to resume even after an event like that you could use a extension to persist jobs to an external storage. Just like Tracking system, Persistence system in Dependable is also extensible. It's built with document based storage in mind. At the moment you can use SQL server based storage by using SQL server persistance extension.

### Installation via nuget

```sh
install-package dependable.extensions.persistence.sql
````

### Configuration
Once you add a reference to the package you can create the persistence table using the following helper method

```csharp
DependableJobsTable.Create("connectionstring");
```

After that change dependable configuration to use Sql persistence.

```csharp
var scheduler = new DependableConfiguration()
                    .UseSqlRepositoryProvider("ConnectionStringConfigurationName", "InstanceA")
                    .CreateScheduler();
```

Second argument to ```UseSqlRepositoryProvider``` has a special purpose. Dependable relies on the fact that each scheduler has it's own storage. This could introduce problems in environments where you have a shared database for multiple systems running background jobs. You can get around this problem by specifying a unique instance name.
