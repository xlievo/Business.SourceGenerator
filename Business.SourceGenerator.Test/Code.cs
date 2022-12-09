namespace MyCode
{
    public partial class TypeTarget
    {
        public string A { get; set; }

        public int B { get; set; }

        public static System.DateTime? C;

        public static string? D;

        public static object? E;

        public static dynamic? F;

        public System.Threading.Tasks.ValueTask G(dynamic a, int b = 2, params object[] args)
        {
            return System.Threading.Tasks.ValueTask.CompletedTask;
        }
    }
}






























