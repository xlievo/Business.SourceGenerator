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

namespace Business.SourceGenerator.Analysis
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using static Business.SourceGenerator.Analysis.SyntaxToCode;

    internal static class AnalysisMeta
    {
        //new Dictionary<string, string>
        //{
        //    ["System.Void"] = "global::Business.SourceGenerator.Meta.Types.VoidType.Singleton",
        //    ["System.Object"] = "global::Business.SourceGenerator.Meta.Types.ObjectType.Singleton",
        //    ["System.Decimal"] = "global::Business.SourceGenerator.Meta.Types.DecimalType.Singleton",
        //    ["System.Double"] = "global::Business.SourceGenerator.Meta.Types.DoubleType.Singleton",
        //    ["System.Single"] = "global::Business.SourceGenerator.Meta.Types.SingleType.Singleton",
        //    ["System.UInt16"] = "global::Business.SourceGenerator.Meta.Types.UInt16Type.Singleton",
        //    ["System.UInt32"] = "global::Business.SourceGenerator.Meta.Types.UInt32Type.Singleton",
        //    ["System.UInt64"] = "global::Business.SourceGenerator.Meta.Types.UInt64Type.Singleton",
        //    ["System.Int16"] = "global::Business.SourceGenerator.Meta.Types.Int16Type.Singleton",
        //    ["System.Int32"] = "global::Business.SourceGenerator.Meta.Types.Int32Type.Singleton",
        //    ["System.Int64"] = "global::Business.SourceGenerator.Meta.Types.Int64Type.Singleton",
        //    ["System.String"] = "global::Business.SourceGenerator.Meta.Types.StringType.Singleton",
        //    ["System.Char"] = "global::Business.SourceGenerator.Meta.Types.CharType.Singleton",
        //    ["System.SByte"] = "global::Business.SourceGenerator.Meta.Types.SByteType.Singleton",
        //    ["System.Byte"] = "global::Business.SourceGenerator.Meta.Types.ByteType.Singleton",
        //    ["System.Boolean"] = "global::Business.SourceGenerator.Meta.Types.BooleanType.Singleton",
        //    ["System.Delegate"] = "global::Business.SourceGenerator.Meta.Types.DelegateType.Singleton",
        //    ["System.MulticastDelegate"] = "global::Business.SourceGenerator.Meta.Types.MulticastDelegateType.Singleton",
        //    ["System.Enum"] = "global::Business.SourceGenerator.Meta.Types.EnumType.Singleton",
        //    //["System.Type"] = "global::Business.SourceGenerator.Meta.Types.TypeType.Singleton",
        //    //["System."] = "global::Business.SourceGenerator.Meta.Types..Singleton",
        //}
    readonly static Lazy<AnalysisInfoModel> analysisInfoModel = new Lazy<AnalysisInfoModel>(() => new AnalysisInfoModel(new ConcurrentDictionary<string, StringCollection>(), new ConcurrentDictionary<string, SymbolInfo>(), new ConcurrentDictionary<string, SymbolInfo>(), new ConcurrentDictionary<string, ConcurrentDictionary<string, AssignmentExpressionSyntax>>(), new ConcurrentDictionary<string, ConcurrentDictionary<string, SymbolInfo>>(), new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, AttributeSyntax>>>(), new Dictionary<string, (string key, string code)>()));

        public static AnalysisInfoModel AnalysisInfo => analysisInfoModel.Value;

        public readonly struct AnalysisInfoModel
        {
            internal AnalysisInfoModel(ConcurrentDictionary<string, StringCollection> syntaxTrees, ConcurrentDictionary<string, SymbolInfo> declaredSymbols, ConcurrentDictionary<string, SymbolInfo> typeSymbols, ConcurrentDictionary<string, ConcurrentDictionary<string, AssignmentExpressionSyntax>> staticAssignments, ConcurrentDictionary<string, ConcurrentDictionary<string, SymbolInfo>> invocations, ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, AttributeSyntax>>> attributes, Dictionary<string, (string key, string code)> accessorType)
            {
                SyntaxTrees = syntaxTrees;
                DeclaredSymbols = declaredSymbols;
                TypeSymbols = typeSymbols;
                StaticAssignments = staticAssignments;
                Invocations = invocations;
                Attributes = attributes;
                AccessorType = accessorType;
            }

            public readonly ConcurrentDictionary<string, StringCollection> SyntaxTrees { get; }

            public readonly ConcurrentDictionary<string, SymbolInfo> DeclaredSymbols { get; }

            public readonly ConcurrentDictionary<string, SymbolInfo> TypeSymbols { get; }

            //================================================================================//

            public readonly ConcurrentDictionary<string, ConcurrentDictionary<string, AssignmentExpressionSyntax>> StaticAssignments { get; }

            public readonly ConcurrentDictionary<string, ConcurrentDictionary<string, SymbolInfo>> Invocations { get; }

            public readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, AttributeSyntax>>> Attributes { get; }

            public readonly Dictionary<string, (string key, string code)> AccessorType { get; }

            //public readonly IReadOnlyDictionary<string, string> BasrType { get; }
        }

        public readonly struct Names
        {
            public Names(string syntax, string symbol, string declaredFull, string declaredStandard, string assemblyName)
            {
                Syntax = syntax;
                Symbol = symbol;
                DeclaredFull = declaredFull;
                DeclaredStandard = declaredStandard;
                AssemblyName = assemblyName;
            }

            public readonly string Syntax { get; }

            public readonly string Symbol { get; }

            public readonly string DeclaredFull { get; }

            public readonly string DeclaredStandard { get; }

            public readonly string AssemblyName { get; }
        }

        public readonly struct SymbolInfo
        {
            readonly static SymbolInfo defaultValue = default;

            public bool IsNull() => this.Equals(defaultValue);

            public override string ToString() => SyntaxToCode.ToCode(Syntax);

            public SymbolInfo(SyntaxNode syntax, ISymbol symbol, ISymbol declared, ISymbol source, SyntaxNode references, bool isCustom, ConcurrentDictionary<string, ConcurrentDictionary<string, AttributeSyntax>> attributes, Names names, IEnumerable<ITypeSymbol> genericArguments = default)
            {
                Syntax = syntax;
                Symbol = symbol;
                Declared = declared;

                //Type = type;
                Source = source;
                References = references;// symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                IsCustom = isCustom;

                //Assignment = assignme-+nt;
                //Model = model;
                Attributes = attributes;
                Names = names;
                GenericArguments = genericArguments;
            }

            public SymbolInfo(SyntaxNode syntax, ISymbol symbol, ISymbol declared, ISymbol source, ConcurrentDictionary<string, ConcurrentDictionary<string, AttributeSyntax>> attributes, string assemblyName = default, IEnumerable<ITypeSymbol> genericArguments = default) : this(syntax, symbol, declared, source, default, default, attributes, default, genericArguments)
            {
                References = symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() ?? declared.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                IsCustom = !(References is null || !declared.DeclaringSyntaxReferences.Any());
                Names = new Names(syntax.GetFullName(), symbol?.GetFullName(), declared.GetFullName(), declared.GetFullNameStandardFormat(), assemblyName);
            }

            public SymbolInfo Set(SyntaxNode syntax = default, ISymbol symbol = default, ISymbol declared = default, ISymbol source = default, SyntaxNode references = default, bool? isCustom = default, Names? names = default, ConcurrentDictionary<string, ConcurrentDictionary<string, AttributeSyntax>> attributes = default, IEnumerable<ITypeSymbol> genericArguments = default) => new SymbolInfo(syntax ?? Syntax, symbol ?? Symbol, declared ?? Declared, source ?? Source, references ?? References, isCustom ?? IsCustom, attributes ?? Attributes, names ?? Names, genericArguments);

            public string GetFullName() => null != GenericArguments ? $"{Declared.GetFullName()}<{string.Join(", ", GenericArguments.Select(c => c.GetFullName()))}>" : null;

            public readonly SyntaxNode Syntax { get; }

            public readonly ISymbol Symbol { get; }

            public readonly ISymbol Declared { get; }

            public readonly ISymbol Source { get; }

            public readonly SyntaxNode References { get; }

            /// <summary>
            /// !(References is null || !declared.DeclaringSyntaxReferences.Any())
            /// </summary>
            public readonly bool IsCustom { get; }

            public readonly ConcurrentDictionary<string, ConcurrentDictionary<string, AttributeSyntax>> Attributes { get; }

            public readonly Names Names { get; }

            public readonly IEnumerable<ITypeSymbol> GenericArguments { get; }
        }

        /*
        static readonly Type[] baseTypes = new Type[] {
            //typeof(System.Object), typeof(System.Type), typeof(System.Decimal), typeof(System.Double), typeof(System.Single), typeof(System.UInt16), typeof(System.UInt32), typeof(System.UInt64), typeof(System.Int16), typeof(System.Int32), typeof(System.Int64), typeof(System.String), typeof(System.Char), typeof(System.SByte), typeof(System.Byte), typeof(System.Boolean), typeof(System.Delegate), typeof(System.MulticastDelegate), typeof(System.Enum), typeof(System.DateTime), typeof(System.DateTimeOffset), typeof(System.IDisposable), typeof(System.Array), typeof(System.Collections.IEnumerable), typeof(System.Collections.IEnumerator), typeof(System.DBNull), typeof(System.Threading.Tasks.Task), typeof(System.Threading.Tasks.ValueTask),
            typeof(System.Object[]), typeof(System.Type[]), typeof(System.Decimal[]), typeof(System.Double[]), typeof(System.Single[]), typeof(System.UInt16[]), typeof(System.UInt32[]), typeof(System.UInt64[]), typeof(System.Int16[]), typeof(System.Int32[]), typeof(System.Int64[]), typeof(System.String[]), typeof(System.Char[]), typeof(System.SByte[]), typeof(System.Byte[]), typeof(System.Boolean[]), typeof(System.Delegate[]), typeof(System.MulticastDelegate[]), typeof(System.Enum[]), typeof(System.DateTime[]),typeof(System.DateTimeOffset[]), typeof(System.DBNull[]),  typeof(System.Threading.Tasks.Task[]), typeof(System.Threading.Tasks.ValueTask[])
        };
        */

        public static void Init(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver))
            {
                return;
            }

            var compilation = context.Compilation;
            var references = compilation.ReferencedAssemblyNames.Select(c => c.Name);

            #region AddSource

            //add int type.
            compilation = compilation.AddSyntaxTree(@"namespace Business.SourceGenerator.Meta.Types { readonly struct BasrTypeDeclaration { readonly static System.Type[] basrType = new System.Type[] { typeof(int) }; } }", out _, context.ParseOptions);

            var businessSourceGeneratorDirectory = context.GetMSBuildProperty("Business_SourceGenerator");
            System.Diagnostics.Debug.WriteLine($"BusinessSourceGeneratorDirectory: {businessSourceGeneratorDirectory}");

            foreach (var item in references)
            {
                var value = context.GetMSBuildProperty(item.Replace(".", "_"));

                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                var src = System.IO.Path.Combine(System.IO.Path.GetFullPath(value), "src");

                if (!System.IO.Directory.Exists(src))
                {
                    continue;
                }

                System.Diagnostics.Debug.WriteLine($"ReferenceDirectory: {src}");
                foreach (var item2 in System.IO.Directory.GetFiles(src, "*.cs", System.IO.SearchOption.AllDirectories))
                {
                    compilation = compilation.AddSyntaxTree(System.IO.File.ReadAllText(item2), out _, context.ParseOptions, item2);
                }
            }

            #endregion

            #region Init

            foreach (var item in AnalysisInfo.SyntaxTrees)
            {
                if (!references.Contains(item.Key))
                {
                    AnalysisInfo.SyntaxTrees.TryRemove(item.Key, out _);
                }
            }

            foreach (var item in AnalysisInfo.DeclaredSymbols)
            {
                if (!references.Contains(item.Value.Names.AssemblyName))
                {
                    AnalysisInfo.DeclaredSymbols.TryRemove(item.Key, out _);
                }
            }

            foreach (var item in AnalysisInfo.TypeSymbols)
            {
                if (!references.Contains(item.Value.Names.AssemblyName))
                {
                    AnalysisInfo.TypeSymbols.TryRemove(item.Key, out _);
                }
            }

            AnalysisInfo.Attributes.Clear();
            AnalysisInfo.StaticAssignments.Clear();
            AnalysisInfo.Invocations.Clear();
            AnalysisInfo.AccessorType.Clear();

            //=======================add=======================//

            foreach (var item in compilation.SyntaxTrees)
            {
                var syntaxTrees = AnalysisInfo.SyntaxTrees.GetOrAdd(compilation.AssemblyName, new StringCollection());

                if (!syntaxTrees.Contains(item.FilePath))
                {
                    syntaxTrees.Add(item.FilePath);
                }

                var model = compilation.GetSemanticModel(item);
                var nodes = item.GetRoot().DescendantNodes();

                foreach (var item2 in nodes.AsParallel())
                {
                    InitSymbols(model, item2);
                }
            }

            foreach (var info in AnalysisInfo.DeclaredSymbols.Values.AsParallel())
            {
                if (info.Attributes.Any())
                {
                    foreach (var item2 in info.Attributes)
                    {
                        var attributes = AnalysisInfo.Attributes.GetOrAdd(item2.Key, c => new ConcurrentDictionary<string, ConcurrentDictionary<string, AttributeSyntax>>()).GetOrAdd(info.Names.Syntax, c => new ConcurrentDictionary<string, AttributeSyntax>());

                        foreach (var attr in item2.Value)
                        {
                            attributes.TryAdd(attr.Key, attr.Value);
                        }
                    }
                }

                switch (info.Syntax)
                {
                    case AssignmentExpressionSyntax node:
                        var assignment = node.Left.GetSymbolInfo();//.Symbol;

                        if (null != assignment.Symbol && assignment.Symbol.IsStatic)// && (assignment is IFieldSymbol || assignment is IPropertySymbol)
                        {
                            AnalysisInfo.StaticAssignments.GetOrAdd(assignment.Names.Symbol, c => new ConcurrentDictionary<string, AssignmentExpressionSyntax>()).AddOrUpdate(info.Names.Syntax, node, (x, y) => node);
                        }
                        break;
                    case InvocationExpressionSyntax node:
                        if (null != info.Symbol)
                        {
                            AnalysisInfo.Invocations.GetOrAdd(info.Symbol.GetFullNameOrig(), c => new ConcurrentDictionary<string, SymbolInfo>()).AddOrUpdate(info.Names.Syntax, info, (x, y) => info);
                        }
                        break;
                    default: break;
                }
            }

            #endregion
        }

        static void InitSymbols(SemanticModel model, SyntaxNode syntax)
        {
            switch (syntax.RawKind)
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
                case (int)SyntaxKind.ArrayType:
                case (int)SyntaxKind.PointerType:
                    {
                        ISymbol declared;

                        if (syntax.RawKind == (int)SyntaxKind.FieldDeclaration)
                        {
                            var field = syntax as FieldDeclarationSyntax;
                            var declaration = field.Declaration.Variables.First();
                            declared = model.GetDeclaredSymbol(declaration);
                        }
                        else
                        {
                            declared = model.GetDeclaredSymbol(syntax) ?? model.GetTypeInfo(syntax).Type;
                        }

                        if (declared is null || SymbolKind.ErrorType == declared.Kind)
                        {
                            return;
                        }

                        if (declared is ITypeSymbol typeSymbol && SpecialType.System_Void == typeSymbol.SpecialType)
                        {
                            return;
                        }

                        ISymbol source = null;
                        var parent = syntax.Parent;

                        switch (syntax.RawKind)
                        {
                            case (int)SyntaxKind.ClassDeclaration:
                            case (int)SyntaxKind.StructDeclaration:
                            case (int)SyntaxKind.InterfaceDeclaration:

                            case (int)SyntaxKind.PropertyDeclaration:
                            case (int)SyntaxKind.FieldDeclaration:

                            case (int)SyntaxKind.MethodDeclaration:
                            case (int)SyntaxKind.LocalFunctionStatement:

                            case (int)SyntaxKind.ContinueStatement:
                                source = declared.ContainingSymbol;
                                break;
                            default:
                                do
                                {
                                    switch (parent?.RawKind)
                                    {
                                        case (int)SyntaxKind.ContinueStatement:
                                        case (int)SyntaxKind.MethodDeclaration:
                                        case (int)SyntaxKind.LocalFunctionStatement:

                                        case (int)SyntaxKind.NamespaceDeclaration:
                                        case (int)SyntaxKind.ClassDeclaration:
                                        case (int)SyntaxKind.StructDeclaration:

                                        case (int)SyntaxKind.PropertyDeclaration:
                                            source = model.GetDeclaredSymbol(parent); break;
                                        case (int)SyntaxKind.FieldDeclaration:
                                            {
                                                var field = parent as FieldDeclarationSyntax;
                                                source = model.GetDeclaredSymbol(field.Declaration.Variables.First());
                                            }
                                            break;
                                        default: break;
                                    }
                                    parent = parent?.Parent;
                                } while (source is null && null != parent);

                                if (source is null)
                                {
                                    source = declared.ContainingSymbol;
                                }
                                break;
                        }

                        var symbol = model.GetSymbolInfo(syntax).Symbol;

                        var attrs = new ConcurrentDictionary<string, ConcurrentDictionary<string, AttributeSyntax>>();

                        if (syntax is MemberDeclarationSyntax member)
                        {
                            foreach (var item in member.AttributeLists)
                            {
                                foreach (var item2 in item.Attributes)
                                {
                                    if (model.GetSymbolInfo(item2).Symbol is IMethodSymbol method)
                                    {
                                        attrs.GetOrAdd(method.ReceiverType.GetFullName(), c => new ConcurrentDictionary<string, AttributeSyntax>()).AddOrUpdate(item2.GetFullName(), item2, (x, y) => item2);
                                    }
                                }
                            }
                        }

                        //var syntaxKey = syntax.GetFullName();

                        var info = new SymbolInfo(syntax, symbol, declared, source, attrs, model.Compilation.AssemblyName);

                        var syntaxKey = info.Names.Syntax;

                        //Add to TypeSymbols
                        if (declared is ITypeSymbol type)
                        {
                            //var key = declared.GetFullName();

                            AnalysisInfo.TypeSymbols.AddOrUpdate(info.Names.DeclaredStandard, info, (x, y) =>
                            {
                                if (info.Syntax.IsKind(SyntaxKind.InterfaceDeclaration) || info.Syntax.IsKind(SyntaxKind.StructDeclaration) || info.Syntax.IsKind(SyntaxKind.ClassDeclaration))
                                {
                                    //If it's Interface or Struct or Class, update the key, And replace the original info
                                    //If it is the definition of Interface or Struct or Class, update it
                                    //syntaxKey = y.Syntax.GetFullName();
                                    syntaxKey = y.Names.Syntax;
                                    return info;
                                }

                                return y;
                            });
                        }

                        //Add again, remove first, because of the same key, but the info may be updated
                        if (AnalysisInfo.DeclaredSymbols.ContainsKey(syntaxKey))
                        {
                            AnalysisInfo.DeclaredSymbols.TryRemove(syntaxKey, out _);
                        }
                        AnalysisInfo.DeclaredSymbols.TryAdd(info.Names.Syntax, info);
                    }
                    break;
                default: break;
            }
        }

        public static string GetMSBuildProperty(this GeneratorExecutionContext context, string name, string defaultValue = "") => context.AnalyzerConfigOptions.GlobalOptions.TryGetValue($"build_property.{name}", out var value) ? value : defaultValue;

        public static Compilation AddSyntaxTree(this GeneratorExecutionContext context, string code, string path = null) => context.Compilation.AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(code, context.ParseOptions).WithFilePath(path ?? Guid.NewGuid().ToString("N")));

        public static Compilation AddSyntaxTree(this Compilation compilation, string code, out SyntaxTree tree, ParseOptions options = null, string path = null)
        {
            tree = SyntaxFactory.ParseSyntaxTree(code, options).WithFilePath(path ?? Guid.NewGuid().ToString("N"));
            return compilation.AddSyntaxTrees(tree);
        }

        public static Compilation AddSyntaxTree(this Compilation compilation, string code, ParseOptions options = null, string path = null) => AddSyntaxTree(compilation, code, out _, options, path);

        #region GetFullName GetSymbolInfo GetDefinition

        public static SymbolInfo GetSymbolInfo(this SyntaxNode syntax) => GetSymbolInfo(syntax.GetFullName());

        public static SymbolInfo GetSymbolInfo(this string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                throw new ArgumentException(nameof(fullName));
            }

            if (AnalysisInfo.DeclaredSymbols.TryGetValue(fullName, out SymbolInfo info))
            {
                return info;
            }

            return default;
        }

        public static SymbolInfo GetSymbolInfo(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol is null)
            {
                throw new ArgumentNullException(nameof(typeSymbol));
            }

            if (AnalysisInfo.TypeSymbols.TryGetValue(typeSymbol.GetFullNameStandardFormat(), out SymbolInfo info))
            {
                return info;
            }

            return default;
        }

        public static IMethodSymbol GetDefinition(this IMethodSymbol method) => method?.ReducedFrom?.OriginalDefinition ?? method?.OriginalDefinition ?? method;

        static ISymbol GetDefinition(this ISymbol symbol) => symbol?.OriginalDefinition ?? symbol;

        public static string GetFullName(this SyntaxNode syntax)
        {
            if (syntax is null)
            {
                throw new ArgumentNullException(nameof(syntax));
            }

            return $"{syntax.SyntaxTree.FilePath}@{syntax.Span}@{syntax.RawKind}";
        }

        /*
        public static IEnumerable<string> GetGenericArgs(this ISymbol symbol)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            if (symbol is INamedTypeSymbol named)
            {
                return named.TypeArguments.Select(c => c.GetFullName());
            }

            return Array.Empty<string>();
        }
        */

        public static string GetFullNameOrig(this IMethodSymbol method) => GetDefinition(method).GetFullName();

        public static string GetFullNameOrig(this ISymbol symbol) => GetDefinition(symbol).GetFullName();

        #endregion
    }
}