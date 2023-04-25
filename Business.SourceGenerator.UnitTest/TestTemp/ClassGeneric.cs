




namespace MyCode
{
    public partial class ClassGeneric<T> : System.Collections.Generic.Dictionary<T, T>
    where T : class
    {
        public System.Nullable<System.DateTimeOffset> A0 { get; set; }

        public T A { get; set; }

        public int[] C { get; set; }

        public System.Collections.Generic.Dictionary<T, T> B { get; set; }

        public System.Action<T?>? F;

        public T A1;

        public System.Collections.Generic.Dictionary<T, T> B1;

        public System.Action<T?> F1;

        public static T A2 { get; set; }

        public static System.Collections.Generic.Dictionary<T, T> B2 { get; set; }

        public static System.Action<T?> F2 { get; set; }

        public static int A3;

        public static System.Collections.Generic.Dictionary<T, T> B3;

        public static System.Action<T?> F3;

        public System.Threading.Tasks.ValueTask<T> G(T a, int b, decimal c, params object[] args)
        {
            return System.Threading.Tasks.ValueTask.FromResult<T>(default);
        }

        public static System.Threading.Tasks.Task<T> GG(T a, int b, decimal c, params object[] args)
        {
            return System.Threading.Tasks.Task.FromResult<T>(default);
        }

        public ClassGeneric()
        {
            //System.Globalization.Calendar
        }

        public ClassGeneric(T t)
        {

        }

        public ClassGeneric(ref int aaa, out int bbb)
        {
            bbb = 3;
            var ss = new sbyte[666];
            ss[2] = 6;
        }
    }
}