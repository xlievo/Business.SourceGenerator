namespace Business.SourceGenerator.Test.TestTemp
{
    public partial class ClassMethod
    {
        public System.Threading.Tasks.ValueTask G(dynamic a, int b, decimal c, params object[] args)
        {
            return System.Threading.Tasks.ValueTask.CompletedTask;
        }
    }

    public partial class TTT<T, T2>
    where T2 : System.Collections.Generic.IDictionary<string, System.IDisposable>
    {
        public TTT(T t, params object[] objs)
        {

        }

        public T2 TTT2<T>(T2 t2, T t) where T : System.Collections.Generic.IDictionary<string, System.DateTime> => default;
    }
}
