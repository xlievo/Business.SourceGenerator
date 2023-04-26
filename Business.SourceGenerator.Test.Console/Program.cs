﻿using Business.SourceGenerator;
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
using System.Threading.Tasks;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace Business.SourceGenerator.Test.Console
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            BusinessSourceGenerator.Generator.SetGeneratorCode();

            var result = typeof(MyCode.ClassGeneric<string>)
                    .CreateInstance<IGeneratorAccessor>()
                    .AccessorSet<IGeneratorAccessor>("A", "WWW")
                    .AccessorSet<IGeneratorAccessor>("B", new Dictionary<string, string>());

            var MyStruct111 = typeof(MyStruct)
                .CreateInstance<IGeneratorAccessor>("aaa111")
                .AccessorSet<IGeneratorAccessor>("bbb", 888)
                .AccessorSet<IGeneratorAccessor>("ccc", DateTimeOffset.Now);

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