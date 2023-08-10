using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using Xunit;
using Business.SourceGenerator;
using Business.SourceGenerator.Meta;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Business.SourceGenerator.UnitTest;
using TypeNameFormatter;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using MyCode;
using Business.SourceGenerator.Analysis;
using System.Collections;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System.Collections.Specialized;
using static System.Runtime.CompilerServices.ConfiguredTaskAwaitable;
using static System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using static System.Runtime.CompilerServices.YieldAwaitable;
using static System.Runtime.InteropServices.ComWrappers;
using static System.Runtime.InteropServices.Marshalling.CustomMarshallerAttribute;
using static System.Runtime.InteropServices.ObjectiveC.ObjectiveCMarshal;
using static System.Text.StringBuilder;
using static System.TimeZoneInfo;

namespace Business.SourceGenerator.Test
{
    public class UnitTest1
    {
        [Theory]
        [InlineData("GetFullName2.cs")]
        public void GetFullName2Test(string file, bool global = false)
        {
            //string nullType2 = GetFormattedName(typeof(System.Threading.Tasks.ValueTask<System.Threading.Tasks.ValueTask<int?>?>?));

            //
            //System.Threading.Tasks.ValueTask<System.Collections.Generic.IDictionary<System.Threading.Tasks.ValueTask<System.Int32[]>?, System.Threading.Tasks.ValueTask<System.Int32?>?>>?

            string nullType4 = GetFormattedName(typeof(ValueTask<IDictionary<System.Nullable<ValueTask<int[]?>>, ValueTask<int?>?>?>?));
            string nullType5 = GetFormattedName(typeof(ValueTask<IDictionary<ValueTask<int[]?>?, ValueTask<int?>?>?>?), true);
            string nullType6 = GetFormattedName(typeof((string? a, int? b)?));
            string nullType7 = GetFormattedName(typeof((string? a, int? b)?), true);
            string nullType8 = GetFormattedName(typeof(int?*[]));
            string nullType9 = GetFormattedName(typeof(int?*[]), true);

            var anonymousType = new { A = "A", B = new int?(3) };
            var anonymouType = anonymousType.GetType();
            string nullType10 = GetFormattedName(anonymouType);
            string nullType11 = GetFormattedName(anonymouType, true);

            //{System.String A, System.Nullable<System.Int32> B}
            //{System.String A, System.Nullable<System.Int32> B}

            const string assemblyName = "GetFullName2Assembly";

            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "TestTemp", file);

            Debug.Assert(System.IO.File.Exists(path));

            Compilation compilation = CreateCompilation(System.IO.File.ReadAllText(path), assemblyName: assemblyName);

            var refs = System.IO.Directory.GetFiles(System.IO.Path.Combine(AppContext.BaseDirectory, "ref_NETCore_7.0"));

            var references = new List<MetadataReference>(refs.Length);
            foreach (var item in refs)
            {
                references.Add(MetadataReference.CreateFromFile(item));
            }

            compilation = compilation.AddReferences(references);

            var syntax = compilation.SyntaxTrees.FirstOrDefault();

            var model = compilation.GetSemanticModel(syntax);

            var nodes = syntax.GetRoot().DescendantNodes();

            var symbols = GetSymbol(nodes, model).ToList();

            static string typeClean(string type) => Analysis.SymbolTypeName.TypeNameClean(type, $"{assemblyName}.", "System.Collections.Generic.", "System.Collections.ObjectModel.", "System.Threading.Tasks.");

            var symbolsName0 = symbols.Select(c => Analysis.SymbolTypeName.GetFullName(c, Analysis.SymbolTypeName.GetFullNameOpt.Create(typeParameterStyle: Analysis.SymbolTypeName.TypeParameterStyle.Real))).Distinct().ToList();

            var symbolsName = symbols.Select(c =>
            {
                var name = Analysis.SymbolTypeName.GetFullNameStandardFormat(c, Analysis.SymbolTypeName.GetFullNameOpt.Create(typeParameterStyle: Analysis.SymbolTypeName.TypeParameterStyle.Name));

                if (name == "MyCode.GetFullName2<T>.AnonymousType<T2>..c2")
                {

                }

                return name;
            }).Distinct().ToList();

            var symbolsName3 = symbols.Select(c => Analysis.SymbolTypeName.GetFullName(c, Analysis.SymbolTypeName.GetFullNameOpt.Create(typeParameterStyle: Analysis.SymbolTypeName.TypeParameterStyle.FullName))).Distinct().ToList();

            //foreach (var item in symbols)
            //{
            //    if (item.ToDisplayString() == "T2?")
            //    {

            //    }
            //    var name = Analysis.Test.GetFullName2(item);
            //}

            var symbolsName2 = symbols.Select(c => Analysis.SymbolTypeName.GetFullName(c, Analysis.SymbolTypeName.GetFullNameOpt.Create(noNullableQuestionMark: true))).Distinct().ToList();

            //var symbolsName3 = symbols.Select(c => Analysis.Test.GetFullName2(c, Analysis.Test.GetFullNameOpt2.Create(noNullableQuestionMark: true, anonymousTypeStyle: Analysis.Test.AnonymousTypeStyle.Curly))).Distinct().ToList();

