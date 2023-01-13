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

namespace Business.SourceGenerator.Test
{
    public class UnitTest1
    {
        [Theory]
        [InlineData("ClassGeneric.cs")]
        public void ClassGenericTest(string file, bool global = false)
        {
            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "TestTemp", file);

            Debug.Assert(System.IO.File.Exists(path));

            var testCode = @"
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Business.SourceGenerator;
using Business.SourceGenerator.Meta;

namespace UnitAssembly
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            Utils.GlobalEntryAssemblyName = ""UnitAssembly"";

            var result = typeof(MyCode.ClassGeneric<string>)
                    .CreateInstance<IGeneratorAccessor>()
                    .AccessorSet<IGeneratorAccessor>(""A"", ""WWW"")
                    .AccessorSet<IGeneratorAccessor>(""B"", new Dictionary<string, string>());

            return 0;
        }

        public object Test()
        {
            Utils.GlobalEntryAssemblyName = ""UnitAssembly"";

            var result = typeof(MyCode.ClassGeneric<string>)
                    .CreateInstance<IGeneratorAccessor>()
                    .AccessorSet<IGeneratorAccessor>(""A"", ""WWW"")
                    .AccessorSet<IGeneratorAccessor>(""B"", new Dictionary<string, string>());

            return result;
        }
    }
}
";

            var source = Compilation(path, global, OutputKind.ConsoleApplication, testCode);
        }

        [Theory]
        [InlineData("ClassMember.cs")]
        public void ClassMemberTest(string file, bool global = false)
        {
            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "TestTemp", file);

            Debug.Assert(System.IO.File.Exists(path));

            var testCode = @"
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Business.SourceGenerator;
using Business.SourceGenerator.Meta;

namespace UnitAssembly
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            Utils.GlobalEntryAssemblyName = ""UnitAssembly"";

            var result = typeof(MyCode.ClassMember)
                    .CreateInstance<IGeneratorAccessor>()
                    .AccessorSet<IGeneratorAccessor>(""A"", ""WWW"")
                    .AccessorSet<IGeneratorAccessor>(""B"", new Dictionary<string, int?>());

            return 0;
        }

        public object Test()
        {
            Utils.GlobalEntryAssemblyName = ""UnitAssembly"";

            var result = typeof(MyCode.ClassMember)
                    .CreateInstance<IGeneratorAccessor>()
                    .AccessorSet<IGeneratorAccessor>(""A"", ""WWW"")
                    .AccessorSet<IGeneratorAccessor>(""B"", new Dictionary<string, int?>());

            return result;
        }
    }
}
";

            var source = Compilation(path, global, OutputKind.ConsoleApplication, testCode);
        }

        [Theory]
        [InlineData("ClassMethod.cs")]
        public void ClassMethodTest(string file, bool global = false)
        {
            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "TestTemp", file);

            Debug.Assert(System.IO.File.Exists(path));

            var source = Compilation(path, global);
        }

        [Theory]
        [InlineData("StructMember.cs")]
        public void StructMemberTest(string file, bool global = false)
        {
            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "TestTemp", file);

            Debug.Assert(System.IO.File.Exists(path));

            var source = Compilation(path, global);
        }

        [Theory]
        [InlineData("StructMethod.cs")]
        public void StructMethodTest(string file, bool global = false)
        {
            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "TestTemp", file);

            Debug.Assert(System.IO.File.Exists(path));

            var source = Compilation(path, global);
        }

        static string Compilation(string file, bool global = false, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary, string source = default)
        {
            // Create the 'input' compilation that the generator will act on
            Compilation inputCompilation = CreateCompilation(System.IO.File.ReadAllText(file), outputKind);

            if (source is not null)
            {
                inputCompilation = inputCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source));
            }

            //inputCompilation = inputCompilation.AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            //inputCompilation = inputCompilation.AddReferences(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));
            inputCompilation = inputCompilation.AddReferences(MetadataReference.CreateFromFile(typeof(Meta.Accessibility).Assembly.Location));

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
            Debug.Assert(outputCompilation.SyntaxTrees.Count() == ((source is not null) ? 3 : 2)); // we have two syntax trees, the original 'user' provided one, and the one added by the generator

            var outputDiagnostics = outputCompilation.GetDiagnostics();

            var mainResult = MainInvoke(outputCompilation);

            var methodResult = MethodInvoke(outputCompilation, "Test");

            Debug.Assert(!outputDiagnostics.Any(c => DiagnosticSeverity.Error == c.Severity), DiagnosticsFirst(outputDiagnostics));// verify the compilation with the added source has no diagnostics

            // Or we can look at the results directly:
            GeneratorDriverRunResult runResult = driver.GetRunResult();

            // The runResult contains the combined results of all generators passed to the driver
            Debug.Assert(runResult.GeneratedTrees.Length == 1);
            Debug.Assert(!runResult.Diagnostics.Any(c => DiagnosticSeverity.Error == c.Severity), DiagnosticsFirst(runResult.Diagnostics));

            // Or you can access the individual results on a by-generator basis
            GeneratorRunResult generatorResult = runResult.Results[0];
            Debug.Assert(generatorResult.Generator == generator);
            Debug.Assert(!generatorResult.Diagnostics.Any(c => DiagnosticSeverity.Error == c.Severity), DiagnosticsFirst(generatorResult.Diagnostics));

            Debug.Assert(generatorResult.GeneratedSources.Length == 1);
            Debug.Assert(generatorResult.Exception is null);

            return generatorResult.GeneratedSources.First().SourceText.ToString();
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

            var main = ((Task<int>)method.Invoke(instance, BindingFlags.InvokeMethod, Type.DefaultBinder, new object[] { Array.Empty<string>() }, null)).GetAwaiter().GetResult();

            assemblyContext.Unload();

            return main;
        }

        static object MethodInvoke(Compilation compilation, string methodName)
        {
            using var codeStream = new MemoryStream();

            var emitResult = compilation.Emit(codeStream);

            codeStream.Seek(0, SeekOrigin.Begin);

            var assemblyContext = new AssemblyLoadContext(Path.GetRandomFileName(), true);
            var assembly = assemblyContext.LoadFromStream(codeStream);

            var symbol = compilation.GetSymbolsWithName(methodName).FirstOrDefault() as IMethodSymbol;

            if (symbol is null)
            {
                return default;
            }

            var type = assembly.GetType($"{symbol.ContainingNamespace.MetadataName}.{symbol.ContainingType.MetadataName}");
            var instance = assembly.CreateInstance(type.FullName);
            var method = type.GetMethod(symbol.MetadataName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            var main = method.Invoke(instance, BindingFlags.InvokeMethod, Type.DefaultBinder, Array.Empty<object>(), null);

            assemblyContext.Unload();

            return main;
        }
    }
}
