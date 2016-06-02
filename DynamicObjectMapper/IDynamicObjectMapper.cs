using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynamicObjectMapper
{
    public interface IDynamicObjectMapper<in T> where T : new()
    {
        IDictionary<string, object> Map(T source, params string[] properties);

        Task<object> ReshapeObjectAsync(string properties, T originalObject);
    }
}
