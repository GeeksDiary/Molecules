using System;

static internal class SerializationUtilities
{
    public static string PersistedTypeName(Type type)
    {
        return String.Format("{0}, {1}", type.FullName, type.Assembly.GetName().Name);
    }
}