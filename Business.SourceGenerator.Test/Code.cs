namespace MyCode
{
    public class TypeTarget
    {
        public string A { get; set; }

        public int B { get; set; }

        public static System.DateTime? C;

        public static string? D;

        public System.Threading.Tasks.ValueTask E(dynamic a, int b = 2, params object[] args)
        {
            return System.Threading.Tasks.ValueTask.CompletedTask;
        }
    }
}
























