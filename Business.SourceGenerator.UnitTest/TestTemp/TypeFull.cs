


using System;























namespace MyCode
{
    public interface INumberBase<TSelf>
        where TSelf : INumberBase<TSelf>
    {
    }
    public class TypeFull3
    {
        public TypeFull3 A;

        public System.Reflection.FieldInfo B;

        public void C(System.TypedReference aaa, DateTimeOffset b, DateTime c)
        {
            var ss = b.Date;

            var dd = new DateTimeOffset(b.Year, b.Month, b.Day, b.Hour, b.Minute, b.Second, b.Millisecond, b.Microsecond, b.Offset);
            //dd.to
            //DateTimeOffset.TryParse().
            //DateTime.TryParse();
            var dd2 = new DateTime(c.Year, c.Month, c.Day, c.Hour, c.Minute, c.Second, c.Millisecond, c.Microsecond, c.Kind);
        }
    }

    //public ref partial struct TypedReference
    //{
    //    public static object? ToObject(TypedReference value)
    //    {
    //        object? result = default;

    //        return result;
    //    }
    //}

    [Business.SourceGenerator.Meta.GeneratorType]
    public partial struct TypeFull<Type>
    //where Type : INumberBase<Type>
    {
        public delegate object GetValue(object obj);

        public string Write<T>() where T : struct => default;

        public GetValue Get { get; set; }

        public GetValue Get2 { get; init; }

        public event Func<System.Reflection.Assembly, string, IntPtr>? ResolvingUnmanagedDll;

        public string A { get; set; }
        public Type B { get; set; }

        public (int a, string, decimal c) C { get; set; }

        public static Type C2 { get; set; }

        public static Type D2;

        public static Enum E;

        public delegate void UnhandledExceptionEventHandler2(object sender, ref UnhandledExceptionEventArgs e, out string e2);

        public event UnhandledExceptionEventHandler2? E2;

        public static event UnhandledExceptionEventHandler2? E3;

        //public string System_ValueTuple_System_Int32__System_String_ { get; set; }

        public static TOther CreateSaturating<TOther>()
            where TOther : INumberBase<TOther> => default;
        public TypeFull(string a)
        {
            var dd = new UnhandledExceptionEventHandler2((object a, ref UnhandledExceptionEventArgs b, out string c) => { c = default; });
            //
            Get = obj =>
            {
                var obj2 = (TypeFull<Type>)obj;
                //return obj2.Write<object>();
                return default;
            };
            this.A = a ?? throw new System.ArgumentNullException(nameof(a));
        }

        public static void StructMethod3<T2>(string? a, ref T2 b, out (int? c1, string? c2) c)
        {
            c = (333, "xxx");
            //return default;
        }

        public static System.Threading.Tasks.ValueTask<T2> StructMethod2<T2>(string? a, ref T2 b, out (int? c1, string? c2) c)
        {
            c = (333, "xxx");
            return default;
        }
        public System.Threading.Tasks.ValueTask<T2> StructMethod<T2>(string? a, ref T2 b, out (int? c1, string? c2) c)
            where T2 : class, System.Collections.Generic.IList<int>
        {
            //b.c1 = (T)888;
            c = (333, "xxx");
            return System.Threading.Tasks.ValueTask.FromResult(b);
        }
    }

    public enum MyEnum
    {
        A, B, C
    }
}


