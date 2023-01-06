namespace MyCode
{
    public partial struct StructMethod
    {
        public System.Threading.Tasks.ValueTask G(dynamic a, int b, decimal c, params object[] args)
        {
            return System.Threading.Tasks.ValueTask.CompletedTask;
        }

        public void StructMember2(ref string? a, out int? b, ref (int? c1, string? c2) c, out (int? c1, string? c2) d)
        {
            d = default;
            var dd = c.c1 as dynamic;

            //this.A = a;
            b = 9;
        }

        public (int? c1, string? c2) StructMethod3(ref string? a, out int? b, ref (int? c1, string? c2) c, out (int? c1, string? c2) d)
        {
            d = default;
            var dd = c.c1 as dynamic;

            //this.A = a;
            b = 9;

            return c;
        }

        public System.Threading.Tasks.Task StructMethod4(ref string? a, out int? b, ref (int? c1, string? c2) c, out (int? c1, string? c2) d)
        {
            d = default;
            var dd = c.c1 as dynamic;

            //this.A = a;
            b = 9;

            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.ValueTask StructMethod5(ref string? a, out int? b, ref (int? c1, string? c2) c, out (int? c1, string? c2) d)
        {
            d = default;
            var dd = c.c1 as dynamic;

            //this.A = a;
            b = 9;

            return System.Threading.Tasks.ValueTask.CompletedTask;
        }

        public System.Threading.Tasks.Task<(int? c1, string? c2)> StructMethod6(ref string? a, out int? b, ref (int? c1, string? c2) c, out (int? c1, string? c2) d)
        {
            d = default;
            var dd = c.c1 as dynamic;

            //this.A = a;
            b = 9;

            return System.Threading.Tasks.Task.FromResult(c);
        }

        public System.Threading.Tasks.ValueTask<(int? c1, string? c2)> StructMethod7(ref string? a, out int? b, ref (int? c1, string? c2) c, out (int? c1, string? c2) d)
        {
            d = default;
            var dd = c.c1 as dynamic;

            //this.A = a;
            b = 9;

            return System.Threading.Tasks.ValueTask.FromResult(c);
        }
    }
}
