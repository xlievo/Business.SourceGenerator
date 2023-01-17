namespace MyCode
{
    public partial struct MethodInvoke
    {
        public System.Threading.Tasks.ValueTask G(dynamic a, int b, decimal c, params object[] args)
        {
            return System.Threading.Tasks.ValueTask.CompletedTask;
        }

        public void StructMember2(ref string? a, out int? b, ref (int? c1, string? c2) c, out (int? c1, string? c2) d)
        {
            d = default;
            var dd = c.c1 as dynamic;
            b = 9;
        }

        public (int? c1, string? c2) StructMethod3(ref string? a, out int? b, ref (int? c1, string? c2) c, out (int? c1, string? c2) d)
        {
            d = default;
            var dd = c.c1 as dynamic;
            b = 9;

            return c;
        }

        public System.Threading.Tasks.Task StructMethod4(ref string? a, out int? b, ref (int? c1, string? c2) c, out (int? c1, string? c2) d)
        {
            d = default;
            var dd = c.c1 as dynamic;
            b = 9;

            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.ValueTask StructMethod5(ref string? a, out int? b, ref (int? c1, string? c2) c, out (int? c1, string? c2) d)
        {
            d = default;
            var dd = c.c1 as dynamic;
            b = 9;

            return System.Threading.Tasks.ValueTask.CompletedTask;
        }

        public System.Threading.Tasks.Task<(int? c1, string? c2)> StructMethod6(ref string? a, out int? b, ref (int? c1, string? c2) c, out (int? c1, string? c2) d)
        {
            d = default;
            var dd = c.c1 as dynamic;
            b = 9;

            return System.Threading.Tasks.Task.FromResult(c);
        }

        public System.Threading.Tasks.ValueTask<(int? c1, string? c2)> StructMethod7(ref string? a, out int? b, ref (int? c1, string? c2) c, out (int? c1, string? c2) d)
        {
            d = default;
            var dd = c.c1 as dynamic;
            b = 9;

            return System.Threading.Tasks.ValueTask.FromResult(c);
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
