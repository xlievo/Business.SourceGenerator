using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Business.SourceGenerator.Test
{
    public class UnitTest1
    {
        [Theory]
        [InlineData("ClassGeneric.cs")]
        public void ClassGenericTest(string file)
        {
            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "TestTemp", file);

            Debug.Assert(System.IO.File.Exists(path));

            var source = Compilation(path);
        }

        [Theory]
        [InlineData("ClassMember.cs")]
        public void ClassMemberTest(string file)
        {
            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "TestTemp", file);

            Debug.Assert(System.IO.File.Exists(path));

            var source = Compilation(path);
        }

        [Theory]
        [InlineData("ClassMethod.cs")]
        public void ClassMethodTest(string file)
        {
            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "TestTemp", file);

            Debug.Assert(System.IO.File.Exists(path));

            var source = Compilation(path);
        }

        [Theory]
        [InlineData("StructMember.cs")]
        public void StructMemberTest(string file)
        {
            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "TestTemp", file);

            Debug.Assert(System.IO.File.Exists(path));

            var source = Compilation(path);
        }

        [Theory]
        [InlineData("StructMethod.cs")]
        public void StructMethodTest(string file)
        {
            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "TestTemp", file);

            Debug.Assert(System.IO.File.Exists(path));

            var source = Compilation(path);
        }

        static string Compilation(string file)
        {
            // Create the 'input' compilation that the generator will act on
            Compilation inputCompilation = CreateCompilation(System.IO.File.ReadAllText(file));

            inputCompilation = inputCompilation.AddReferences(MetadataReference.CreateFromFile(typeof(Business.SourceGenerator.Meta.Accessibility).Assembly.Location));

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
            Debug.Assert(outputCompilation.SyntaxTrees.Count() == 2); // we have two syntax trees, the original 'user' provided one, and the one added by the generator
            var outputDiagnostics = outputCompilation.GetDiagnostics();
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

    }
}
