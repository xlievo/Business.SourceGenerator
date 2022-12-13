namespace MyCode
{
    public partial struct StructMethodTarget
    {
        public System.Threading.Tasks.ValueTask G(dynamic a, int b, decimal c, params object[] args)
        {
            return System.Threading.Tasks.ValueTask.CompletedTask;
        }
    }
}
