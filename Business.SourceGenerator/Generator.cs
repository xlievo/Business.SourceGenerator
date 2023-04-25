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

            try
            {
                AnalysisMeta.Init(context);

                var generatorTypeCode = $"{Expression.GeneratorCode(AnalysisMeta.AnalysisInfo, context.Compilation.AssemblyName, opt, usings)}";

                #region AddSource

                var accessors = Expression.GeneratorAccessor(AnalysisMeta.AnalysisInfo, context.Compilation.AssemblyName, opt, usings);

                if (!string.IsNullOrEmpty(generatorTypeCode) || !string.IsNullOrEmpty(accessors))
                {
                    var accessorsCode = $"{format2}{accessors}";
                    var usings2 = $"{string.Join(Meta.Global.EnvironmentNewLine, usings.Select(c => $"using {c};"))}{format2}";

                    var code = $"{(!opt.Global ? usings2 : default)}{generatorTypeCode}{accessorsCode}";

                    context.AddSource(Meta.Global.GeneratorCodeName, Microsoft.CodeAnalysis.Text.SourceText.From(code, System.Text.Encoding.UTF8));
                }

                #endregion
            }
            catch (Exception ex)
            {
                context.Log($"{Environment.NewLine}{ex}{Environment.NewLine}", DiagnosticSeverity.Error);
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
