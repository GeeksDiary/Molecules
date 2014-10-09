using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Dependable.Dispatcher;
using Dependable.Persistence;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dependable.Extensions.Persistence.Sql
{
    public class SqlPersistenceStore : IPersistenceStore
    {
        readonly string _instanceName;

        static readonly JsonSerializerSettings DefaultSerializerSettings =
            new JsonSerializerSettings();

        readonly SqlConnection _connection;

        const string Columns =
            "Id, Type, Method, Arguments, CreatedOn, RootId, ParentId, CorrelationId, Status, " +
            "DispatchCount, RetryOn, Continuation, ExceptionFilters, Suspended, InstanceName";

        static SqlPersistenceStore()
        {
            DefaultSerializerSettings.Converters.Add(new StringEnumConverter());
        }

        public SqlPersistenceStore(string connectionStringName, string instanceName)
        {
            if (string.IsNullOrWhiteSpace(connectionStringName))
                throw new ArgumentException("A valid connectionStringName is required.");

            if (string.IsNullOrWhiteSpace(instanceName))
                throw new ArgumentException("A valid instanceName is required.");

            _instanceName = instanceName;

            _connection =
                new SqlConnection(ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString);

            _connection.Open();
        }

        public Job Load(Guid id)
        {
            return
                _connection.Query(
                    string.Format(
                        "select {0} from DependableJobs where Id = @Id and InstanceName = @InstanceName",
                        Columns),
                    new {Id = id, InstanceName = _instanceName}).Select(Deserialize).FirstOrDefault();
        }

        public Job LoadBy(Guid correlationId)
        {
            return
                _connection.Query(
                    string.Format("select {0} from DependableJobs where CorrelationId = @Id", Columns),
                    new {Id = correlationId, InstanceName = _instanceName})
                    .Select(Deserialize)
                    .FirstOrDefault();
        }

        public IEnumerable<Job> LoadBy(JobStatus status)
        {
            return
                _connection.Query(
                    string.Format(
                        "select {0} from DependableJobs " +
                        "where Status = @Status and Suspended = 0 and InstanceName = @InstanceName",
                        Columns),
                    new {Status = status.ToString(), InstanceName = _instanceName})
                    .Select(Deserialize)
                    .ToArray();
        }

        public void Store(Job job)
        {
            var existing = Load(job.Id);
            if (existing == null)
                Insert(new[] {job});
            else
                Update(job);
        }

        public void Store(IEnumerable<Job> jobs)
        {
            Insert(jobs);
        }

        public IEnumerable<Job> LoadSuspended(Type forActivityType, int max)
        {
            return
                _connection.Query(
                    string.Format(
                        "select top {0} {1} from DependableJobs " +
                        "where Type = @Type and suspended = 1 and InstanceName = @InstanceName order by CreatedOn",
                        max,
                        Columns),
                        new
                        {
                            Type = SerializationUtilities.PersistedTypeName(forActivityType), 
                            InstanceName = _instanceName
                        })
                    .Select(Deserialize);
        }

        public IEnumerable<Job> LoadSuspended(IEnumerable<Type> excludeActivityTypes, int max)
        {
            return
                _connection.Query(
                    string.Format(
                        "select top {0} {1} from DependableJobs " +
                        "where Type not in (@Exclude) and suspended = 1 and " + 
                        "InstanceName = @InstanceName order by CreatedOn",
                        max,
                        Columns),
                        new
                        {
                            Exclude = excludeActivityTypes,
                            InstanceName = _instanceName
                        })
                    .Select(Deserialize);
        }

        public int CountSuspended(Type type)
        {
            return
                _connection.ExecuteScalar<int>(
                    "select count(*) from DependableJobs " +
                    "where Type = COALESCE(@Type, Type) and suspended = 1 and InstanceName = @InstanceName",
                    new
                    {
                        Type =  type != null ? SerializationUtilities.PersistedTypeName(type) : null, 
                        InstanceName = _instanceName
                    });
        }

        static Job Deserialize(dynamic record)
        {
            var arguments = DeserializeArguments(record.Arguments);
            var exceptionFilters = DesrializeExceptionFilters(record.ExceptionFilters);

            var job = new Job(
                (Guid) record.Id,
                (Type) Type.GetType(record.Type),
                record.Method,
                arguments,
                (DateTime) record.CreatedOn,
                (Guid) record.RootId,
                (Guid?) record.ParentId,
                (Guid) record.CorrelationId,
                Enum.Parse(typeof (JobStatus), record.Status),
                (int) record.DispatchCount,
                (DateTime?) record.RetryOn,
                exceptionFilters)
            {
                Suspended = (bool) record.Suspended,
                Continuation = JsonConvert.DeserializeObject<Continuation>(
                    record.Continuation,
                    DefaultSerializerSettings)
            };

            return job;
        }

        void Insert(IEnumerable<Job> jobs)
        {
            var table = JobsDataTable.Create();

            foreach (var job in jobs)
            {
                var r = table.NewRow();

                r["Id"] = job.Id;
                r["Type"] = SerializationUtilities.PersistedTypeName(job.Type);
                r["Method"] = job.Method;

                r["Arguments"] = SerializeArguments(job.Arguments);

                r["CreatedOn"] = job.CreatedOn;
                r["RootId"] = job.RootId;

                if (job.ParentId.HasValue)
                    r["ParentId"] = job.ParentId;

                r["CorrelationId"] = job.CorrelationId;
                r["Status"] = job.Status.ToString();
                r["DispatchCount"] = job.DispatchCount;

                if (job.RetryOn.HasValue)
                    r["RetryOn"] = job.RetryOn;

                r["Continuation"] = JsonConvert.SerializeObject(job.Continuation, DefaultSerializerSettings);
                r["ExceptionFilters"] = SerializeExceptionFilters(job.ExceptionFilters);

                r["Suspended"] = job.Suspended;
                r["InstanceName"] = _instanceName;

                table.Rows.Add(r);
            }

            table.AcceptChanges();

            using (var bulkCopy = new SqlBulkCopy(_connection))
            {
                bulkCopy.DestinationTableName = "DependableJobs";
                bulkCopy.WriteToServer(table);
            }
        }

        void Update(Job job)
        {
            _connection.Execute(
                "update DependableJobs set Status = @Status, DispatchCount = @DispatchCount, " +
                "RetryOn = @RetryOn, Continuation = @Continuation, Suspended = @Suspended where Id = @Id",
                new
                {
                    job.Id,
                    Status = job.Status.ToString(),
                    job.DispatchCount,
                    job.RetryOn,
                    Continuation = JsonConvert.SerializeObject(job.Continuation, DefaultSerializerSettings),
                    job.Suspended
                });
        }

        static string SerializeArguments(IEnumerable<object> arguments)
        {
            return JsonConvert.SerializeObject(arguments.Select(StoredArgument.From).ToArray());
        }

        static object[] DeserializeArguments(string serializedArguments)
        {
            var arguments = JsonConvert.DeserializeObject<StoredArgument[]>(serializedArguments);
            return arguments.Select(a => a.ToObject()).ToArray();
        }

        static string SerializeExceptionFilters(IEnumerable<ExceptionFilter> exceptionFilters)
        {
            return JsonConvert.SerializeObject(
                exceptionFilters.Select(f => new StoredExceptionFilter
                {
                    TypeName = SerializationUtilities.PersistedTypeName(f.Type),
                    Method = f.Method,
                    Arguments = f.Arguments.Select(StoredArgument.From).ToArray()
                }));
        }

        static ExceptionFilter[] DesrializeExceptionFilters(string serializedExceptionFilters)
        {
            var exceptionFilters = JsonConvert.DeserializeObject<StoredExceptionFilter[]>(serializedExceptionFilters);

            return exceptionFilters
                .Select(f => new ExceptionFilter
                {
                    Type = Type.GetType(f.TypeName),
                    Method = f.Method,
                    Arguments = f.Arguments.Select(a => a.ToObject()).ToArray()
                }).ToArray();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}