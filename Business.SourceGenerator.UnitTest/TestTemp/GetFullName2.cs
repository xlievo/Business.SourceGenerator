using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using TypeNameFormatter;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System.ComponentModel.Design.Serialization;

namespace MyCode
{
    public unsafe class GetFullName2<T>
        where T : struct
    {
        const string AAA = "";

        event D2 E2 = default;

        delegate void D2(sbyte sb);

        public GetFullName2(T[]? typeParameter)
        {
            var z = "z";
        }

        //INamedTypeSymbol.TypeArguments
        public ValueTask<IDictionary<ValueTask<int[]?>?, ValueTask<int?>?>?>? A { get; set; }

        ////TupleType
        public (string? a, int? b)? B;

        public (T a, int b)? C;

        public (T? a, int? b)? D;

        public (T[]? a, int[]? b)? E { get; set; }

        public T* F { get; set; }

        public T?* G { get; set; }
        //System.ValueTuple<MyCode.GetFullName2<T>.T?*[], System.Int32?*[]>?
        public (T?*[]? a, int?*[]? b)? H { get; set; }

        public T[]? TypeParameter { get; set; }

        public dynamic? Dynamic { get; set; }

        public dynamic[]? Dynamic2 { get; set; }

        public void AnonymousType<T2>(dynamic? dynamic2, T2[]? t2, T? t, (DateOnly? a, ValueTask<DateTimeOffset?>? b)? p = default, Func<string, string> typeClean = default)
        where T2 : struct
        {
            _ = new { A = "A", B = new System.Nullable<int>(3) };

            _ = new { T2 = t2, T = t };

            dynamic s = new { A = "A", B = new System.Nullable<int>(3) };

            s.A = "B";

            _ = typeof(IDictionary<,>);

            //_ = new T2();

            dynamic s2 = 2;

            "aaa".Select(c =>
            {
                var c2 = new { T2 = t2, T = t };
                return Char.IsSymbol(c);
            });

            new List<DateFormat>().Select(c =>
            {
                var c2 = new { T2 = t2, T = t };
                return "";
            });

            string typeClean2<T3>(string name, T[]? t, T2? t2, T3? t3)
                where T3 : struct
                => typeClean?.Invoke(name) ?? name;
        }
    }

    public static class GetFullName3
    {
        public static (string, DateInterval) AnonymousType2<T2>(this T2? t2, dynamic? dynamic2, (DateOnly? a, ValueTask<DateTimeOffset?>? b)? p = default, Func<string, string> typeClean = default)
            where T2 : System.Collections.Generic.Dictionary<string, System.DateTime>, System.Collections.Generic.IDictionary<string, System.DateTime>, System.IDisposable
            => default;
    }
}