            //var symbolsName4 = symbols.Select(c => Analysis.Test.GetFullName2(c, Analysis.Test.GetFullNameOpt2.Create(noNullableQuestionMark: true, anonymousTypeStyle: Analysis.Test.AnonymousTypeStyle.Curly | Analysis.Test.AnonymousTypeStyle.Name))).Distinct().ToList();

            //var symbolsName44 = symbols.Select(c => Analysis.Test.GetFullName2(c, Analysis.Test.GetFullNameOpt2.Create(noNullableQuestionMark: true, anonymousTypeStyle: Analysis.Test.AnonymousTypeStyle.Curly | Analysis.Test.AnonymousTypeStyle.Type))).Distinct().ToList();

            //var symbolsName5 = symbols.Select(c => Analysis.Test.GetFullName2(c, Analysis.Test.GetFullNameOpt2.Create(noNullableQuestionMark: true, anonymousTypeStyle: Analysis.Test.AnonymousTypeStyle.Type))).Distinct().ToList();

            //var symbolsName55 = symbols.Select(c => Analysis.Test.GetFullName2(c, Analysis.Test.GetFullNameOpt2.Create(noNullableQuestionMark: true, anonymousTypeStyle: Analysis.Test.AnonymousTypeStyle.Name))).Distinct().ToList();

            //var symbolsName6 = symbols.Select(c => Analysis.Test.GetFullName2(c, Analysis.Test.GetFullNameOpt2.Create(noNullableQuestionMark: true, anonymousTypeStyle: Analysis.Test.AnonymousTypeStyle.Type | Analysis.Test.AnonymousTypeStyle.Round))).Distinct().ToList();

            //var symbolsName66 = symbols.Select(c => Analysis.Test.GetFullName2(c, Analysis.Test.GetFullNameOpt2.Create(noNullableQuestionMark: true, anonymousTypeStyle: Analysis.Test.AnonymousTypeStyle.Name | Analysis.Test.AnonymousTypeStyle.Round))).Distinct().ToList();

            //var symbolsName666 = symbols.Select(c => Analysis.Test.GetFullName2(c, Analysis.Test.GetFullNameOpt2.Create(noNullableQuestionMark: true, anonymousTypeStyle: Analysis.Test.AnonymousTypeStyle.Round))).Distinct().ToList();

            var testArgs = Analysis.SymbolTypeName.GetFullName(symbols[35], Analysis.SymbolTypeName.GetFullNameOpt.Create(captureStyle: Analysis.SymbolTypeName.CaptureStyle.Args));
            var testArgs2 = Analysis.SymbolTypeName.GetFullName(symbols[35]);
            //[35] = {(T a, int b).b}
            //System.ValueTuple<T, System.Int32>.b

            //<MyCode.GetFullName2<T>.T[], System.Int32[]>
            //System.ValueTuple<MyCode.GetFullName2<T>.T[], System.Int32[]>

            //<MyCode.GetFullName2<T>.T?, System.Int32?>
            //<MyCode.GetFullName2<T>.T?, System.Int32?>
            //<MyCode.GetFullName2<T>.T?, System.Int32?>
            //<MyCode.GetFullName2<T>.T?, System.Int32?>

            //<<System.ValueTuple<MyCode.GetFullName2<T>.T?, System.Int32?>>
            //System.ValueTuple<MyCode.GetFullName2<T>.T, System.Int32>?
            //System.ValueTuple<MyCode.GetFullName2<T>.T?, System.Int32?>?

            //<T>.AnonymousType<T2>(<T>.AnonymousType<T2>.T2[], <<T>.T>, <<<System.DateOnly>, <<<System.Int32>>>>>)

            //MyCode.GetFullName2<T>.AnonymousType<T2>(MyCode.GetFullName2<T>.AnonymousType<T2>.T2[], MyCode.GetFullName2<T>.T?, System.ValueTuple<System.DateOnly?, System.Threading.Tasks.ValueTask<System.Int32?>?>?)

            //MyCode.GetFullName2<T>.AnonymousType<T2>(MyCode.GetFullName2<T>.AnonymousType<T2>.T2[], MyCode.GetFullName2<T>.T?, System.ValueTuple<System.DateOnly?, System.Threading.Tasks.ValueTask<System.Int32?>?>?)

            Debug.Assert(symbolsName.Contains(nullType4));
            Debug.Assert(symbolsName3.Contains(nullType4));
            Debug.Assert(symbolsName2.Contains(nullType5));
            Debug.Assert(symbolsName.Contains(nullType6));
            Debug.Assert(symbolsName2.Contains(nullType7));
            Debug.Assert(symbolsName.Contains(nullType8));
            Debug.Assert(symbolsName2.Contains(nullType9));
            Debug.Assert(symbolsName.Contains(nullType10));
            Debug.Assert(symbolsName2.Contains(nullType11));

            //foreach (var item in symbols)
            //{
            //    var name = Analysis.Test.GetFullName2(item);
            //}
        }

