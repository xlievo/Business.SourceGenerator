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

            var dtt = typeof(DateTimeOffset).AsGeneratorType().AccessorGet<DateTimeOffset>("Now");
            System.Console.WriteLine(dtt.Ticks);

            var d2 = dtt.AsGeneratorType().AccessorGet<long>("Ticks");

            System.Console.WriteLine(d2);

            var dtt3 = typeof(System.IO.BufferedStream).AsGeneratorType().CreateInstance<System.IO.BufferedStream>(System.IO.Stream.Null);
            var d23 = dtt3.AsGeneratorType().AccessorGet<long>("Length");
            System.Console.WriteLine(d23);
            //var ddd = async (obj, m, args) => { var result = ((global::Business.SourceGenerator.Test.ResultObject2<global::MyStruct7>)obj).ToBytes(!m[0].c ? (global::System.Boolean)m[0].v : default); return result; };



            string? a5 = "a5";
            (int c1, string c2) c5 = (55, "66");
            //var rrr5 = GetMethod<MyStruct>(c => c.StructMethod7(ref a5, ref c5)).Name;
            //System.Console.WriteLine(rrr5);

            var myStruct = typeof(MyStruct<>)
                .AsGeneratorType(typeof(MyStruct<List<MyStruct<List<int>>>>))
                .CreateInstance("888");

            //ref string? a, ref (int c1, string c2) b, out (int? c1, string? c2) c
            var args2 = new object[] {
                string.Empty,
                RefArg.Ref(new MyStruct<List<MyStruct<List<int>>>>("aaa555") { B = new List<MyStruct<List<int>>> { new MyStruct<List<int>>("") { B = new List<int> { 777 } } } }),
                RefArg.Out<(int? c1, string? c2)>()
            };

            //var ddd = typeof(System.Data.DataRowCollection);

            var r = await myStruct.AccessorMethodAsync<MyStruct<List<MyStruct<List<int>>>>>("StructMethod", args2);
            System.Console.WriteLine(r.B.FirstOrDefault().B.FirstOrDefault());

            System.Console.WriteLine(myStruct.AccessorGet<string>("A"));
            myStruct.AccessorSet("A", "999").AccessorSet("A", "555000");
            System.Console.WriteLine(myStruct.AccessorGet<string>("A"));

            var result = typeof(ClassGeneric<string>)
                    .AsGeneratorType()
                    .CreateInstance()
                    .AccessorSet("A", "WWW")
                    .AccessorSet<ClassGeneric<string>>("B", new Dictionary<string, string>());

            var type = result.GetType();


            //try
            //{
            //    System.Console.WriteLine("IsGenericType：" + type.IsGenericType);
            //}
            //catch (Exception ex)
            //{
            //    System.Console.WriteLine(ex.Message);
            //}

            //try
            //{
            //    System.Console.WriteLine("IsGenericParameter：" + type.IsGenericParameter);
            //}
            //catch (Exception ex)
            //{
            //    System.Console.WriteLine(ex.Message);
            //}

            //try
            //{
            //    System.Console.WriteLine("IsGenericTypeDefinition：" + type.IsGenericTypeDefinition);
            //}
            //catch (Exception ex)
            //{
            //    System.Console.WriteLine(ex.Message);
            //}

            try
            {
                System.Console.WriteLine("IsSealed：" + type.IsSealed);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            try
            {
                System.Console.WriteLine("IsAbstract：" + type.IsAbstract);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            try
            {
                System.Console.WriteLine("IsTypeDefinition：" + type.IsTypeDefinition);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            System.Console.WriteLine("!!!");

            //IsExtern
            //IsSealed
            //IsAbstract
            //IsOverride
            //IsVirtual
            //IsStatic
            //IsDefinition

            ////System.Console.WriteLine(type.IsExtern);
            //System.Console.WriteLine("IsSealed " + type.IsSealed);
            //System.Console.WriteLine("IsAbstract " + type.IsAbstract);
            ////System.Console.WriteLine(type.IsOverride);
            ////System.Console.WriteLine(type.IsVirtual);
            ////System.Console.WriteLine(type.IsStatic);
            //System.Console.WriteLine("IsGenericTypeDefinition " + type.IsGenericTypeDefinition);
            //System.Console.WriteLine("IsTypeDefinition " + type.IsTypeDefinition);

            //System.Console.WriteLine(type.IsValueType);
            ////System.Console.WriteLine(type.IsSealed);
            //System.Console.WriteLine(type.IsAbstract);
            ////System.Console.WriteLine(type.IsOverride);
            ////System.Console.WriteLine(type.IsVirtual);
            ////System.Console.WriteLine(type.IsStatic);
            //System.Console.WriteLine(type.IsDefinition());


            var MyStruct111 = typeof(MyStruct)
                .AsGeneratorType()
                .CreateInstance("aaa111")
                .AccessorSet("bbb", 888)
                .AccessorSet("ccc", DateTimeOffset.Now);

            //var MyStruct222 = typeof(MyStruct2).CreateInstance<MyStruct2>("222");

            //var myStruct = (MyStruct2)(MyStruct222);
            //System.Console.WriteLine(JsonSerializer.Serialize(MyStruct222));
            //[Serde.Json.JsonSerializer]

            var structMethod7 = (MyStruct111.TypeMeta.AccessorType.Members["StructMethod7"] as IAccessorMethodCollection).First();
            foreach (var item in structMethod7.Attributes)
            {
                var arg = item.ConstructorArguments;

                System.Console.WriteLine(item.Name + " " + arg.ElementAt(0).Value + " " + arg.ElementAt(1).Value + " " + arg.ElementAt(2).Value);
            }

            var arr0attr = structMethod7.Parameters[0].Attributes.First();
            var arg2 = arr0attr.ConstructorArguments;
            System.Console.WriteLine(arr0attr.Name + " " + arg2.ElementAt(0).Value + " " + arg2.ElementAt(1).Value + " " + arg2.ElementAt(2).Value);

            var arr0attr2 = structMethod7.Parameters[1].Attributes.First();
            var arg3 = arr0attr2.ConstructorArguments;
            System.Console.WriteLine(arr0attr2.Name + " " + arg3.ElementAt(0).Value + " " + arg3.ElementAt(1).Value + " " + arg3.ElementAt(2).Value);

            //StructMember2(ref string? a, out int* b, ref (int c1, string c2) c, out (int? c1, string? c2) d)
            //StructMember2(ref string? a, out int b, ref (int c1, string c2) c, out (int? c1, string? c2) d)
            if (MyStruct111.AccessorMethod("StructMember2", out (int c1, string c2) value222, 
                RefArg.Ref("a"), 
                RefArg.Out<int>(), 
                RefArg.Ref((777, "xzzzxx")), 
                RefArg.Out<(int?, string?)>()))
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



            System.Console.WriteLine(MyStruct111.AccessorGet<string>("aaa"));

            System.Console.WriteLine(MyStruct111.AccessorGet<int?>("bbb"));

            System.Console.WriteLine(MyStruct111.AccessorGet<DateTimeOffset?>("ccc"));

            System.Console.WriteLine(MyStruct111.AccessorGet<object>("ccc"));

            var data = typeof(ResultObject<>).AsGeneratorType(typeof(MyStruct)).CreateInstance(MyStruct111.Instance, 8888, "message", true);

            var data5 = typeof(ResultObject3<>).AsGeneratorType<MyStruct5>().CreateInstance<ResultObject3<MyStruct5>>(new MyStruct5("```5555555555555555555555~!@#"), 333, "message", true);

            var data6 = typeof(ResultObject3<>).AsGeneratorType(typeof(MyStruct6)).CreateInstance<ResultObject3<MyStruct6>>(typeof(MyStruct6).AsGeneratorType().CreateInstance<MyStruct6>("```6666666666666666666666~!@#"), 333, "message", true);

            var data7 = typeof(ResultObject3<>).AsGeneratorType(typeof(MyStruct7)).CreateInstance<ResultObject3<MyStruct7>>(new MyStruct7("```7777777777777777777777~!@#"), 333, "message", true);

            var data8 = typeof(ResultObject3<>).AsGeneratorType(typeof(MyStruct8)).CreateInstance<ResultObject3<MyStruct8>>(new MyStruct8("```8888888888888888888888~!@#"), 333, "message", true);

            var data9 = typeof(ResultObject3<>).AsGeneratorType(typeof(MyStruct9)).CreateInstance<ResultObject3<MyStruct9>>(new MyStruct9("```9999999999999999999999~!@#"), 333, "message", true);

            var data10 = typeof(ResultObject3<>).AsGeneratorType(typeof(MyStruct10)).CreateInstance<ResultObject3<MyStruct10>>(new MyStruct10("```1010101010101010101~!@#"), 333, "message", true);

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