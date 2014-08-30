using System;
using Newtonsoft.Json.Linq;

namespace Dependable.Extensions.Persistence.Sql
{
    public class StoredArgument
    {
        public string TypeName { get; set; }

        public JToken Value { get; set; }

        public static StoredArgument From(object argument)
        {
            return new StoredArgument
            {
                TypeName = SerializationUtilities.PersistedTypeName(argument.GetType()),
                Value = JToken.FromObject(argument)
            };
        }

        public object ToObject()
        {
            return Value.ToObject(Type.GetType(TypeName, true));
        }
    }
}