using System;

namespace Dependable
{
    public class SingleActivity : Activity
    {
        internal SingleActivity(Type type, string name, object[] arguments)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (arguments == null) throw new ArgumentNullException("arguments");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A valid name is required.");

            Type = type;
            Name = name;
            Arguments = arguments;
        }

        public Type Type { get; private set; }

        public string Name { get; private set; }

        public object[] Arguments { get; private set; }

        public Activity OnFailed { get; private set; }

        public SingleActivity Failed(Activity next)
        {
            if (next == null) throw new ArgumentNullException("next");
            OnFailed = next;
            return this;
        }
    }
}