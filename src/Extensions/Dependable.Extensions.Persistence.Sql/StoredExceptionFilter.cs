namespace Dependable.Extensions.Persistence.Sql
{
    public class StoredExceptionFilter
    {
        public string TypeName { get; set; }

        public string Method { get; set; }

        public StoredArgument[] Arguments { get; set; }
    }
}