using System;
using System.Data;

namespace Dependable.Extensions.Persistence.Sql
{
    static internal class JobsDataTable
    {
        public static DataTable Create()
        {
            var table = new DataTable("DependableJobs");
            table.Columns.AddRange(
                new[]
                {
                    new DataColumn {ColumnName = "Id", DataType = typeof (Guid)},
                    new DataColumn {ColumnName = "Type", DataType = typeof (string)},
                    new DataColumn {ColumnName = "Method", DataType = typeof (string)},
                    new DataColumn {ColumnName = "Arguments", DataType = typeof (string)},
                    new DataColumn {ColumnName = "CreatedOn", DataType = typeof (DateTime)},
                    new DataColumn {ColumnName = "RootId", DataType = typeof (Guid)},
                    new DataColumn
                    {
                        ColumnName = "ParentId",
                        DataType = typeof (Guid),
                        AllowDBNull = true
                    },
                    new DataColumn {ColumnName = "CorrelationId", DataType = typeof (Guid)},
                    new DataColumn {ColumnName = "Status", DataType = typeof (string)},
                    new DataColumn {ColumnName = "DispatchCount", DataType = typeof (int)},
                    new DataColumn
                    {
                        ColumnName = "RetryOn",
                        DataType = typeof (DateTime),
                        AllowDBNull = true
                    },
                    new DataColumn {ColumnName = "Continuation", DataType = typeof (string)},
                    new DataColumn {ColumnName = "ExceptionFilters", DataType = typeof (string)},
                    new DataColumn {ColumnName = "Suspended", DataType = typeof (bool)},
                    new DataColumn {ColumnName = "InstanceName", DataType = typeof (string)}
                });

            table.PrimaryKey = new[] {table.Columns[0]};
            return table;
        }
    }
}