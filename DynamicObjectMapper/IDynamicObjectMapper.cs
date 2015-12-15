using System.Collections.Generic;

namespace DynamicObjectMapper
{
    public interface IDynamicObjectMapper<in T> where T : new()
    {
        IDictionary<string, object> Map(T source, params string[] properties);
    }
}
