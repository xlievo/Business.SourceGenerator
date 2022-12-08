using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Xunit;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Business.SourceGenerator.Test
{
    public class UnitTest1
    {
        [Fact]
        public void SimpleGeneratorTest()
        {
            // Create the 'input' compilation that the generator will act on
            Compilation inputCompilation = CreateCompilation(@"
namespace MyCode
{
    public class TypeTarget
    {
        public string A { get; set; }

        public int B { get; set; }

        public static DateTime? C;

        public static string? D;

        public ValueTask E(dynamic a, int b = 2, params object[] args)
        {
            return ValueTask.CompletedTask;
        }
    }
}
");
            inputCompilation = inputCompilation.AddReferences(MetadataReference.CreateFromFile(typeof(Business.SourceGenerator.Meta.Accessibility).Assembly.Location));

            //var refs = System.IO.Directory.GetFiles(System.IO.Path.Combine("D:\\Repos\\Business.SourceGenerator\\Business.SourceGenerator.Test\\", "ref"));

            //var references = new List<MetadataReference>(refs.Length);
            //foreach (var item in refs)
            //{
            //    references.Add(MetadataReference.CreateFromFile(item));
            //}

            //inputCompilation = inputCompilation.AddReferences(references);

            // directly create an instance of the generator
            // (Note: in the compiler this is loaded from an assembly, and created via reflection at runtime)
            Generator generator = new Generator();

            // Create the driver that will control the generation, passing in our generator
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            // Run the generation pass
            // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            // We can now assert things about the resulting compilation:
            Debug.Assert(diagnostics.IsEmpty); // there were no diagnostics created by the generators
            Debug.Assert(outputCompilation.SyntaxTrees.Count() == 2); // we have two syntax trees, the original 'user' provided one, and the one added by the generator
            //var dias = outputCompilation.GetDiagnostics();
            //Debug.Assert(outputCompilation.GetDiagnostics().IsEmpty); // verify the compilation with the added source has no diagnostics

            // Or we can look at the results directly:
            GeneratorDriverRunResult runResult = driver.GetRunResult();

            // The runResult contains the combined results of all generators passed to the driver
            Debug.Assert(runResult.GeneratedTrees.Length == 1);
            Debug.Assert(runResult.Diagnostics.IsEmpty);

            // Or you can access the individual results on a by-generator basis
            GeneratorRunResult generatorResult = runResult.Results[0];
            Debug.Assert(generatorResult.Generator == generator);
            Debug.Assert(generatorResult.Diagnostics.IsEmpty);

            Debug.Assert(generatorResult.GeneratedSources.Length == 1);
            Debug.Assert(generatorResult.Exception is null);

            var source = generatorResult.GeneratedSources.First().SourceText.ToString();
        }

        private static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

    }
}
