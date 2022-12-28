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

    [Generator(LanguageNames.CSharp)]
    public class Generator : ISourceGenerator
    {
        /// <summary>
        /// "BusinessSourceGenerator"
        /// </summary>
        const string GeneratorCodeName = "BusinessSourceGenerator";

        public void Execute(GeneratorExecutionContext context)
        {
            var watchCount = new System.Diagnostics.Stopwatch();
            watchCount.Start();
            var opt = new Expression.ToCodeOpt(standardFormat: true, global: false);
            var format = opt.StandardFormat ? Environment.NewLine : " ";
            var format2 = opt.StandardFormat ? $"{format}{format}" : format;

            try
            {
                MetaData.Init(context);

                var generatorTypeCode = $"{Expression.GeneratorCode(MetaData.AnalysisInfo, context.Compilation.AssemblyName, opt)}";
                /*
                string generatorTypeCode = null;

                if (null != context.Compilation.GetEntryPoint(context.CancellationToken))
                {
                    watch.Restart();
                    generatorTypeCode = $"{format2}{Expression.GeneratorCode(MetaData.AnalysisInfo, context.Compilation.AssemblyName, opt)}";
                    watch.Stop();
                    context.Log($"step 2 GeneratorCode complete! [{watch.Elapsed.TotalMilliseconds.Scale(0)}ms]");
                }
                */
                #region AddSource

                var accessors = Expression.GeneratorAccessor(MetaData.AnalysisInfo, context.Compilation.AssemblyName, opt);

                if (!string.IsNullOrEmpty(generatorTypeCode) || !string.IsNullOrEmpty(accessors))
                {
                    var accessorsCode = $"{format2}{accessors}";
                    var usings = $@"using Business.SourceGenerator.Meta;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;{format2}";

                    var code = $"{(!opt.Global ? usings : default)}{generatorTypeCode}{accessorsCode}";

                    context.AddSource(GeneratorCodeName, Microsoft.CodeAnalysis.Text.SourceText.From(code, System.Text.Encoding.UTF8));
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
