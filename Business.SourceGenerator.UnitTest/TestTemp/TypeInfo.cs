





using System.Collections.Generic;

namespace MyCode
{
    public partial struct TypeInfo<Type>
    {
        public System.Threading.Tasks.ValueTask<T> Test2<T>(T? a, System.DateTimeOffset? c, Type d, dynamic e)
        {
            var t = new Dictionary<T, Type>();
            var type = t.GetType();

            return System.Threading.Tasks.ValueTask.FromResult<T>(default);
        }
    }
}
