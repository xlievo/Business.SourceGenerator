using Business.SourceGenerator;
using Business.SourceGenerator.Meta;
using Business.SourceGenerator.Test;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
//using System.Text.Json;
//using System.Text.Json.Serialization;
//using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;

//using Serde;
//using Serde.Json;

using System.Threading.Tasks;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Xml.Serialization;
using System.Xml.Linq;
using static Business.SourceGenerator.Utils;

//[module: SuppressMessage("Microsoft.Design", "CS8604:", Scope = "namespace", Target = "Business.SourceGenerator.Test.Console")]

//[assembly: AssemblyMetadata("IsTrimmable", "False")]

namespace Business.SourceGenerator.Test.Console
{
    //[UnconditionalSuppressMessage("AOT", "CS8604")]
    //[UnconditionalSuppressMessage("Aot", "CS8604:",
    //Justification = "The unfriendly method is not reachable with AOT")]
    internal class Program
    {
        //public static MethodInfo? GetMethod<T>(System.Linq.Expressions.Expression<System.Action<T>> methodSelector) => ((System.Linq.Expressions.MethodCallExpression)methodSelector.Body).Method;

        static async Task<int> Main(string[] args)
        {
            BusinessSourceGenerator.Generator.SetGeneratorCode();
            
            string? a5 = "a5";
            (int c1, string c2) c5 = (55, "66");
            //var rrr5 = GetMethod<MyStruct>(c => c.StructMethod7(ref a5, ref c5)).Name;
            //System.Console.WriteLine(rrr5);

            //var myStruct = typeof(MyStruct<>)
            //    .GetGenericType(typeof(int))
            //    .CreateInstance<IGeneratorAccessor>("666");

            ////ref string? a, ref (int c1, string c2) b, out (int? c1, string? c2) c
            //var args2 = new object[] { 
            //    string.Empty,
            //    RefArg.Ref((55, "66")),
            //    RefArg.Out<(int? c1, string? c2)>()
            //};

            //var r = await myStruct.AccessorMethodAsync("StructMethod", args2);


            var result = typeof(ClassGeneric<string>)
                    .CreateInstance<IGeneratorAccessor>()
                    .AccessorSet<IGeneratorAccessor>("A", "WWW")
                    .AccessorSet<IGeneratorAccessor>("B", new Dictionary<string, string>());

            var type = result.GetType();
            try
            {
                System.Console.WriteLine("IsGenericType：" + type.IsGenericType);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            try
            {
                System.Console.WriteLine("IsGenericParameter：" + type.IsGenericParameter);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            try
            {
                System.Console.WriteLine("IsGenericTypeDefinition：" + type.IsGenericTypeDefinition);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

           

            //System.Console.WriteLine(type.IsValueType);
            ////System.Console.WriteLine(type.IsSealed);
            //System.Console.WriteLine(type.IsAbstract);
            ////System.Console.WriteLine(type.IsOverride);
            ////System.Console.WriteLine(type.IsVirtual);
            ////System.Console.WriteLine(type.IsStatic);
            //System.Console.WriteLine(type.IsDefinition());


            var MyStruct111 = typeof(MyStruct)
                .CreateInstance<IGeneratorAccessor>("aaa111")
                .AccessorSet<IGeneratorAccessor>("bbb", 888)
                .AccessorSet<IGeneratorAccessor>("ccc", DateTimeOffset.Now);

            //var MyStruct222 = typeof(MyStruct2).CreateInstance<MyStruct2>("222");

            //var myStruct = (MyStruct2)(MyStruct222);
            //System.Console.WriteLine(JsonSerializer.Serialize(MyStruct222));
            //[Serde.Json.JsonSerializer]

           var structMethod7 = (MyStruct111.AccessorType().Members["StructMethod7"] as IAccessorMethodCollection).First();
            foreach (var item in structMethod7.Attributes)
            {
                var arg = item.Constructor;
                
                System.Console.WriteLine(item.Name + " " + arg.ElementAt(0).Value + " " + arg.ElementAt(1).Value + " " + arg.ElementAt(2).Value);
            }

            var arr0attr = structMethod7.Parameters[0].Attributes.First();
            var arg2 = arr0attr.Constructor;
            System.Console.WriteLine(arr0attr.Name + " " + arg2.ElementAt(0).Value + " " + arg2.ElementAt(1).Value + " " + arg2.ElementAt(2).Value);

            var arr0attr2 = structMethod7.Parameters[1].Attributes.First();
            var arg3 = arr0attr2.Constructor;
            System.Console.WriteLine(arr0attr2.Name + " " + arg3.ElementAt(0).Value + " " + arg3.ElementAt(1).Value + " " + arg3.ElementAt(2).Value);

            //StructMember2(ref string? a, out int* b, ref (int c1, string c2) c, out (int? c1, string? c2) d)
            if (MyStruct111.AccessorMethod("StructMember2", out (int c1, string c2) value222, "a", (777, "xzzzxx")))
            {
                System.Console.WriteLine(value222);
            }

            //StructMethod7(ref string? a, out int? b, ref (int c1, string c2) c, out (int? c1, string? c2) d)
            if (MyStruct111.AccessorMethod("StructMethod7", out ValueTask<(int c1, string c2)> value7, RefArg.Ref("a"), RefArg.Out<int?>(), RefArg.Ref((888, "xzzzxx")), RefArg.Out<(int?, string?)>()))
            {
                System.Console.WriteLine(await value7);
            }

            MyStruct111.AccessorMethod<ValueTask<(int c1, string c2)>>("StructMethod7", out var value7772, RefArg.Ref("a"), RefArg.Out<int?>(), RefArg.Ref((999, "xzzzxx")), RefArg.Out<(int?, string?)>());

            System.Console.WriteLine(await value7772);

            var args7 = new object[] { RefArg.Ref("a"), RefArg.Out<int?>(), RefArg.Ref((999, "xzzzxx")), RefArg.Out<(int?, string?)>() };

            var value777 = await MyStruct111.AccessorMethodAsync<(int c1, string c2)>("StructMethod7", args7);

            System.Console.WriteLine(((RefArg)args7[1]).Value);



            if (MyStruct111.AccessorGet("aaa", out string value))
            {
                System.Console.WriteLine(value);
            }

            if (MyStruct111.AccessorGet("bbb", out int? value22))
            {
                System.Console.WriteLine(value22);
            }

            if (MyStruct111.AccessorGet("ccc", out DateTimeOffset? value3))
            {
                System.Console.WriteLine(value3);
            }

            _ = MyStruct111.AccessorGet("ccc", out object rrr);

            System.Console.WriteLine(rrr);

            var data = typeof(ResultObject<>).GetGenericType(typeof(MyStruct)).CreateInstance(MyStruct111, 8888, "message", true);

            var data5 = typeof(ResultObject3<>).GetGenericType<MyStruct5>().CreateInstance<ResultObject3<MyStruct5>>(new MyStruct5("```5555555555555555555555~!@#"), 333, "message", true);

            var data6 = typeof(ResultObject3<>).GetGenericType(typeof(MyStruct6)).CreateInstance<ResultObject3<MyStruct6>>(typeof(MyStruct6).CreateInstance("```6666666666666666666666~!@#"), 333, "message", true);

            var data7 = typeof(ResultObject3<>).GetGenericType(typeof(MyStruct7)).CreateInstance<ResultObject3<MyStruct7>>(new MyStruct7("```7777777777777777777777~!@#"), 333, "message", true);

            var data8 = typeof(ResultObject3<>).GetGenericType(typeof(MyStruct8)).CreateInstance<ResultObject3<MyStruct8>>(new MyStruct8("```8888888888888888888888~!@#"), 333, "message", true);

            var data9 = typeof(ResultObject3<>).GetGenericType(typeof(MyStruct9)).CreateInstance<ResultObject3<MyStruct9>>(new MyStruct9("```9999999999999999999999~!@#"), 333, "message", true);

            var data10 = typeof(ResultObject3<>).GetGenericType(typeof(MyStruct10)).CreateInstance<ResultObject3<MyStruct10>>(new MyStruct10("```1010101010101010101~!@#"), 333, "message", true);

            System.Console.WriteLine(data);

            System.Console.WriteLine(data5.Data.aaa);
            System.Console.WriteLine(data6.Data.aaa);
            System.Console.WriteLine(data7.Data.aaa);
            System.Console.WriteLine(data8.Data.aaa);
            System.Console.WriteLine(data9.Data.aaa);
            System.Console.WriteLine(data10.Data.aaa);

            //System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(data5, typeof(ResultObject3<MyStruct5>), SourceGenerationContext2.Default));
            //System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(data6, typeof(ResultObject3<MyStruct6>), SourceGenerationContext2.Default));
            //System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(data7, typeof(ResultObject3<MyStruct7>), SourceGenerationContext2.Default));
            //System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(data8, typeof(ResultObject3<MyStruct8>), SourceGenerationContext2.Default));
            //System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(data9, typeof(ResultObject3<MyStruct9>), SourceGenerationContext2.Default));
            //System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(data10, typeof(ResultObject3<MyStruct10>), SourceGenerationContext2.Default));

            System.Console.WriteLine(System.AppContext.BaseDirectory);

            System.Console.Read();

            return 0;
        }
    }
}

//[JsonSerializable(typeof(Type), GenerationMode = JsonSourceGenerationMode.Metadata)]
//[JsonSerializable(typeof(Assembly), GenerationMode = JsonSourceGenerationMode.Metadata)]
//[JsonSourceGenerationOptions(WriteIndented = true)]
////[JsonSerializable(typeof(ResultObject<MyStruct>), GenerationMode = JsonSourceGenerationMode.Metadata)]
//[JsonSerializable(typeof(ResultObject2<MyStruct2>), GenerationMode = JsonSourceGenerationMode.Metadata)]
//[JsonSerializable(typeof(ResultObject3<MyStruct3>), GenerationMode = JsonSourceGenerationMode.Metadata)]
//[JsonSerializable(typeof(ResultObject4<MyStruct4>), GenerationMode = JsonSourceGenerationMode.Metadata)]
//[JsonSerializable(typeof(ResultObject3<MyStruct5>), GenerationMode = JsonSourceGenerationMode.Metadata)]
//[JsonSerializable(typeof(ResultObject3<MyStruct6>), GenerationMode = JsonSourceGenerationMode.Metadata)]
//[JsonSerializable(typeof(ResultObject3<MyStruct7>), GenerationMode = JsonSourceGenerationMode.Metadata)]
//[JsonSerializable(typeof(ResultObject3<MyStruct8>), GenerationMode = JsonSourceGenerationMode.Metadata)]
//[JsonSerializable(typeof(ResultObject3<MyStruct9>), GenerationMode = JsonSourceGenerationMode.Metadata)]
//[JsonSerializable(typeof(ResultObject3<MyStruct10>), GenerationMode = JsonSourceGenerationMode.Metadata)]
//internal partial class SourceGenerationContext2 : JsonSerializerContext { }