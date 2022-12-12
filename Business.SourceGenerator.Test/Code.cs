

namespace MyCode
{
    public partial class TypeTarget
    {
        public string A { get; set; }

        public int? B { get; set; }

        public System.DateTime? C { get; set; }

        public object? D { get; set; }

        public dynamic? E { get; set; }

        public System.Action<int?>? F;

        public string A1;

        public int? B1;

        public System.DateTime? C1;

        public object? D1;

        public dynamic? E1;

        public System.Action<int?> F1;

        public static string A2 { get; set; }

        public static int? B2 { get; set; }

        public static System.DateTime? C2 { get; set; }

        public static object? D2 { get; set; }

        public static dynamic? E2 { get; set; }

        public static System.Action<int?> F2 { get; set; }

        public static string A3;

        public static int? B3;

        public static System.DateTime? C3;

        public static object? D3;

        public static dynamic? E3;

        public static System.Action<int?> F3;

        public System.Threading.Tasks.ValueTask G(dynamic a, int b = 2, params object[] args)
        {
            return System.Threading.Tasks.ValueTask.CompletedTask;
        }
    }
}






























