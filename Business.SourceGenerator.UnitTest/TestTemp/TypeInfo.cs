













namespace MyCode
{
    public partial struct TypeInfo<Type>
    {
        public System.Threading.Tasks.ValueTask<T> Test2<T>(T? a, System.DateTimeOffset? c, Type d, dynamic e)
        {
            var t = new System.Collections.Generic.Dictionary<T, Type>();

            var type = t.GetType();

            return System.Threading.Tasks.ValueTask.FromResult<T>(default);
        }
    }

    [Business.SourceGenerator.Meta.GeneratorType]
    public partial struct MyStruct<T>
    //where T : class
    {
        public string A { get; set; }

        public T B { get; set; }

        public MyStruct(string a)
        {
            this.A = a ?? throw new System.ArgumentNullException(nameof(a));
        }

        public System.Threading.Tasks.ValueTask<T> StructMethod<T2>(string? a, ref T b, out (int? c1, string? c2) c)
        //where T2 : class, System.Collections.Generic.IList<int>
        {
            //b.c1 = (T)888;
            c = (333, "xxx");
            return System.Threading.Tasks.ValueTask.FromResult(b);
        }
    }
}
