namespace MyCode
{
    public partial class ClassGeneric<T> : System.Collections.Generic.Dictionary<T, T>
    {
        public T A { get; set; }

        public System.Collections.Generic.Dictionary<T, T> B { get; set; }

        public System.Action<T?>? F;

        public T A1;

        public System.Collections.Generic.Dictionary<T, T> B1;

        public System.Action<T?> F1;

        public static T A2 { get; set; }

        public static System.Collections.Generic.Dictionary<T, T> B2 { get; set; }

        public static System.Action<T?> F2 { get; set; }

        public static T A3;

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
            
        }
    }
}