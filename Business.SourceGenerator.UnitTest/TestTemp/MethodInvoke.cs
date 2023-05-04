











namespace MyCode
{
    public partial struct MethodInvoke<T>
        where T : System.IDisposable
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

        public (int? c1, string? c2) StructMethod3(ref string? a, out int? b, ref (int? c1, string? c2) c, out (int? c1, string? c2) d, string e = "eee")
        {
            d = default;
            var dd = c.c1 as dynamic;
            b = 9;
            a = "www";
            c.c1 = 6789;

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

        [Business.SourceGenerator.Test.GeneratorType2(typeof(System.DateTimeOffset), "999", 333)]
        [Business.SourceGenerator.Test.GeneratorType2(typeof(System.DateTimeOffset), b: 444)]
        [Business.SourceGenerator.Test.GeneratorType2]
        public System.Threading.Tasks.ValueTask<dynamic> StructMethod7<T2>(
        ref T2? a,
        out int? b,
        ref (System.DateTimeOffset? c1, T2? c2) c,
        out (int? c1, string? c2) d,
        ref StructMember2 structMember21,
        string e = "eee",
        System.Single f = default,
        StructMember2 structMember2 = default,
        object ddf = default, params int[] ppp)
            where T2 : System.Collections.Generic.IList<string>
        {
            structMember21.A = 333666;
            d = (555, "TTT");
            var dd = c.c1 as dynamic;
            b = 9;
            //a = "www";
            //c.c1 = 6789;
            c.c2.Add("6789");
            f = 123;
            return System.Threading.Tasks.ValueTask.FromResult<dynamic>(f);
        }

        public System.Threading.Tasks.ValueTask<dynamic> StructMethod7<T2>(
        T2? a,
        out int? b,
        ref (System.DateTimeOffset? c1, T2? c2) c,
        out (int? c1, string? c2) d,
        ref StructMember2 structMember21,
        string e = "eee",
        System.Single f = default,
        StructMember2 structMember2 = default,
        object ddf = default, params int[] ppp)
            where T2 : System.Collections.Generic.IList<string>
        {
            structMember21.A = 333666;
            d = (666, "YYY");
            var dd = c.c1 as dynamic;
            b = 9;
            //a = "www";
            //c.c1 = 6789;
            c.c2.Add("6789");
            f = 123;
            return System.Threading.Tasks.ValueTask.FromResult<dynamic>(f);
        }

        public MethodInvoke(ref int a, out int b, params string[] c)
        {
            b = 0;
        }

        //public System.Threading.Tasks.ValueTask<(T? c1, string? c2)> StructMethod8<Type>(ref string? a, out int? b, ref (T? c1, string? c2) c, out (T? c1, Type? c2) d, Type e, (T? c1, Type? c2) e2, ref T e3, ref Type e4, out T e5, out Type e6)
        //{
        //    e5 = default;
        //    e6 = default;
        //    d = default;
        //    var dd = c.c1 as dynamic;
        //    b = 9;

        //    return System.Threading.Tasks.ValueTask.FromResult(c);
        //}
    }

    // public partial class TTT<T, T2>
    //where T2 : System.Collections.Generic.IDictionary<string, System.IDisposable>
    // {
    //     public TTT(T t, params object[] objs)
    //     {

    //     }

    //     public T2 TTT2<T>(T2 t2, T t) where T : System.Collections.Generic.IDictionary<string, System.DateTime> => default;
    // }
}

public partial struct StructMember2
{
    public StructMember2(int? a, System.DateTime? c, object d, dynamic e)
    {
        A = a;
        C = c;
        D = d;
        E = e;

        //var methodInvoke = new MethodInvoke<System.IO.MemoryStream>();

        //var aaa = new System.IO.MemoryStream();
        //IList<string> bbb = new List<string>();

        ////(T? c1, T2? c2)
        //(System.IO.MemoryStream c1, IList<string> c2) cc = (aaa, bbb);

        //var structMember2 = new MyCode.StructMember2 { A = 111 };

        //methodInvoke.StructMethod7(ref bbb, out int? bb, ref cc, out (int? c1, string c2) ddd, ref structMember2).GetAwaiter().GetResult();
    }

    public int? A { get; set; }

    public System.DateTime? C { get; set; }
    public object? D { get; set; }

    public dynamic? E { get; set; }
}
