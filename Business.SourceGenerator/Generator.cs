/*==================================
             ########
            ##########
             ########
            ##########
          ##############
         #######  #######
        ######      ######
        #####        #####
        ####          ####
        ####   ####   ####
        #####  ####  #####
         ################
          ##############
==================================*/

namespace Business.SourceGenerator
{
    using Business.SourceGenerator.Analysis;
    using Microsoft.CodeAnalysis;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    [Generator(LanguageNames.CSharp)]
    public class Generator : ISourceGenerator
    {
        readonly bool global = true;

        //internal Generator(bool global = false)
        //{
        //    this.global = global;
        //}

        public void Execute(GeneratorExecutionContext context)
        {
            //try
            //{
            //    using (FileStream moduleStream = File.OpenRead(""))
            //    using (var module = new PEReader(moduleStream))
            //    {
            //        if (module.HasMetadata)
            //        {
            //            MetadataReader moduleMetadataReader = module.GetMetadataReader();
            //            if (moduleMetadataReader.IsAssembly)
            //            {
            //                string culture = moduleMetadataReader.GetString(moduleMetadataReader.GetAssemblyDefinition().Culture);

            //                //assembliesToSkipPublish.Add(taskItem);
            //                if (culture == "" || culture.Equals("neutral", StringComparison.OrdinalIgnoreCase))
            //                {
            //                    // NativeAOT doesn't consume resource assemblies yet so skip them
            //                    //list.Add(taskItem);
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (BadImageFormatException)
            //{
            //}

            var watchCount = new System.Diagnostics.Stopwatch();
            watchCount.Start();

            //var global = !string.IsNullOrEmpty(context.GetMSBuildProperty("Business_SourceGenerator_Global"));

            //if (!global)
            //{
            //    global = this.global;
            //}

            var opt = new SyntaxToCode.ToCodeOpt(standardFormat: true, global: global);
            var format = opt.StandardFormat ? Environment.NewLine : " ";
            var format2 = opt.StandardFormat ? $"{format}{format}" : format;

            var usings = new string[]
            {
                "Business.SourceGenerator.Meta",
                "Business.SourceGenerator",
                "System.Collections.Generic",
                "System.Collections.ObjectModel",
                "System.Threading.Tasks.Sources",
                "System.Threading.Tasks",
                "System.Threading",
                "System.Linq.Expressions",
                "System.Linq",
                "System.Globalization",
                "System.IO",
                "System",
            };

            var usings2 = !opt.Global ? $"{string.Join(Meta.Global.EnvironmentNewLine, usings.Select(c => $"using {c};"))}{format2}" : default;

            try
            {
                AnalysisMeta.Init(context);

                var generator = Expression.GeneratorCode(AnalysisMeta.AnalysisInfo, context.Compilation.AssemblyName, opt, usings);

                var app = context.Compilation.Options.OutputKind is OutputKind.ConsoleApplication || context.Compilation.Options.OutputKind is OutputKind.WindowsApplication || context.Compilation.Options.OutputKind is OutputKind.WindowsRuntimeApplication;

                if (app)
                {
                    var generatorType = $"{generator.types}";

                    //var file = System.IO.Path.GetTempFileName();
                    //System.IO.File.WriteAllText(file, generatorType, System.Text.Encoding.UTF8);
                    
                    ////context.Compilation.Emit().
                    //try
                    //{
                    //    using (var ms = new MemoryStream())
                    //    {
                    //        var compilation = context.Compilation.AddSyntaxTree(generatorType, context.ParseOptions);
                    //        //context.AddSource(Meta.Global.GeneratorCodeName, Microsoft.CodeAnalysis.Text.SourceText.From($"{usings2}{generatorType}", System.Text.Encoding.UTF8));

                    //        var emit = compilation.Emit(ms);

                    //        if (emit.Success)
                    //        {
                    //            System.IO.File.WriteAllBytes("D:\\Source\\Repos\\Business.SourceGenerator.Test\\csc\\Class1.dll", ms.ToArray());
                    //        }
                    //    }
                    //    var refs = string.Join(" ", context.Compilation.References.Select(c => $"/r:\"{c.Display}\""));
                    //    using (var p = Process.Start(new ProcessStartInfo("dotnet", @$"""C:\Program Files\dotnet\sdk\7.0.400\Roslyn\bincore\csc.dll"" {file} /target:library {refs} /out:D:\Source\Repos\Business.SourceGenerator.Test\csc\Class1.dll") { RedirectStandardOutput = true }))
                    //    {
                    //        var output = p.StandardOutput.ReadToEnd();
                    //    }
                    //}
                    //catch (Exception)
                    //{
                    //    throw;
                    //}
                    //finally
                    //{
                    //    System.IO.File.Delete(file);
                    //}

                    //using (var p = new Process())
                    //{
                    //    p.StartInfo = new ProcessStartInfo("cmd", "");
                    //}

                    //System.IO.File.Delete(file);

                    #region AddSource

                    context.AddSource(Meta.Global.GeneratorCodeName, Microsoft.CodeAnalysis.Text.SourceText.From($"{usings2}{generatorType}", System.Text.Encoding.UTF8));

                    foreach (var item in generator.code)
                    {
                        context.AddSource($"{Guid.NewGuid():N}_TypeMeta", Microsoft.CodeAnalysis.Text.SourceText.From($"{usings2}{item}", System.Text.Encoding.UTF8));
                    }

                    #endregion

                    //var accessors = Expression.GeneratorAccessor(AnalysisMeta.AnalysisInfo, context.Compilation.AssemblyName, opt, usings);

                    #region AddSource AccessorType

                    foreach (var item in AnalysisMeta.AnalysisInfo.AccessorType)
                    {
                        //var kkk = item.Key.Replace("<", "_").Replace(">", "_").Replace("?", "_");
                        //if (200 < kkk.Length)
                        //{
                        //    kkk = kkk.Substring(0, 200);
                        //}
                        //if (kkk.Contains("{System.String account}"))
                        //{

                        //}
                        context.AddSource($"{Guid.NewGuid():N}_AccessorType", Microsoft.CodeAnalysis.Text.SourceText.From(item.Value, System.Text.Encoding.UTF8));
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                context.Log($"{ex.Message}{ex.StackTrace.Replace(Environment.NewLine, " ")}", DiagnosticSeverity.Error);
            }
            finally
            {
                //watch.Stop();
                watchCount.Stop();
                //context.Log($"generator complete! [{watchCount.Elapsed.TotalMilliseconds.Scale(0)}ms]");
            }
        }

        public void Initialize(GeneratorInitializationContext context) => context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<SyntaxNode> SyntaxNodes { get; } = new List<SyntaxNode>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode) => SyntaxNodes.Add(syntaxNode);
    }
}