        static IEnumerable<ISymbol> GetSymbol(IEnumerable<SyntaxNode> nodes, SemanticModel model)
        {
            foreach (var item in nodes)
            {
                switch (item.RawKind)
                {
                    case (int)SyntaxKind.InterfaceDeclaration:
                    case (int)SyntaxKind.ClassDeclaration:
                    case (int)SyntaxKind.StructDeclaration:
                    case (int)SyntaxKind.PropertyDeclaration:
                    case (int)SyntaxKind.FieldDeclaration:
                    case (int)SyntaxKind.MethodDeclaration:
                    case (int)SyntaxKind.LocalFunctionStatement:
                    case (int)SyntaxKind.ContinueStatement:
                    case (int)SyntaxKind.ConstructorConstraint:
                    case (int)SyntaxKind.ConstructorDeclaration:
                    case (int)SyntaxKind.EnumDeclaration:
                    case (int)SyntaxKind.IndexerDeclaration:
                    case (int)SyntaxKind.VariableDeclarator:
                    case (int)SyntaxKind.VariableDeclaration:
                    case (int)SyntaxKind.BaseList:
                    case (int)SyntaxKind.SimpleBaseType:
                    case (int)SyntaxKind.TypeOfExpression:
                    case (int)SyntaxKind.CastExpression:
                    case (int)SyntaxKind.PredefinedType:
                    case (int)SyntaxKind.DivideExpression:
                    case (int)SyntaxKind.ConditionalExpression:
                    case (int)SyntaxKind.DefaultExpression:

                    case (int)SyntaxKind.QualifiedName:
                    case (int)SyntaxKind.GenericName:
                    case (int)SyntaxKind.TypeParameter:
                    case (int)SyntaxKind.TypeArgumentList:
                    case (int)SyntaxKind.Argument:
                    case (int)SyntaxKind.Parameter:
                    case (int)SyntaxKind.InvocationExpression:
                    case (int)SyntaxKind.IdentifierName:
                    case (int)SyntaxKind.Attribute:

                    case (int)SyntaxKind.AsExpression:
                    case (int)SyntaxKind.ObjectCreationExpression:
                    case (int)SyntaxKind.EqualsValueClause:
                    case (int)SyntaxKind.SimpleMemberAccessExpression:
                    case (int)SyntaxKind.SimpleAssignmentExpression:
                    case (int)SyntaxKind.NullLiteralExpression:
                    case (int)SyntaxKind.ElementAccessExpression:
                    case (int)SyntaxKind.NullableType:
                    case (int)SyntaxKind.TupleType:
                        {
                            var declared = model.GetDeclaredSymbol(item) ?? model.GetTypeInfo(item).Type;

                            if (null == declared || SymbolKind.ErrorType == declared.Kind)
                            {
                                continue;
                            }

                            if (declared is ITypeSymbol typeSymbol && Microsoft.CodeAnalysis.SpecialType.System_Void == typeSymbol.SpecialType)
                            {
                                continue;
                            }

                            yield return declared;
                        }
                        break;
                    default: break;
                }
            }
        }

        static string GetFormattedName(Type type, bool noNull = false)
        {
            var opt = TypeNameFormatter.TypeNameFormatOptions.Namespaces | TypeNameFormatter.TypeNameFormatOptions.NoKeywords | TypeNameFormatter.TypeNameFormatOptions.NoTuple;
            //| TypeNameFormatter.TypeNameFormatOptions.NoAnonymousTypes;

            //if (noAnonymous)
            //{
            //opt |= TypeNameFormatter.TypeNameFormatOptions.NoAnonymousTypes;
            //}

            if (noNull)
            {
                opt |= TypeNameFormatter.TypeNameFormatOptions.NoNullableQuestionMark;
            }
            return TypeName.GetFormattedName(type, opt);
        }

        [Theory]
        [InlineData("Nullable.cs")]
        public void NullTest(string file, bool global = false)
        {
            string dateTimeOffsetType = GetFormattedName(typeof(DateTimeOffset?));
            string intType = GetFormattedName(typeof(int?));
            string valueTaskType = GetFormattedName(typeof(ValueTask<int>));

            const string assemblyName = "NullableAssembly";

            var testCode = $@"
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Business.SourceGenerator;
using Business.SourceGenerator.Meta;

namespace UnitAssembly
{{
    internal class Program
    {{
        static async Task<int> Main(string[] args)
        {{
            return 0;
        }}

        public object Test()
        {{
            {assemblyName}.BusinessSourceGenerator.Generator.SetGeneratorCode();

            Business.SourceGenerator.Utils.GeneratorCode.GeneratorType.TryGetValue(typeof({valueTaskType}), out Business.SourceGenerator.Meta.TypeMeta value);

            return value;
        }}
    }}
}}
";

            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "TestTemp", file);

            Debug.Assert(System.IO.File.Exists(path));

            var compileResult = Compilation(path, global, OutputKind.ConsoleApplication, assemblyName, testCode);

            var source = compileResult.GeneratorSource;

            dynamic testResult = MethodInvoke(compileResult.Compilation, "UnitAssembly.Program.Test");

            //Debug.Assert(testResult is not null);

            //Debug.Assert((bool)"WWW".Equals(testResult.A));

