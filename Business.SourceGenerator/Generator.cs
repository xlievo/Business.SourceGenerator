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
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    [Generator]
    public class Generator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            System.Diagnostics.Debugger.Launch();

            try
            {
                MetaData.Init(context);

                var keyFormat = GeneratorGenericType.TypeKeyFormat.ToLower;

                var generatorField = SyntaxFactoryExt.ParseField("generator", SyntaxFactory.ParseName(TypeNameFormatter.TypeName.GetFormattedName(typeof(Lazy<IGeneratorCode>), TypeNameFormatter.TypeNameFormatOptions.Namespaces)), SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword).Initializer(SyntaxFactoryExt.ObjectCreationExpression(SyntaxFactory.ParseName(TypeNameFormatter.TypeName.GetFormattedName(typeof(Lazy<IGeneratorCode>), TypeNameFormatter.TypeNameFormatOptions.Namespaces)), SyntaxFactory.ParenthesizedLambdaExpression(SyntaxFactoryExt.ObjectCreationExpression(SyntaxFactory.ParseTypeName(Utils.GeneratorCodeName)))));

                var generatorProperty = SyntaxFactoryExt.ParseProperty("Generator", SyntaxFactory.ParseName(TypeNameFormatter.TypeName.GetFormattedName(typeof(IGeneratorCode), TypeNameFormatter.TypeNameFormatOptions.Namespaces)), SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)).WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactoryExt.QualifiedName("generator", "Value"))), null, SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword);

                MemberDeclarationSyntax utils = SyntaxFactoryExt.ParseClass(Utils.GeneratorCodeName).AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword)
                   .AddBaseListTypes(SyntaxFactory.ParseName(TypeNameFormatter.TypeName.GetFormattedName(typeof(IGeneratorCode), TypeNameFormatter.TypeNameFormatOptions.Namespaces))).AddMembers(generatorField, generatorProperty);

                var generatorGenericTypesName = nameof(IGeneratorCode.GeneratorGenericTypes);
                var generatorGenericTypesNameToLower = $"{generatorGenericTypesName[0].ToString().ToLower()}{generatorGenericTypesName.Substring(1)}";
                var typeId = SyntaxFactory.Identifier("type");
                var typeArgumentsId = SyntaxFactory.Identifier("typeArguments");

                var genericTypes = GeneratorGenericType.GetTypes(MetaData.AnalysisInfo, keyFormat);

                if (genericTypes.Any())
                {
                    var key = SyntaxFactory.IdentifierName("key");
                    var keyToLower = SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, key, SyntaxFactory.IdentifierName(keyFormat.GetName())));

                    var generatorGenericTypesField = SyntaxFactoryExt.ParseField(generatorGenericTypesNameToLower, typeof(Type[]), SyntaxKind.ReadOnlyKeyword).Initializer(SyntaxFactoryExt.ArrayInitializerExpression(genericTypes.Select(c => SyntaxFactory.TypeOfExpression(SyntaxFactory.ParseTypeName(c.Value))).ToArray()));

                    var generatorGenericTypesProperty = SyntaxFactoryExt.ParseProperty(generatorGenericTypesName, typeof(IEnumerable<Type>), SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)).WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.IdentifierName(generatorGenericTypesNameToLower))), null, SyntaxKind.PublicKeyword);

                    utils = (utils as ClassDeclarationSyntax).AddMembers(
                        generatorGenericTypesField,
                        generatorGenericTypesProperty);

                    #region public Type GetGenericType(string key)

                    utils = (utils as ClassDeclarationSyntax).AddMembers(SyntaxFactoryExt.ParseMethod(nameof(IGeneratorCode.GetGenericType)).AddModifiers(SyntaxKind.PublicKeyword)
                    .WithReturnType(typeof(Type))
                    .WithParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("key")).WithType(SyntaxFactoryExt.ParseType(typeof(string))))
                    .WithBody(
                        SyntaxFactory.IfStatement(SyntaxFactory.IsPatternExpression(key, SyntaxFactory.ConstantPattern(SyntaxFactoryExt.ParseLiteralNull())), SyntaxFactory.Block(SyntaxFactory.ThrowStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactoryExt.ArgumentNullException(), SyntaxFactory.ArgumentList().Add(SyntaxFactoryExt.NameOf(key)), null)))),
                        SyntaxFactoryExt.ParseSwitch(keyToLower,
                        SyntaxFactory.ReturnStatement(SyntaxFactoryExt.ParseDefaultLiteral()), genericTypes.Select(c => (SyntaxFactoryExt.ParseLiteral(c.Key) as ExpressionSyntax, SyntaxFactory.ReturnStatement(SyntaxFactory.TypeOfExpression(SyntaxFactory.ParseTypeName(c.Value))) as StatementSyntax)).ToArray())));

                    #endregion

                    #region public string GetGenericType(Type type, params Type[] typeArguments)

                    var argsId = SyntaxFactory.Identifier("args");
                    var typeIdName = SyntaxFactory.IdentifierName("type");
                    var typeArgumentsIdName = SyntaxFactory.IdentifierName("typeArguments");

                    utils = (utils as ClassDeclarationSyntax).AddMembers(SyntaxFactoryExt.ParseMethod(nameof(IGeneratorCode.GetGenericType)).AddModifiers(SyntaxKind.PublicKeyword)
                    .WithReturnType(typeof(string))
                    .WithParameters(SyntaxFactory.Parameter(typeId).WithType(SyntaxFactoryExt.ParseType(typeof(Type))), SyntaxFactory.Parameter(typeArgumentsId).WithType(SyntaxFactoryExt.ParseType(typeof(Type[]))).AddModifiers(SyntaxFactory.Token(SyntaxKind.ParamsKeyword)))
                    .WithBody(
                        SyntaxFactory.IfStatement(SyntaxFactory.IsPatternExpression(typeIdName, SyntaxFactory.ConstantPattern(SyntaxFactoryExt.ParseLiteralNull())), SyntaxFactory.Block(SyntaxFactory.ThrowStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactoryExt.ArgumentNullException(), SyntaxFactory.ArgumentList().Add(SyntaxFactoryExt.NameOf(typeIdName)), null)))),

                        SyntaxFactory.IfStatement(SyntaxFactory.IsPatternExpression(typeArgumentsIdName, SyntaxFactory.ConstantPattern(SyntaxFactoryExt.ParseLiteralNull())), SyntaxFactory.Block(SyntaxFactory.ThrowStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactoryExt.ArgumentNullException(), SyntaxFactory.ArgumentList().Add(SyntaxFactoryExt.NameOf(typeArgumentsIdName)), null)))),

                        SyntaxFactory.LocalDeclarationStatement(SyntaxFactoryExt.VariableDeclaration("var", SyntaxFactoryExt.VariableDeclarator("typeName", SyntaxFactoryExt.InvocationExpression(SyntaxFactoryExt.MemberAccessExpression(SyntaxFactoryExt.MemberAccessExpression("Business.SourceGenerator.TypeNameFormatter", "TypeName"), "GetFormattedName"), typeIdName, SyntaxFactoryExt.BinaryExpression(SyntaxFactoryExt.MemberAccessExpression("Business.SourceGenerator.TypeNameFormatter", "TypeNameFormatOptions", "Namespaces"), SyntaxFactoryExt.MemberAccessExpression("Business.SourceGenerator.TypeNameFormatter", "TypeNameFormatOptions", "NoKeywords"), SyntaxFactoryExt.MemberAccessExpression("Business.SourceGenerator.TypeNameFormatter", "TypeNameFormatOptions", "NoAnonymousTypes"), SyntaxFactoryExt.MemberAccessExpression("Business.SourceGenerator.TypeNameFormatter", "TypeNameFormatOptions", "NoGeneric")))))),

                        SyntaxFactory.LocalDeclarationStatement(SyntaxFactoryExt.VariableDeclaration("var", SyntaxFactoryExt.VariableDeclarator(argsId, SyntaxFactoryExt.InvocationExpression(SyntaxFactoryExt.MemberAccessExpression("string", "Join"), SyntaxFactoryExt.ParseLiteral(", "), SyntaxFactoryExt.InvocationExpression(SyntaxFactoryExt.MemberAccessExpression("typeArguments", "Select"), SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(SyntaxFactory.ParseToken("c")), SyntaxFactoryExt.InvocationExpression(SyntaxFactoryExt.MemberAccessExpression(SyntaxFactoryExt.MemberAccessExpression("Business.SourceGenerator.TypeNameFormatter", "TypeName"), "GetFormattedName"), SyntaxFactory.IdentifierName("c"), SyntaxFactoryExt.BinaryExpression(SyntaxFactoryExt.MemberAccessExpression("Business.SourceGenerator.TypeNameFormatter", "TypeNameFormatOptions", "Namespaces"), SyntaxFactoryExt.MemberAccessExpression("Business.SourceGenerator.TypeNameFormatter", "TypeNameFormatOptions", "NoKeywords"), SyntaxFactoryExt.MemberAccessExpression("Business.SourceGenerator.TypeNameFormatter", "TypeNameFormatOptions", "NoAnonymousTypes"))))))))),

                        SyntaxFactory.IfStatement(SyntaxFactoryExt.InvocationExpression(SyntaxFactoryExt.MemberAccessExpression(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), SyntaxFactory.IdentifierName("IsNullOrEmpty")), SyntaxFactory.IdentifierName(argsId)), SyntaxFactory.Block(SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("typeName")))),

                        SyntaxFactory.ReturnStatement(SyntaxFactory.InterpolatedStringExpression(SyntaxFactory.Token(SyntaxKind.InterpolatedStringStartToken), new SyntaxList<InterpolatedStringContentSyntax>()
                        .Add(SyntaxFactory.Interpolation(SyntaxFactory.IdentifierName("typeName")))
                        .Add(SyntaxFactory.InterpolatedStringText(SyntaxFactoryExt.Token(SyntaxKind.InterpolatedStringTextToken, "<")))
                        .Add(SyntaxFactory.Interpolation(SyntaxFactory.IdentifierName(argsId)))
                        .Add(SyntaxFactory.InterpolatedStringText(SyntaxFactoryExt.Token(SyntaxKind.InterpolatedStringTextToken, ">"))), SyntaxFactory.Token(SyntaxKind.InterpolatedStringEndToken)))
                        ));

                    #endregion

                    #region public Type MakeGenericType(Type type, params Type[] typeArguments)

                    utils = (utils as ClassDeclarationSyntax).AddMembers(SyntaxFactoryExt.ParseMethod(nameof(IGeneratorCode.MakeGenericType)).AddModifiers(SyntaxKind.PublicKeyword)
                    .WithReturnType(typeof(Type))
                    .WithParameters(SyntaxFactory.Parameter(typeId).WithType(SyntaxFactoryExt.ParseType(typeof(Type))), SyntaxFactory.Parameter(typeArgumentsId).WithType(SyntaxFactoryExt.ParseType(typeof(Type[]))).AddModifiers(SyntaxFactory.Token(SyntaxKind.ParamsKeyword)))
                    .WithBody(SyntaxFactory.ReturnStatement(SyntaxFactoryExt.InvocationExpression(SyntaxFactory.IdentifierName("GetGenericType"), SyntaxFactoryExt.InvocationExpression(SyntaxFactory.IdentifierName("GetGenericType"), typeIdName, typeArgumentsIdName)))));

                    #endregion

                    #region public object CreateGenericType(Type type, params Type[] typeArguments)

                    utils = (utils as ClassDeclarationSyntax).AddMembers(SyntaxFactoryExt.ParseMethod(nameof(IGeneratorCode.CreateGenericType)).AddModifiers(SyntaxKind.PublicKeyword)
                    .WithReturnType(typeof(object))
                    .WithParameters(SyntaxFactory.Parameter(typeId).WithType(SyntaxFactoryExt.ParseType(typeof(Type))), SyntaxFactory.Parameter(typeArgumentsId).WithType(SyntaxFactoryExt.ParseType(typeof(Type[]))).AddModifiers(SyntaxFactory.Token(SyntaxKind.ParamsKeyword)))
                    .WithBody(
                        SyntaxFactory.IfStatement(SyntaxFactory.IsPatternExpression(typeIdName, SyntaxFactory.ConstantPattern(SyntaxFactoryExt.ParseLiteralNull())), SyntaxFactory.Block(SyntaxFactory.ThrowStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactoryExt.ArgumentNullException(), SyntaxFactory.ArgumentList().Add(SyntaxFactoryExt.NameOf(typeIdName)), null)))),

                        SyntaxFactory.IfStatement(SyntaxFactory.IsPatternExpression(typeArgumentsIdName, SyntaxFactory.ConstantPattern(SyntaxFactoryExt.ParseLiteralNull())), SyntaxFactory.Block(SyntaxFactory.ThrowStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactoryExt.ArgumentNullException(), SyntaxFactory.ArgumentList().Add(SyntaxFactoryExt.NameOf(typeArgumentsIdName)), null)))),

                        SyntaxFactory.LocalDeclarationStatement(SyntaxFactoryExt.VariableDeclaration("var", SyntaxFactoryExt.VariableDeclarator("key", SyntaxFactoryExt.InvocationExpression(SyntaxFactory.IdentifierName("GetGenericType"), typeIdName, typeArgumentsIdName)))),

                        SyntaxFactoryExt.ParseSwitch(keyToLower,
                    SyntaxFactory.ReturnStatement(SyntaxFactoryExt.ParseDefaultLiteral()), genericTypes.Select(c => (SyntaxFactoryExt.ParseLiteral(c.Key) as ExpressionSyntax, SyntaxFactory.ReturnStatement(SyntaxFactoryExt.ObjectCreationExpression(SyntaxFactory.ParseTypeName(c.Value))) as StatementSyntax)).ToArray())
                    ));

                    #endregion
                }
                else
                {
                    utils = (utils as ClassDeclarationSyntax).AddMembers(SyntaxFactoryExt.ParseProperty(generatorGenericTypesName, typeof(IEnumerable<Type>), SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)).WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactoryExt.ParseDefaultLiteral())), null, SyntaxKind.PublicKeyword));

                    #region public Type GetGenericType(string key)

                    utils = (utils as ClassDeclarationSyntax).AddMembers(SyntaxFactoryExt.ParseMethod(nameof(IGeneratorCode.GetGenericType)).AddModifiers(SyntaxKind.PublicKeyword)
                    .WithReturnType(typeof(Type))
                    .WithParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("key")).WithType(SyntaxFactoryExt.ParseType(typeof(string))))
                    .WithBody(SyntaxFactory.ReturnStatement(SyntaxFactoryExt.ParseDefaultLiteral())));

                    #endregion

                    #region public string GetGenericType(Type type, params Type[] typeArguments)

                    utils = (utils as ClassDeclarationSyntax).AddMembers(SyntaxFactoryExt.ParseMethod(nameof(IGeneratorCode.GetGenericType)).AddModifiers(SyntaxKind.PublicKeyword)
                    .WithReturnType(typeof(string))
                    .WithParameters(SyntaxFactory.Parameter(typeId).WithType(SyntaxFactoryExt.ParseType(typeof(Type))), SyntaxFactory.Parameter(typeArgumentsId).WithType(SyntaxFactoryExt.ParseType(typeof(Type[]))).AddModifiers(SyntaxFactory.Token(SyntaxKind.ParamsKeyword)))
                    .WithBody(SyntaxFactory.ReturnStatement(SyntaxFactoryExt.ParseDefaultLiteral())));

                    #endregion

                    #region public Type MakeGenericType(Type type, params Type[] typeArguments)

                    utils = (utils as ClassDeclarationSyntax).AddMembers(SyntaxFactoryExt.ParseMethod(nameof(IGeneratorCode.MakeGenericType)).AddModifiers(SyntaxKind.PublicKeyword)
                    .WithReturnType(typeof(Type))
                    .WithParameters(SyntaxFactory.Parameter(typeId).WithType(SyntaxFactoryExt.ParseType(typeof(Type))), SyntaxFactory.Parameter(typeArgumentsId).WithType(SyntaxFactoryExt.ParseType(typeof(Type[]))).AddModifiers(SyntaxFactory.Token(SyntaxKind.ParamsKeyword)))
                    .WithBody(SyntaxFactory.ReturnStatement(SyntaxFactoryExt.ParseDefaultLiteral())));

                    #endregion

                    #region public object CreateGenericType(Type type, params Type[] typeArguments)

                    utils = (utils as ClassDeclarationSyntax).AddMembers(SyntaxFactoryExt.ParseMethod(nameof(IGeneratorCode.CreateGenericType)).AddModifiers(SyntaxKind.PublicKeyword)
                    .WithReturnType(typeof(object))
                    .WithParameters(SyntaxFactory.Parameter(typeId).WithType(SyntaxFactoryExt.ParseType(typeof(Type))), SyntaxFactory.Parameter(typeArgumentsId).WithType(SyntaxFactoryExt.ParseType(typeof(Type[]))).AddModifiers(SyntaxFactory.Token(SyntaxKind.ParamsKeyword)))
                    .WithBody(SyntaxFactory.ReturnStatement(SyntaxFactoryExt.ParseDefaultLiteral())));

                    #endregion
                }

                var typesName = nameof(IGeneratorCode.Types);
                var typesNameToLower = $"{typesName[0].ToString().ToLower()}{typesName.Substring(1)}";

                var types = Expression.GetTypes(MetaData.AnalysisInfo.TypeSymbols, keyFormat);

                if (types.Any())
                {
                    var typesField = SyntaxFactoryExt.ParseField(typesNameToLower, typeof(string[]), SyntaxKind.ReadOnlyKeyword).Initializer(SyntaxFactoryExt.ArrayInitializerExpression(types.Select(c => SyntaxFactoryExt.ParseLiteral(c)).ToArray()));

                    var typesProperty = SyntaxFactoryExt.ParseProperty(typesName, typeof(IEnumerable<string>), SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)).WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.IdentifierName(typesNameToLower))), null, SyntaxKind.PublicKeyword);

                    utils = (utils as ClassDeclarationSyntax).AddMembers(typesField, typesProperty);

                    #region public bool ContainsType(Type type)

                    var typeIdName = SyntaxFactory.IdentifierName("type");

                    var key = SyntaxFactory.IdentifierName("typeName");
                    var keyToLower = SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, key, SyntaxFactory.IdentifierName(keyFormat.GetName())));

                    utils = (utils as ClassDeclarationSyntax).AddMembers(SyntaxFactoryExt.ParseMethod(nameof(IGeneratorCode.ContainsType)).AddModifiers(SyntaxKind.PublicKeyword)
                    .WithReturnType(typeof(bool))
                    .WithParameters(SyntaxFactory.Parameter(typeId).WithType(SyntaxFactoryExt.ParseType(typeof(Type))))
                    .WithBody(
                        SyntaxFactory.IfStatement(SyntaxFactory.IsPatternExpression(typeIdName, SyntaxFactory.ConstantPattern(SyntaxFactoryExt.ParseLiteralNull())), SyntaxFactory.Block(SyntaxFactory.ThrowStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactoryExt.ArgumentNullException(), SyntaxFactory.ArgumentList().Add(SyntaxFactoryExt.NameOf(typeIdName)), null)))),

                        SyntaxFactory.LocalDeclarationStatement(SyntaxFactoryExt.VariableDeclaration("var", SyntaxFactoryExt.VariableDeclarator("typeName", SyntaxFactoryExt.InvocationExpression(SyntaxFactoryExt.MemberAccessExpression(SyntaxFactoryExt.MemberAccessExpression("Business.SourceGenerator.TypeNameFormatter", "TypeName"), "GetFormattedName"), typeIdName, SyntaxFactoryExt.BinaryExpression(SyntaxFactoryExt.MemberAccessExpression("Business.SourceGenerator.TypeNameFormatter", "TypeNameFormatOptions", "Namespaces"), SyntaxFactoryExt.MemberAccessExpression("Business.SourceGenerator.TypeNameFormatter", "TypeNameFormatOptions", "NoKeywords"), SyntaxFactoryExt.MemberAccessExpression("Business.SourceGenerator.TypeNameFormatter", "TypeNameFormatOptions", "NoAnonymousTypes"), SyntaxFactoryExt.MemberAccessExpression("Business.SourceGenerator.TypeNameFormatter", "TypeNameFormatOptions", "NoGeneric")))))),

                        SyntaxFactoryExt.ParseSwitch(keyToLower,
                        SyntaxFactory.ReturnStatement(SyntaxFactoryExt.ParseDefaultLiteral()), types.Select(c => (SyntaxFactoryExt.ParseLiteral(c) as ExpressionSyntax, SyntaxFactory.ReturnStatement(SyntaxFactoryExt.ParseBooleanLiteral(true)) as StatementSyntax)).ToArray())
                        ));

                    #endregion
                }
                else
                {
                    utils = (utils as ClassDeclarationSyntax).AddMembers(SyntaxFactoryExt.ParseProperty(typesName, typeof(IEnumerable<string>), SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)).WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactoryExt.ParseDefaultLiteral())), null, SyntaxKind.PublicKeyword));

                    utils = (utils as ClassDeclarationSyntax).AddMembers(SyntaxFactoryExt.ParseMethod(nameof(IGeneratorCode.ContainsType)).AddModifiers(SyntaxKind.PublicKeyword)
                    .WithReturnType(typeof(bool))
                    .WithParameters(SyntaxFactory.Parameter(typeId).WithType(SyntaxFactoryExt.ParseType(typeof(Type))))
                    .WithBody(SyntaxFactory.ReturnStatement(SyntaxFactoryExt.ParseDefaultLiteral())));
                }

                #region AddSource

                if (!string.IsNullOrEmpty(context.Compilation.AssemblyName))
                {
                    utils = SyntaxFactoryExt.ParseNamespace(context.Compilation.AssemblyName)
                        //.AddUsings("System")
                        .AddMembers(utils);
                }

                var code = $"using System; using System.Collections.Generic; using System.Linq; {utils.ToCode()}";

                context.AddSource(Utils.GeneratorCodeName, Microsoft.CodeAnalysis.Text.SourceText.From(code, System.Text.Encoding.UTF8));

                #endregion
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                context.AddSource("Business.SourceGenerator.Logs", Microsoft.CodeAnalysis.Text.SourceText.From($"/*{Environment.NewLine}{ex}{Environment.NewLine}*/", System.Text.Encoding.UTF8));
            }
        }

        public void Execute2(GeneratorExecutionContext context)
        {
            System.Diagnostics.Debugger.Launch();

            MetaData.Init(context);


            var ddd = context.Compilation.Options.SourceReferenceResolver.GetType();

            var code2 = System.IO.File.ReadAllText(System.IO.Path.Combine(@"D:\Repos\Business.SourceGenerator-main\Business.SourceGenerator\Assets\src", "code.cs"));

            var compilation = context.Compilation.AddSyntaxTree(code2, out SyntaxTree tree, context.ParseOptions);

            var sm = compilation.GetSemanticModel(tree);

            var root = tree.GetRoot();
            //SyntaxFactory.NameOfExpression();
            //SyntaxFactory.Token(SyntaxKind.OpenParenToken), Nothing, SyntaxFactory.Token(SyntaxKind.CloseParenToken)

            //IdentifierNameSyntax IdentifierName ArgumentNullException


            //var sss = dd.ToCode();

            var utils = root.DescendantNodes().FirstOrDefault(c => c is ClassDeclarationSyntax utils && utils.Identifier.Text == "Utils") as ClassDeclarationSyntax;
            var last = utils.Members.Last() as MethodDeclarationSyntax;
            var mm = last.Body.Statements.ToList();

            //var isp = SyntaxFactory.IsPatternExpression(SyntaxFactory.IdentifierName("type"), SyntaxFactory.ConstantPattern(SyntaxFactoryExtensions.ParseLiteralNull()));


            //var dd = SyntaxFactory.ObjectCreationExpression(SyntaxFactoryExtensions.ArgumentNullException(), SyntaxFactory.ArgumentList().Add(SyntaxFactoryExtensions.NameOf(SyntaxFactory.IdentifierName("type"))), null);

            var typeName = SyntaxFactory.IdentifierName("type");
            var typeArgumentsName = SyntaxFactory.IdentifierName("typeArguments");

            var zzz = SyntaxFactory.IfStatement(SyntaxFactory.IsPatternExpression(typeName, SyntaxFactory.ConstantPattern(SyntaxFactoryExt.ParseLiteralNull())), SyntaxFactory.Block(SyntaxFactory.ThrowStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactoryExt.ArgumentNullException(), SyntaxFactory.ArgumentList().Add(SyntaxFactoryExt.NameOf(typeName)), null))));

            var zzz2 = SyntaxFactory.IfStatement(SyntaxFactory.IsPatternExpression(typeArgumentsName, SyntaxFactory.ConstantPattern(SyntaxFactoryExt.ParseLiteralNull())), SyntaxFactory.Block(SyntaxFactory.ThrowStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactoryExt.ArgumentNullException(), SyntaxFactory.ArgumentList().Add(SyntaxFactoryExt.NameOf(typeArgumentsName)), null))));

            var zzz3 = SyntaxFactory.IfStatement(SyntaxFactoryExt.InvocationExpression(SyntaxFactoryExt.MemberAccessExpression(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), SyntaxFactory.IdentifierName("IsNullOrEmpty")), typeArgumentsName), SyntaxFactory.Block(SyntaxFactory.ReturnStatement(typeName))).ToCode();

            var zzz4 = SyntaxFactory.ReturnStatement(SyntaxFactoryExt.InvocationExpression(SyntaxFactory.IdentifierName("MakeGenericType"), SyntaxFactory.InterpolatedStringExpression(SyntaxFactory.Token(SyntaxKind.InterpolatedStringStartToken), new SyntaxList<InterpolatedStringContentSyntax>()
                .Add(SyntaxFactory.Interpolation(SyntaxFactory.IdentifierName("typeName")))
                .Add(SyntaxFactory.InterpolatedStringText(SyntaxFactoryExt.Token(SyntaxKind.InterpolatedStringTextToken, "<")))
                .Add(SyntaxFactory.Interpolation(SyntaxFactory.IdentifierName("args")))
                .Add(SyntaxFactory.InterpolatedStringText(SyntaxFactoryExt.Token(SyntaxKind.InterpolatedStringTextToken, ">"))), SyntaxFactory.Token(SyntaxKind.InterpolatedStringEndToken)))).ToCode();

            var vv = (mm[2] as LocalDeclarationStatementSyntax).Declaration.Variables[0].Initializer.Value as InvocationExpressionSyntax;
            var vv2 = (vv.ArgumentList.Arguments[1].Expression as InvocationExpressionSyntax).ArgumentList.Arguments[0].Expression;


            var sssss = SyntaxFactory.LocalDeclarationStatement(SyntaxFactoryExt.VariableDeclaration("var", SyntaxFactoryExt.VariableDeclarator("typeName", SyntaxFactoryExt.InvocationExpression(SyntaxFactoryExt.MemberAccessExpression(SyntaxFactoryExt.MemberAccessExpression("TypeNameFormatter", "TypeName"), "GetFormattedName"), typeName, SyntaxFactory.BinaryExpression(SyntaxKind.BitwiseOrExpression, SyntaxFactory.BinaryExpression(SyntaxKind.BitwiseOrExpression, SyntaxFactory.BinaryExpression(SyntaxKind.BitwiseOrExpression, SyntaxFactoryExt.MemberAccessExpression(SyntaxFactoryExt.MemberAccessExpression("TypeNameFormatter", "TypeNameFormatOptions"), "Namespaces"), SyntaxFactoryExt.MemberAccessExpression(SyntaxFactoryExt.MemberAccessExpression("TypeNameFormatter", "TypeNameFormatOptions"), "NoKeywords")), SyntaxFactoryExt.MemberAccessExpression(SyntaxFactoryExt.MemberAccessExpression("TypeNameFormatter", "TypeNameFormatOptions"), "NoAnonymousTypes")), SyntaxFactoryExt.MemberAccessExpression(SyntaxFactoryExt.MemberAccessExpression("TypeNameFormatter", "TypeNameFormatOptions"), "NoGeneric")))))).ToCode();

            var sssss2 = SyntaxFactory.LocalDeclarationStatement(SyntaxFactoryExt.VariableDeclaration("var", SyntaxFactoryExt.VariableDeclarator("args", SyntaxFactoryExt.InvocationExpression(SyntaxFactoryExt.MemberAccessExpression("string", "Join"), SyntaxFactoryExt.ParseLiteral(", "), SyntaxFactoryExt.InvocationExpression(SyntaxFactoryExt.MemberAccessExpression("typeArguments", "Select"), SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(SyntaxFactory.ParseToken("c")), SyntaxFactoryExt.InvocationExpression(SyntaxFactoryExt.MemberAccessExpression(SyntaxFactoryExt.MemberAccessExpression("TypeNameFormatter", "TypeName"), "GetFormattedName"), SyntaxFactory.IdentifierName("c"), SyntaxFactory.BinaryExpression(SyntaxKind.BitwiseOrExpression, SyntaxFactory.BinaryExpression(SyntaxKind.BitwiseOrExpression, SyntaxFactoryExt.MemberAccessExpression(SyntaxFactoryExt.MemberAccessExpression("TypeNameFormatter", "TypeNameFormatOptions"), "Namespaces"), SyntaxFactoryExt.MemberAccessExpression(SyntaxFactoryExt.MemberAccessExpression("TypeNameFormatter", "TypeNameFormatOptions"), "NoKeywords")), SyntaxFactoryExt.MemberAccessExpression(SyntaxFactoryExt.MemberAccessExpression("TypeNameFormatter", "TypeNameFormatOptions"), "NoAnonymousTypes"))))))))).ToCode();

            var zzz22 = mm[0].ToCode();

            //var sss2 = SyntaxFactoryExtensions.NameOf(SyntaxFactory.IdentifierName("type"));

            //var fff222 = SyntaxFactory.ArgumentList().Add(ff11).ToCode();


            //IfStatementSyntax IfStatement
            //var dict = utils.ChildNodes().FindField("dict");
            //Expression = LiteralExpressionSyntax NullLiteralExpression null
            //Pattern = ConstantPatternSyntax ConstantPattern null
            //var dict222 = new ReadOnlyDictionary<string, Type>(new Dictionary<string, Type> { { "string", typeof(string) } }).ToCode2();

            var fff = utils.Members.First() as FieldDeclarationSyntax;

            var tra = fff.Modifiers.First().TrailingTrivia.First();

            //Microsoft.CodeAnalysis.SyntaxTrivia
            //SyntaxFactory.ParseTrailingTrivia(" ");

            var traType = tra.GetType();

            var init = fff.Declaration.Variables.FirstOrDefault().Initializer;
            var dddw = init.Value as ObjectCreationExpressionSyntax;
            var d2 = dddw.ArgumentList.Arguments.FirstOrDefault().Expression as ObjectCreationExpressionSyntax;
            var d3 = d2.Initializer.Expressions.FirstOrDefault() as InitializerExpressionSyntax;
            var d4 = d3.Expressions.FirstOrDefault();
            var d5 = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.StringLiteralToken, "\"string\"", "string", SyntaxTriviaList.Empty));
            var d6 = d5.ToCode();

            //var dd = SyntaxFactoryEx.ParseType("System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyDictionary<string, System.Type>>>");
            //var init = dict.Initializer.Value as ObjectCreationExpressionSyntax;

            var field = SyntaxFactoryExt.ParseField("dict", typeof(IReadOnlyDictionary<string, Type>))

                .AddModifiers(SyntaxKind.ReadOnlyKeyword, SyntaxKind.StaticKeyword)

                .Initializer(typeof(ReadOnlyDictionary<string, Type>),
                SyntaxFactoryExt.ParseElementArgument(SyntaxFactoryExt.ParseType(typeof(Dictionary<string, Type>)),
               (SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.StringLiteralToken, $"\"string\"", nameof(String), SyntaxTriviaList.Empty)),
               SyntaxFactory.TypeOfExpression(SyntaxFactoryExt.ParseType(typeof(string)))),

                (SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.StringLiteralToken, $"\"string\"", nameof(String), SyntaxTriviaList.Empty)),
               SyntaxFactory.TypeOfExpression(SyntaxFactoryExt.ParseType(typeof(string))))

               ));

            //var sssss = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetKeyword, SyntaxFactory.Block());
            //AccessorDeclarationSyntax GetAccessorDeclaration get;
            var dda = SyntaxFactoryExt.ParseProperty(
                "AAA",
                SyntaxFactoryExt.ParseType(typeof(Dictionary<string, Type>)),
                SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                .Initializer(typeof(Dictionary<string, Type>));
            //.ToCode();

            var dd66 = SyntaxFactory.DefaultExpression(SyntaxFactory.ParseTypeName("string"));

            //var ddd2 = SyntaxFactory.ParseParameterList(string.Empty).AddParameters(SyntaxFactory.Parameter(default, default, SyntaxFactoryEx.ParseType(typeof(string)), SyntaxFactory.Identifier("key"), default)).WithOpenParenToken(SyntaxFactory.Token(SyntaxKind.OpenParenToken)).WithCloseParenToken(SyntaxFactory.Token(SyntaxKind.CloseParenToken));

            //[0] = SwitchStatementSyntax SwitchStatement switch (key)
            //{
            //    case "aaa": return typeof(string);
            //    case "bbb": return typeof(string);
            //    case "ccc": return typeof(string);
            //    default: return de...

            //[0] = SwitchSectionSyntax SwitchSection case "aaa": return typeof(string);

            //[0] = CaseSwitchLabelSyntax CaseSwitchLabel case "aaa":

            //[0] = DefaultSwitchLabelSyntax DefaultSwitchLabel default:

            //Expression = LiteralExpressionSyntax DefaultLiteralExpression default

            //.Add(SyntaxFactory.DefaultSwitchLabel()), 
            //SyntaxFactory.CaseSwitchLabel(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.StringLiteralToken, $"\"aaa\"", nameof(String), SyntaxTriviaList.Empty)))),
            //new SyntaxList<StatementSyntax>(SyntaxFactory.ReturnStatement(SyntaxFactory.TypeOfExpression(SyntaxFactoryEx.ParseType(typeof(string)))))

            //SyntaxFactory.SwitchSection().AddLabels
            var swit = SyntaxFactory.SwitchStatement(SyntaxFactory.IdentifierName("key"), new SyntaxList<SwitchSectionSyntax>().Add(SyntaxFactory.SwitchSection(

                new SyntaxList<SwitchLabelSyntax>(SyntaxFactory.CaseSwitchLabel(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.StringLiteralToken, $"\"aaa\"", nameof(String), SyntaxTriviaList.Empty)))),
                new SyntaxList<StatementSyntax>(SyntaxFactory.ReturnStatement(SyntaxFactory.TypeOfExpression(SyntaxFactoryExt.ParseType(typeof(string)))))))
                .Add(SyntaxFactory.SwitchSection(new SyntaxList<SwitchLabelSyntax>(SyntaxFactory.DefaultSwitchLabel()), new SyntaxList<StatementSyntax>(SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression))))));

            //var swit2 = SyntaxFactoryEx.ParseSwitch(SyntaxFactory.IdentifierName("key"), 
            //    SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression)), 
            //    new KeyValuePair<ExpressionSyntax, StatementSyntax>(SyntaxFactoryEx.ParseStringLiteral("aaa"), SyntaxFactory.ReturnStatement(SyntaxFactory.TypeOfExpression(SyntaxFactoryEx.ParseType(typeof(string))))));
            //var swit2 = SyntaxFactoryEx.ParseSwitch("key",
            //    SyntaxFactory.ReturnStatement(SyntaxFactoryEx.ParseDefaultLiteral()), (SyntaxFactoryEx.ParseStringLiteral("aaa"), SyntaxFactory.ReturnStatement(SyntaxFactoryEx.TypeOfExpression(typeof(string)))));

            var swit2 = SyntaxFactoryExt.ParseSwitch("key",
                SyntaxFactory.ReturnStatement(SyntaxFactoryExt.ParseDefaultLiteral()), (SyntaxFactoryExt.ParseLiteral('m'), SyntaxFactory.ReturnStatement(SyntaxFactoryExt.TypeOfExpression(typeof(string)))));

            // public static LiteralExpressionSyntax ParseStringLiteral(string text) => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.StringLiteralToken, $"\"{text}\"", nameof(String), SyntaxTriviaList.Empty));

            //Expression = LiteralExpressionSyntax DefaultLiteralExpression default
            //SyntaxFactory.DefaultConstraint(SyntaxFactory.Token(SyntaxKind.DefaultLiteralExpression))
            //switch
            //SyntaxFactory.SwitchStatement(null, SyntaxFactory.Token(SyntaxKind.SwitchKeyword), null, )
            //SyntaxFactory.SwitchSection(new SyntaxList<SwitchLabelSyntax>(SyntaxFactory.DefaultSwitchLabel()), new SyntaxList<StatementSyntax>(SyntaxFactory.SwitchStatement(SyntaxFactory.)));

            var code3 = SyntaxFactoryExt.ParseNamespace("Business.SourceGenerator").AddUsings("System", "System.Collections.Generic", "System.Collections.ObjectModel")
                //class
                .AddMembers(SyntaxFactoryExt.ParseClass("Utils").AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                //field
                .AddMembers(field)
                //method
                .AddMembers(SyntaxFactoryExt.ParseMethod("GetType").AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                .WithReturnType(typeof(Type))
                .WithParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("key")).WithType(SyntaxFactoryExt.ParseType(typeof(string))))
                //.WithExpressionBodyDefault()
                .WithBody(swit2)
                ))
                .ToCode();



            var code22 = field.ToCode();

            var nodes = root.ChildNodes();
            foreach (var item in nodes)
            {

            }


            var types = GeneratorGenericType.GetTypes(MetaData.AnalysisInfo);

            if (types.Any())
            {
                var code4 = SyntaxFactoryExt.ParseNamespace("Business.SourceGenerator").AddUsings("System")
                //class
                .AddMembers(SyntaxFactoryExt.ParseClass("Utils").AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.PartialKeyword)
                //method
                .AddMembers(SyntaxFactoryExt.ParseMethod("GetType").AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                .WithReturnType(typeof(Type))
                .WithParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("key")).WithType(SyntaxFactoryExt.ParseType(typeof(string))))
                .WithBody(SyntaxFactoryExt.ParseSwitch("key",
                SyntaxFactory.ReturnStatement(SyntaxFactoryExt.ParseDefaultLiteral()), types.Select(c => (SyntaxFactoryExt.ParseLiteral(c.Key) as ExpressionSyntax, SyntaxFactory.ReturnStatement(SyntaxFactory.TypeOfExpression(SyntaxFactory.ParseTypeName(c.Value))) as StatementSyntax)).ToArray()))))
                .ToCode();
            }

            //var strs = new List<string>();

            var sds = MetaData.AnalysisInfo.DeclaredSymbols;
            var sds2 = MetaData.AnalysisInfo.Invocations;
            //var sds3 = Expression.SemanticModels;
            var sds4 = MetaData.AnalysisInfo.StaticAssignments;
            var sds5 = MetaData.AnalysisInfo.TypeSymbols;

            //Microsoft.CodeAnalysis.INamedTypeSymbol.IsUnboundGenericType = true
            //Microsoft.CodeAnalysis.ITypeSymbol.OriginalDefinition = {DocArg}
            /*
            if (0 < MetaData.AnalysisInfo.GenericTypeSign.Count)
            {
                foreach (var item in MetaData.AnalysisInfo.GenericTypeSign)
                {
                    foreach (var sign in item.Value)
                    {
                        var usings = new List<UsingDirectiveSyntax>();
                        var declarations = new List<FileScopedNamespaceDeclarationSyntax>();

                        var targetsAll = Expression.GetMethodGenericParameters(item.Key, ordinal: sign.Value.MethodGenericPosition);
                        var targets = targetsAll.Distinct();

                        foreach (var target in targets)
                        {
                            if (target.Syntax is BaseTypeDeclarationSyntax typeSyntax)
                            {
                                var all = sign.Value.GenericTypeParameter.MakeGenericTypeAll(typeSyntax);
                                usings.AddRange(all.Usings);
                                declarations.Add(all.Declaration);
                                //var dd = all.Declaration.ToCode();
                                //strs.Add(dd);
                            }
                        }

                        var usings2 = usings.Distinct(Expression.Equality<UsingDirectiveSyntax>.CreateComparer(c => c.ToString())).ToArray();

                        declarations[0] = declarations.FirstOrDefault()?.AddUsings(usings2);

                        var d = string.Join(Environment.NewLine, declarations.Select(c => c.ToCode()));
                    }
                }
            }
            */
            //System.Diagnostics.Debugger.Log(0, null, $"======================{compilation.AssemblyName}======================{Environment.NewLine}");

            foreach (var item in MetaData.AnalysisInfo.TypeSymbols)
            {
                System.Diagnostics.Debugger.Log(0, null, $"TypeDeclaration : -------{item.Value.Declared.Name}-------{Environment.NewLine}");

                if (item.Value.Declared.GetSymbolInfo().Syntax is TypeDeclarationSyntax typeDeclaration)
                {
                    foreach (var item2 in typeDeclaration.Members)
                    {
                        System.Diagnostics.Debugger.Log(0, null, $"Member : {item2.GetSymbolInfo().Declared.Name}{Environment.NewLine}");
                    }
                }

                System.Diagnostics.Debugger.Log(0, null, Environment.NewLine);
            }

            //System.Diagnostics.Debugger.Log(0, null, $"^^^^^^^^^^^^^^^^^^^^^{compilation.AssemblyName}^^^^^^^^^^^^^^^^^^^^^{Environment.NewLine}");

            var ds = MetaData.AnalysisInfo.DeclaredSymbols.Where(c => c.Value.Declared is IFieldSymbol || c.Value.Declared is IPropertySymbol);

            var ss2 = string.Join(Environment.NewLine, ds.Select(c => $"public string {c.Value.Declared.GetFullName().Replace(".", "_").Replace("<", "_").Replace(">", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty).Split(',')[0].Trim()};").GroupBy(c => c).Select(c => c.Key));

            var classCode = $"public class class{MetaData.AnalysisInfo.DeclaredSymbols.Count} {{ {ss2} }}";
            var code = $"namespace codes{MetaData.AnalysisInfo.DeclaredSymbols.Count} {{ {classCode} }}";
            //context.AddSource("code", code);
        }

        public void Initialize(GeneratorInitializationContext context) => context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<SyntaxNode> SyntaxNodes { get; } = new List<SyntaxNode>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode) => SyntaxNodes.Add(syntaxNode);
    }
}
