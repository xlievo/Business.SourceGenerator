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
    internal class Generator : ISourceGenerator
    {
        /// <summary>
        /// "BusinessSourceGenerator"
        /// </summary>
        const string GeneratorCodeName = "BusinessSourceGenerator";

        public void Execute(GeneratorExecutionContext context)
        {
            var opt = new Expression.ToCodeOpt(standardFormat: true);
            var format = opt.StandardFormat ? Environment.NewLine : " ";
            var format2 = opt.StandardFormat ? $"{format}{format}" : format;

            try
            {
                MetaData.Init(context);

                string generatorTypeCode = null;

                if (null != context.Compilation.GetEntryPoint(context.CancellationToken))
                {
                    generatorTypeCode = $"{format2}{Expression.GeneratorCode(MetaData.AnalysisInfo, context.Compilation.AssemblyName, opt)}";
                }

                #region AddSource

                var accessors = Expression.GeneratorAccessor(MetaData.AnalysisInfo, context.Compilation.AssemblyName, opt);

                if (!string.IsNullOrEmpty(generatorTypeCode) || !string.IsNullOrEmpty(accessors))
                {
                    var accessorsCode = $"{format2}{accessors}";
                    var usings = @"using Business.SourceGenerator.Meta;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;";
                    var code = $"{usings}{generatorTypeCode}{accessorsCode}";

                    context.AddSource(GeneratorCodeName, Microsoft.CodeAnalysis.Text.SourceText.From(code, System.Text.Encoding.UTF8));
                }

                #endregion
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                context.AddSource("Business.SourceGenerator.Logs", Microsoft.CodeAnalysis.Text.SourceText.From($"/*{Environment.NewLine}{ex}{Environment.NewLine}*/", System.Text.Encoding.UTF8));
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
