











namespace MyCode
{
    public partial class Refs
    {
        readonly int A;

        public Refs(ref int a, out System.DateTimeOffset b)
        {
            a = 789;
            A = a;
            b = System.DateTimeOffset.Now;
        }

        public System.Threading.Tasks.ValueTask<dynamic> Method0(string ooo, out int bbb, ref int aaa, out int ddd, string www = "sss", params string[] ccc)
        {
            ccc = new string[] { "111", "222" };
            aaa = 999;

            bbb = 3;
            ddd = 4;

            return System.Threading.Tasks.ValueTask.FromResult<dynamic>(ooo);
        }

        public System.Threading.Tasks.ValueTask<dynamic> Method0(ref string ooo, out int bbb, ref int aaa, out int ddd, string www = "sss", params string[] ccc)
        {
            ccc = new string[] { "111", "222" };
            aaa = 999;

            bbb = 3;
            ddd = 4;

            return default;
        }
    }
}