            //Debug.Assert((bool)typeof(Dictionary<string, string>).Equals(testResult.B.GetType()));
        }

        [Theory]
        [InlineData("ClassGeneric.cs")]
        public void ClassGenericTest(string file, bool global = false)
        {
            const string assemblyName = "ClassGenericAssembly";

            var testCode = $@"
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Business.SourceGenerator;
using Business.SourceGenerator.Meta;

namespace UnitAssembly
{{
    internal class Program
    {{
        static async Task<int> Main(string[] args)
        {{
            {assemblyName}.BusinessSourceGenerator.Generator.SetGeneratorCode();

            var result = typeof(MyCode.ClassGeneric<string>)
                    .AsGeneratorType()
                    .CreateInstance()
                    .AccessorSet(""A"", ""WWW"")
                    .AccessorSet(""B"", new Dictionary<string, string>());

            return 0;
        }}

        public object Test()
        {{
            {assemblyName}.BusinessSourceGenerator.Generator.SetGeneratorCode();

            var result = typeof(MyCode.ClassGeneric<string>)
                    .AsGeneratorType()
                    .CreateInstance()
                    .AccessorSet(""A"", ""WWW"")
                    .AccessorSet<MyCode.ClassGeneric<string>>(""B"", new Dictionary<string, string>());

            return result;
        }}
    }}
}}
";

            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "TestTemp", file);

            Debug.Assert(System.IO.File.Exists(path));

            var iResult = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestTemp", "IResult.cs"));

            var compileResult = Compilation(path, global, OutputKind.ConsoleApplication, assemblyName, testCode, iResult);
            //System.Threading.Tasks.ValueTask
            var source = compileResult.GeneratorSource;

            MainInvoke(compileResult.Compilation);

            dynamic testResult = MethodInvoke(compileResult.Compilation, "UnitAssembly.Program.Test");


            Debug.Assert(testResult is not null);

            Debug.Assert((bool)"WWW".Equals(testResult.A));

            Debug.Assert((bool)typeof(Dictionary<string, string>).Equals(testResult.B.GetType()));
        }

        [Theory]
        [InlineData("ClassMember.cs")]
        public void ClassMemberTest(string file, bool global = false)
        {
            const string assemblyName = "ClassMemberAssembly";

            var testCode = $@"
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Business.SourceGenerator;
using Business.SourceGenerator.Meta;

namespace UnitAssembly
{{
    internal class Program
    {{
        static async Task<int> Main(string[] args) => 0;

        public object Test()
        {{
            {assemblyName}.BusinessSourceGenerator.Generator.SetGeneratorCode();

            return typeof(MyCode.ClassMember)
                    .AsGeneratorType()
                    .CreateInstance()
                    .AccessorSet(""A"", ""WWW3"")
                    .AccessorSet<MyCode.ClassMember>(""B"", new Dictionary<string, int?>());
        }}
    }}
}}
";

            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "TestTemp", file);

            Debug.Assert(System.IO.File.Exists(path));

            var compileResult = Compilation(path, global, OutputKind.ConsoleApplication, assemblyName, testCode);

            var source = compileResult.GeneratorSource;

            dynamic testResult = MethodInvoke(compileResult.Compilation, "UnitAssembly.Program.Test");

            Debug.Assert(testResult is not null);

            Debug.Assert((bool)"WWW3".Equals(testResult.A));

            Debug.Assert((bool)typeof(Dictionary<string, int?>).Equals(testResult.B.GetType()));
        }

        [Theory]
        [InlineData("StructMember.cs")]
        public void StructMemberTest(string file, bool global = false)
        {
            const string assemblyName = "StructMemberAssembly";

            var testCode = $@"
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Business.SourceGenerator;
using Business.SourceGenerator.Meta;

namespace UnitAssembly
{{
    internal class Program
    {{
        static async Task<int> Main(string[] args) => 0;

        public object Test()
        {{
            {assemblyName}.BusinessSourceGenerator.Generator.SetGeneratorCode();

            return typeof(MyCode.StructMember)
                .AsGeneratorType()
                .CreateInstance()
                .AccessorSet(""A"", ""WWW2"")
                .AccessorSet<MyCode.StructMember>(""B"", new Dictionary<string, int?>());
        }}
    }}
}}
";

            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "TestTemp", file);

            Debug.Assert(System.IO.File.Exists(path));

            var compileResult = Compilation(path, global, OutputKind.ConsoleApplication, assemblyName, testCode);

            var source = compileResult.GeneratorSource;

            dynamic testResult = MethodInvoke(compileResult.Compilation, "UnitAssembly.Program.Test");

            Debug.Assert(testResult is not null);

            Debug.Assert((bool)"WWW2".Equals(testResult.A));

            Debug.Assert((bool)typeof(Dictionary<string, int?>).Equals(testResult.B.GetType()));
        }

        [Theory]
        [InlineData("MethodInvoke.cs")]
        public void MethodInvokeTest(string file, bool global = false)
        {
            const string assemblyName = "MethodInvokeAssembly";

            var testCode = $@"
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Business.SourceGenerator;
using Business.SourceGenerator.Meta;

namespace UnitAssembly
{{
    internal class Program
    {{
        static async Task<int> Main(string[] args) => 0;

        public object StructMethod3()
        {{
            {assemblyName}.BusinessSourceGenerator.Generator.SetGeneratorCode();

            var acc = typeof(MyCode.MethodInvoke<System.IO.MemoryStream>)
                .AsGeneratorType()
                .CreateInstance();
            
            (int? c1, string? c2) c = (33, ""66"");
            (int? c1, string? c2) d = (44, ""88"");
            
            //StructMethod3(ref string? a, out int? b, ref (int? c1, string? c2) c, out (int? c1, string? c2) d)
            if(acc.AccessorMethod(""StructMethod3"", out object value, 
                RefArg.Ref(string.Empty), 
                RefArg.Out<int?>(),
                RefArg.Ref(c),
                RefArg.Out<(int? c1, string? c2)>()
                ))
            {{
                return value;
            }}

            return default;
        }}

        public object StructMethod4()
        {{
            {assemblyName}.BusinessSourceGenerator.Generator.SetGeneratorCode();

            var acc = typeof(MyCode.MethodInvoke<System.IO.MemoryStream>)
                .AsGeneratorType()    
                .CreateInstance();
            
            (int? c1, string? c2) c = (33, ""66"");
            (int? c1, string? c2) d = (44, ""88"");

            //StructMethod4(ref string? a, out int? b, ref (int? c1, string? c2) c, out (int? c1, string? c2) d)
            if(acc.AccessorMethod(""StructMethod4"", out object value,
                RefArg.Ref(string.Empty), 
                RefArg.Out<int?>(),
                RefArg.Ref(c),
                RefArg.Out<(int? c1, string? c2)>()))
            {{
                return value;
            }}

            return default;
        }}

        public object StructMethod5()
        {{
            {assemblyName}.BusinessSourceGenerator.Generator.SetGeneratorCode();

            var acc = typeof(MyCode.MethodInvoke<System.IO.MemoryStream>)
                .AsGeneratorType()
                .CreateInstance();
            
            (int? c1, string? c2) c = (33, ""66"");
            (int? c1, string? c2) d = (44, ""88"");

            //StructMethod5(ref string? a, out int? b, ref (int? c1, string? c2) c, out (int? c1, string? c2) d)
            if(acc.AccessorMethod(""StructMethod5"", out object value,
                RefArg.Ref(string.Empty), 
                RefArg.Out<int?>(),
                RefArg.Ref(c),
                RefArg.Out<(int? c1, string? c2)>()
                ))
            {{
                return value;
            }}

            return default;
        }}

        public async ValueTask<object> StructMethod6()
        {{
            {assemblyName}.BusinessSourceGenerator.Generator.SetGeneratorCode();

            var acc = typeof(MyCode.MethodInvoke<System.IO.MemoryStream>)
                .AsGeneratorType()
                .CreateInstance();

            (int? c1, string? c2) c = (33, ""66"");
            (int? c1, string? c2) d = (44, ""88"");

            //StructMethod6(ref string? a, out int? b, ref (int? c1, string? c2) c, out (int? c1, string? c2) d)
            return await acc.AccessorMethodAsync(""StructMethod6"", 
                RefArg.Ref(string.Empty), 
                RefArg.Out<int?>(),
                RefArg.Ref(c),
                RefArg.Out<(int? c1, string? c2)>()
                );
        }}

        public async ValueTask<object> StructMethod7()
        {{
            {assemblyName}.BusinessSourceGenerator.Generator.SetGeneratorCode();

            var acc = typeof(MyCode.MethodInvoke<System.IO.MemoryStream>)
                .AsGeneratorType()
                .CreateInstance();

            var aaa = new System.IO.MemoryStream();
            var bbb = new List<string>();

            (DateTimeOffset? c1, IList<string>? c2) c = (DateTimeOffset.Now, bbb);
            (int? c1, string? c2) d = (44, ""88"");

            var structMember2 = new StructMember2 {{ A = 111 }};

            /*
            StructMethod7<T2>(
            ref T2? a,
            out int? b,
            ref (DateTimeOffset? c1, T2? c2) c,
            out (int? c1, string? c2) d,
            ref StructMember2 structMember21,
            string e = ""eee"",
            System.Single f = default,
            StructMember2 structMember2 = default,
            object ddf = default, params int[] ppp)
            */

            var args = new object[] 
            {{ 
                RefArg.Ref(bbb), 
                RefArg.Out<int?>(),
                RefArg.Ref(c), 
                RefArg.Out<(int? c1, string? c2)>(),
                RefArg.Ref(structMember2), 
                ""qqq"", 
                55f 
            }};

            await acc.AccessorMethodAsync(""StructMethod7"", args);

            //structMember2 = (StructMember2)args[2];

            return args[3];
        }}
    }}
}}
";

            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "TestTemp", file);

            Debug.Assert(System.IO.File.Exists(path));

            var iResult = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestTemp", "IResult.cs"));

            var compileResult = Compilation(path, global, OutputKind.ConsoleApplication, assemblyName, testCode, iResult);

            var source = compileResult.GeneratorSource;

            dynamic testResult = MethodInvoke(compileResult.Compilation, "UnitAssembly.Program.StructMethod3");
            Debug.Assert(testResult is not null);
            Debug.Assert((bool)6789.Equals(testResult.Item1));
            Debug.Assert((bool)"66".Equals(testResult.Item2));

            testResult = MethodInvoke(compileResult.Compilation, "UnitAssembly.Program.StructMethod4");
            Debug.Assert(testResult is not null);
            Debug.Assert(testResult is Task);

            testResult = MethodInvoke(compileResult.Compilation, "UnitAssembly.Program.StructMethod5");
            Debug.Assert(testResult is not null);
            Debug.Assert(testResult is ValueTask);

            testResult = MethodInvoke(compileResult.Compilation, "UnitAssembly.Program.StructMethod6");
            Debug.Assert(testResult is not null);
            var testResult2 = testResult.GetAwaiter().GetResult();
            Debug.Assert((bool)33.Equals(testResult2.Item1));
            Debug.Assert((bool)"66".Equals(testResult2.Item2));

            dynamic testResult22 = MethodInvoke(compileResult.Compilation, "UnitAssembly.Program.StructMethod7");
            Debug.Assert(testResult22 is not null);
            var testResult222 = testResult22.GetAwaiter().GetResult();

            //Debug.Assert((bool)6789.Equals(testResult2.Item1));
            //Debug.Assert((bool)"66".Equals(testResult2.Item2));
        }

        [Theory]
        [InlineData("Refs.cs")]
        public void RefsTest(string file, bool global = false)
        {
            const string assemblyName = "RefsAssembly";

            var testCode = $@"
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Business.SourceGenerator;
using Business.SourceGenerator.Meta;

namespace UnitAssembly
{{
    internal class Program
    {{
        static async Task<int> Main(string[] args)
        {{
            return 0;
        }}

        public object Test()
        {{
            {assemblyName}.BusinessSourceGenerator.Generator.SetGeneratorCode();

            //var result = typeof(MyCode.ClassGeneric<string>)
            //    .AsGeneratorType()
            //    .CreateInstance()
            //    .AccessorSet(""A"", ""WWW"")
            //    .AccessorSet<MyCode.ClassGeneric<string>>(""B"", new Dictionary<string, string>());

            //return result;

            //Method0(string ooo, out int bbb, ref int aaa, out int ddd, string www = ""sss"", params string[] ccc)
            var args = new object[] {{ ""yyy"", RefArg.Out<int>(), RefArg.Ref(555), RefArg.Out<int>(), ""www"" }};

            //Refs(ref int a, out System.DateTimeOffset b)
            var args2 = new object[] {{ RefArg.Ref(777), RefArg.Out<System.DateTimeOffset>() }};

            var success = typeof(MyCode.Refs)
                .AsGeneratorType()
                .CreateInstance(args2)
                .AccessorMethod(""Method0"", out ValueTask<dynamic> result, args);

            return args;
            
            //return typeof(Business.SourceGenerator.Test.ResultObject3<object>).CreateInstance<IGeneratorAccessor>();

            return 0;
        }}
    }}
}}
";


            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "TestTemp", file);

            Debug.Assert(System.IO.File.Exists(path));

            var iResult = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestTemp", "IResult.cs"));

            var compileResult = Compilation(path, global, OutputKind.ConsoleApplication, assemblyName, testCode);
            //System.Threading.Tasks.ValueTask
            var source = compileResult.GeneratorSource;

            var testResult = MethodInvoke(compileResult.Compilation, "UnitAssembly.Program.Test");


            Debug.Assert(testResult is not null);

            //Debug.Assert((bool)"WWW".Equals(testResult.A));

            //Debug.Assert((bool)typeof(Dictionary<string, string>).Equals(testResult.B.GetType()));
        }

        [Theory]
        [InlineData("TypeInfo.cs")]
        public void TypeInfoTest(string file, bool global = false)
        {
            const string assemblyName = "TypeInfoAssembly";

            var testCode = $@"
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Business.SourceGenerator;
using Business.SourceGenerator.Meta;

namespace UnitAssembly
{{
    internal class Program
    {{
        static async Task<int> Main(string[] args)
        {{
            return 0;
        }}

        public object Test()
        {{
            {assemblyName}.BusinessSourceGenerator.Generator.SetGeneratorCode();

            var result = typeof(MyCode.MyStruct<>)
                .AsGeneratorType(typeof(MyCode.MyStruct<List<int>>))
                .CreateInstance<MyCode.MyStruct<MyCode.MyStruct<List<int>>>>(""666"");

            //var result = typeof(MyCode.TypeInfo<Func<string, bool?>>)
            //        .CreateInstance<IGeneratorAccessor>();

            return result;
        }}
    }}
}}
";

            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "TestTemp", file);

            Debug.Assert(System.IO.File.Exists(path));

            var iResult = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestTemp", "IResult.cs"));

            var compileResult = Compilation(path, global, OutputKind.ConsoleApplication, assemblyName, testCode, iResult);
            //System.Threading.Tasks.ValueTask
            var source = compileResult.GeneratorSource;

            var testResult = MethodInvoke(compileResult.Compilation, "UnitAssembly.Program.Test");


            Debug.Assert(testResult is not null);

            //Debug.Assert((bool)"WWW".Equals(testResult.A));

            //Debug.Assert((bool)typeof(Dictionary<string, string>).Equals(testResult.B.GetType()));
        }

        [Theory]
        [InlineData("TypeFull.cs")]
        public void TypeFullTest(string file, bool global = false)
        {
            System.TypedReference ddd = default;
            System.Reflection.FieldInfo ddd2 = default;
            //System.Reflection.FieldInfo vvv2 = default;
            //var dd = typeof(global::System.ValueTuple<global::System.Object, global::System.Object, global::System.Object, global::System.Object, global::System.Object, global::System.Object, global::System.Object, global::System.Object>);
            //var sss = new System_Nullable_System_DateOnly__Type();

            var types = new List<Type>();

            var ass = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (var item in ass)
            {
                foreach (var type in item.GetExportedTypes())
                {
                    if (!type.IsAbstract && !type.IsEnum && !type.IsCOMObject && !type.IsGenericType && !type.IsGenericParameter && !type.IsGenericMethodParameter && !typeof(void).Equals(type) && type.IsPublic)
                    {
                        if (type.FullName.StartsWith("Microsoft") || type.FullName.StartsWith("System.ComponentModel") || type.FullName.StartsWith("System.Diagnostics") || type.FullName.StartsWith("Newtonsoft"))
                        {
                            continue;
                        }

                        types.Add(type);
                    }
                }
            }

            //System.Runtime.CompilerServices.YieldAwaitable

            //var types2 = typeof(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter);

            //var nnn = types2.FullName;

            var tt = types.Select(c => $"typeof({c.FullName.Replace('+', '.')})").Take(800).ToArray();

            var ts = string.Join(", ", tt);

            const string assemblyName = "TypeFullAssembly";
            //System.Half
            var testCode = $@"
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Business.SourceGenerator;
using Business.SourceGenerator.Meta;

namespace UnitAssembly
{{
    internal class Program
    {{
        //typeof(System.Half)
        //Type[] types = new Type[] {{ {ts} }};

        static async Task<int> Main(string[] args)
        {{
            return 0;
        }}

        public object Test()
        {{
            {assemblyName}.BusinessSourceGenerator.Generator.SetGeneratorCode();

            // public System.Threading.Tasks.ValueTask<T2> StructMethod<T2>(string? a, ref T2 b, out (int? c1, string? c2) c)
             var args = new object[] {{ ""yyy"", RefArg.Out<int>(), RefArg.Ref(555), RefArg.Out<int>(), ""www"" }};

            //out System.Threading.Tasks.ValueTask<IList<int>> value,

            //<MyCode.TypeFull<MyCode.TypeFull<List<int>>>>
            var result = typeof(MyCode.TypeFull<>)
                    .AsGeneratorType(typeof(MyCode.TypeFull<List<int>>))
                    .CreateInstance(""666"")
                    .AccessorSet(""A"", ""WWW2"")
                    //.AccessorGet<string>(""A"");
                    //.AccessorMethod(""StructMethod"", out System.Threading.Tasks.ValueTask<IList<int>> value,
                    //    string.Empty, 
                    //    RefArg.Ref(new List<int> {{ 1, 2, 3 }}),
                    //    RefArg.Out<(int? c1, string? c2)>()
                    //    );
                    .AccessorMethodAsync(""StructMethod"", 
                        string.Empty, 
                        RefArg.Ref(new List<int> {{ 1, 2, 3 }}),
                        RefArg.Out<(int? c1, string? c2)>()
                        );

            //MyCode.TypeFull<int>.C2 = 555;
            //typeof(MyCode.TypeFull<int>).AsGeneratorType().AccessorSet(""C2"", 666);
            //return MyCode.TypeFull<int>.C2;

            return result;
        }}
    }}
}}
";
            //typeof(MyCode.TypeFull<>).AsGeneratorType();

            //<MyCode.TypeFull<MyCode.TypeFull<List<int>>>>
            //<MyCode.TypeFull<MyCode.TypeFull<List<int>>>>
            //MyCode.TypeFull<int>.C2 = 555;
            //typeof(MyCode.TypeFull<int>).AsGeneratorType().AccessorSet("C2", 666);
            //new MyCode.TypeFull<int>().AsGeneratorType().AccessorSet("C2", 777);
            //var result = typeof(MyCode.TypeFull<>)
            //       .AsGeneratorType(typeof(MyCode.TypeFull<List<int>>))
            //       .CreateInstance("666")
            //       .AccessorSet("A", "WWW")
            //       .AccessorGet<string>("A");
            //.AccessorMethod("StructMethod",
            //    string.Empty,
            //    RefArg.Ref(new List<int>()),
            //    RefArg.Out<(int? c1, string? c2)>()
            //    );

            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "TestTemp", file);

            Debug.Assert(System.IO.File.Exists(path));

            var iResult = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestTemp", "IResult.cs"));

            var compileResult = Compilation(path, global, OutputKind.ConsoleApplication, assemblyName, testCode, iResult);
            //System.Threading.Tasks.ValueTask
            var source = compileResult.GeneratorSource;

            dynamic testResult = MethodInvoke(compileResult.Compilation, "UnitAssembly.Program.Test");

            //var testResult2 = testResult.GetAwaiter().GetResult();

            Debug.Assert(testResult is not null);

            //Debug.Assert((bool)"WWW".Equals(testResult.A));

            //Debug.Assert((bool)typeof(Dictionary<string, string>).Equals(testResult.B.GetType()));
        }

        static (Compilation Compilation, string GeneratorSource) Compilation(string file, bool global = false, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary, string assemblyName = "UnitAssembly", params string[] source)
        {
            // Create the 'input' compilation that the generator will act on
            Compilation inputCompilation = CreateCompilation(System.IO.File.ReadAllText(file), outputKind, assemblyName);

            if (source is not null)
            {
                //inputCompilation = inputCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source));

                foreach (var item in source)
                {
                    inputCompilation = inputCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(item));
                }
            }

            //inputCompilation = inputCompilation.AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            //inputCompilation = inputCompilation.AddReferences(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));
            inputCompilation = inputCompilation.AddReferences(MetadataReference.CreateFromFile(typeof(Meta.IAccessor).Assembly.Location));

            var refs = System.IO.Directory.GetFiles(System.IO.Path.Combine(AppContext.BaseDirectory, "ref_NETCore_7.0"));

            var references = new List<MetadataReference>(refs.Length);
            foreach (var item in refs)
            {
                references.Add(MetadataReference.CreateFromFile(item));
            }

            inputCompilation = inputCompilation.AddReferences(references);

            // directly create an instance of the generator
            // (Note: in the compiler this is loaded from an assembly, and created via reflection at runtime)
            Generator generator = new Generator();

            // Create the driver that will control the generation, passing in our generator
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            // Run the generation pass
            // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            // We can now assert things about the resulting compilation:
            Debug.Assert(!diagnostics.Any(c => DiagnosticSeverity.Error == c.Severity), DiagnosticsFirst(diagnostics)); // there were no diagnostics created by the generators
            //Debug.Assert(outputCompilation.SyntaxTrees.Count() == ((source is not null) ? 3 : 2)); // we have two syntax trees, the original 'user' provided one, and the one added by the generator

            var outputDiagnostics = outputCompilation.GetDiagnostics();

            var error = outputDiagnostics.Where(c => DiagnosticSeverity.Error == c.Severity).ToArray();

            var eee = error.GroupBy(c => c.Id).ToArray();

            Debug.Assert(!error.Any(), DiagnosticsFirst(outputDiagnostics));// verify the compilation with the added source has no diagnostics

            // Or we can look at the results directly:
            GeneratorDriverRunResult runResult = driver.GetRunResult();

            // The runResult contains the combined results of all generators passed to the driver
            //Debug.Assert(runResult.GeneratedTrees.Length == 1);
            Debug.Assert(!runResult.Diagnostics.Any(c => DiagnosticSeverity.Error == c.Severity), DiagnosticsFirst(runResult.Diagnostics));

            // Or you can access the individual results on a by-generator basis
            GeneratorRunResult generatorResult = runResult.Results[0];
            Debug.Assert(generatorResult.Generator == generator);
            Debug.Assert(!generatorResult.Diagnostics.Any(c => DiagnosticSeverity.Error == c.Severity), DiagnosticsFirst(generatorResult.Diagnostics));

            //Debug.Assert(generatorResult.GeneratedSources.Length == 1);
            Debug.Assert(generatorResult.Exception is null);

            var debugSources = generatorResult.GeneratedSources.FirstOrDefault().SourceText.ToString();
            return (outputCompilation, generatorResult.GeneratedSources.First().SourceText.ToString());
        }

        static string DiagnosticsFirst(ImmutableArray<Diagnostic> diagnostics, DiagnosticSeverity severity = DiagnosticSeverity.Error) => diagnostics.FirstOrDefault(c => severity == c.Severity)?.GetMessage();

        private static Compilation CreateCompilation(string source, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary, string assemblyName = "UnitAssembly")
            => CSharpCompilation.Create(assemblyName,
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(outputKind));

        static int MainInvoke(Compilation compilation)
        {
            using var codeStream = new MemoryStream();

            var emitResult = compilation.Emit(codeStream);

            codeStream.Seek(0, SeekOrigin.Begin);

            var assemblyContext = new AssemblyLoadContext(Path.GetRandomFileName(), true);
            var assembly = assemblyContext.LoadFromStream(codeStream);

            var entryPoint = compilation.GetEntryPoint(default);

            if (entryPoint is null)
            {
                return -1;
            }

            var type = assembly.GetType($"{entryPoint.ContainingNamespace.MetadataName}.{entryPoint.ContainingType.MetadataName}");
            var instance = assembly.CreateInstance(type.FullName);
            var method = type.GetMethod(entryPoint.MetadataName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            var result = ((Task<int>)method.Invoke(instance, BindingFlags.InvokeMethod, Type.DefaultBinder, new object[] { Array.Empty<string>() }, null)).GetAwaiter().GetResult();

            assemblyContext.Unload();

            return result;
        }

        static object MethodInvoke(Compilation compilation, string methodName)
        {
            using var codeStream = new MemoryStream();

            var emitResult = compilation.Emit(codeStream);

            codeStream.Seek(0, SeekOrigin.Begin);

            var assemblyContext = new AssemblyLoadContext(Path.GetRandomFileName(), true);
            var assembly = assemblyContext.LoadFromStream(codeStream);

            var sp = methodName.Split('.');
            var nameOpt = new FullName.GetFullNameOpt(true);
            var symbol = compilation.GetSymbolsWithName(sp[sp.Length - 1]).FirstOrDefault(c => FullName.GetFullName(c, nameOpt) == methodName);

            if (symbol is null)
            {
                return default;
            }

            var type = assembly.GetType($"{symbol.ContainingNamespace.MetadataName}.{symbol.ContainingType.MetadataName}");
            var instance = assembly.CreateInstance(type.FullName);
            var method = type.GetMethod(symbol.MetadataName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            var result = method.Invoke(instance, BindingFlags.InvokeMethod, Type.DefaultBinder, Array.Empty<object>(), null);

            assemblyContext.Unload();

            return result;
        }
    }
}
