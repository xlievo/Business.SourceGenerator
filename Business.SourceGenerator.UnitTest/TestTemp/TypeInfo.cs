








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
    {
        public string A { get; set; }

        public T B { get; set; }

        public MyStruct(string a)
        {
            this.A = a ?? throw new System.ArgumentNullException(nameof(a));
        }

        public System.Threading.Tasks.ValueTask<(int c1, string c2)> StructMethod(ref string? a, ref (int c1, string c2) b, out (int? c1, string? c2) c)
        {
            c = (333, "xxx");
            return System.Threading.Tasks.ValueTask.FromResult(b);
        }
    }
}
