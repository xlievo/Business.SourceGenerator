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

    internal class GeneratorType
    {
        public enum TypeKeyFormat
        {
            No,
            ToLower,
            ToUpper
        }

        const string Key = "Business.SourceGenerator.Meta.GeneratorTypeAttribute";

        internal readonly struct MakeGenericType
        {
            public readonly IEnumerable<INamedTypeSymbol> definitions;

            public readonly IEnumerable<INamedTypeSymbol> references;

            public MakeGenericType(IEnumerable<INamedTypeSymbol> definitions, IEnumerable<INamedTypeSymbol> references)
            {
                this.definitions = definitions;
                this.references = references;
            }
        }

        public static IDictionary<string, MakeGenericType> GetMakeGenericTypes(MetaData.AnalysisInfoModel analysisInfo)
        {
            var makeGenericTypes = new Dictionary<string, MakeGenericType>();

            foreach (var info in analysisInfo.Attributes)
            {
                if (Key == info.Key)
                {
                    foreach (var item in info.Value)
                    {
                        var declarationInfo = item.Key.GetSymbolInfo();

                        var key = declarationInfo.Declared.GetFullNameStandardFormat();

                        if (makeGenericTypes.ContainsKey(key))
                        {
                            continue;
                        }

                        //IResult<DataType, DataType2, DataType3>
                        if (!(declarationInfo.Declared is INamedTypeSymbol declared) || !declared.IsGenericType || declared.IsUnboundGenericType || (TypeKind.Interface != declared.TypeKind && TypeKind.Class != declared.TypeKind))
                        {
                            continue;
                        }

                        var definitions = new Dictionary<string, INamedTypeSymbol>();
                        var references = new Dictionary<string, INamedTypeSymbol>();

                        var typeSymbols = GetTypeReference(analysisInfo.TypeSymbols, declared);//, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration
                                                                                               //var references = GetTypeReference(analysisInfo.DeclaredSymbols, declared).Where(c => c.Value is INamedTypeSymbol named && !named.TypeArguments.Any(c2 => TypeKind.TypeParameter == c2.TypeKind)).OrderBy(c => c.Value.GetFullName());

                        foreach (var reference in typeSymbols)
                        {
                            if (!(reference is INamedTypeSymbol named))
                            {
                                continue;
                            }

                            if (named.IsDefinition)
                            {
                                if ((TypeKind.Class == named.TypeKind || TypeKind.Struct == named.TypeKind) && !named.IsAbstract)
                                {
                                    var name = named.GetFullName();

                                    if (!definitions.ContainsKey(name))
                                    {
                                        definitions.Add(name, named);
                                    }
                                }
                            }
                            else if (named.IsGenericType)
                            {
                                //var args = named.TypeArguments.Select(c => TypeKind.TypeParameter == c.TypeKind ? Expression.GetTypeParameter(c as ITypeParameterSymbol)?.GetFullNameStandardFormat() ?? "System.Object" : c.GetFullNameStandardFormat());

                                //var name = GetGenericTypeName(named.GetFullNameStandardFormat(), args);

                                var name = named.GetFullName(new Expression.GetFullNameOpt(standardFormat: true, args: true));

                                if (!references.ContainsKey(name))
                                {
                                    references.Add(name, named);
                                }
                            }
                        }

                        makeGenericTypes.Add(key, new MakeGenericType(definitions.Values, references.Values));
                    }
                }
            }

            return makeGenericTypes;
        }

        static IEnumerable<ITypeSymbol> GetTypeReference(ConcurrentDictionary<string, MetaData.SymbolInfo> symbols, ITypeSymbol typeSymbol)
        {
            if (typeSymbol is null)
            {
                throw new ArgumentNullException(nameof(typeSymbol));
            }

            //if (TypeKind.Interface != typeSymbol.TypeKind && TypeKind.Class != typeSymbol.TypeKind)
            //{
            //    return Array.Empty<ITypeSymbol>();
            //}

            var typeSymbolName = typeSymbol.OriginalDefinition.GetFullName();

            var dict = new Dictionary<string, ITypeSymbol>();

            foreach (var item in symbols.Values)
            {
                if (!(item.Declared is ITypeSymbol declared))
                {
                    continue;
                }

                if (TypeKind.Interface != declared.TypeKind && TypeKind.Class != declared.TypeKind && TypeKind.Struct != declared.TypeKind)
                {
                    continue;
                }

                if (declared is INamedTypeSymbol namedType && namedType.IsUnboundGenericType)
                {
                    continue;
                }

                switch (typeSymbol.TypeKind)
                {
                    case TypeKind.Class:
                        //var baseType = declared.BaseType;
                        var baseType = declared;

                        while (null != baseType)
                        {
                            //if (baseType.OriginalDefinition.Equals(typeSymbol.OriginalDefinition, SymbolEqualityComparer.Default))
                            if (Expression.Equals(baseType.OriginalDefinition, typeSymbolName))
                            {
                                var key = declared.GetFullNameStandardFormat();

                                if (!dict.ContainsKey(key))
                                {
                                    dict.Add(key, declared);
                                }

                                break;
                            }
                            baseType = baseType.BaseType;
                        }
                        break;
                    case TypeKind.Interface:
                        //if (typeSymbol2.OriginalDefinition.Equals(typeSymbol.OriginalDefinition) || typeSymbol2.AllInterfaces.Any(c => typeSymbol.OriginalDefinition.Equals(c.OriginalDefinition, SymbolEqualityComparer.Default)))
                        if (Expression.Equals(declared.OriginalDefinition, typeSymbolName) || declared.AllInterfaces.Any(c => Expression.Equals(c.OriginalDefinition, typeSymbolName)))
                        {
                            var key = declared.GetFullNameStandardFormat();

                            if (!dict.ContainsKey(key))
                            {
                                dict.Add(key, declared);
                            }
                        }
                        break;
                    default: break;
                }
            }

            if (0 < dict.Count)
            {
                return dict.Values;//list.Distinct(typeSymbolEquality);
            }

            return Array.Empty<ITypeSymbol>();
        }

        public static string GetGenericTypeName(string name, IEnumerable<string> typeArgument) => GetGenericTypeName(name, typeArgument.ToArray());

        public static string GetGenericTypeName(string name, params string[] typeArgument)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }

            if (typeArgument is null)
            {
                throw new ArgumentNullException(nameof(typeArgument));
            }

            var brackLeft = name.IndexOf('<');

            if (-1 < brackLeft)
            {
                name = name.Substring(0, brackLeft);
            }

            return $"{name}<{string.Join(", ", typeArgument)}>";
        }
    }

    internal class MetaData
    {
        public static AnalysisInfoModel AnalysisInfo = new AnalysisInfoModel(new ConcurrentDictionary<string, StringCollection>(), new ConcurrentDictionary<string, SymbolInfo>(), new ConcurrentDictionary<string, SymbolInfo>(), new ConcurrentDictionary<string, ConcurrentDictionary<string, AssignmentExpressionSyntax>>(), new ConcurrentDictionary<string, ConcurrentDictionary<string, SymbolInfo>>(), new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, AttributeSyntax>>>());

        public readonly struct AnalysisInfoModel
        {
            internal AnalysisInfoModel(ConcurrentDictionary<string, StringCollection> syntaxTrees, ConcurrentDictionary<string, SymbolInfo> declaredSymbols, ConcurrentDictionary<string, SymbolInfo> typeSymbols, ConcurrentDictionary<string, ConcurrentDictionary<string, AssignmentExpressionSyntax>> staticAssignments, ConcurrentDictionary<string, ConcurrentDictionary<string, SymbolInfo>> invocations, ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, AttributeSyntax>>> attributes)
            {
                SyntaxTrees = syntaxTrees;
                DeclaredSymbols = declaredSymbols;
                TypeSymbols = typeSymbols;
                StaticAssignments = staticAssignments;
                Invocations = invocations;
                Attributes = attributes;
            }

            public ConcurrentDictionary<string, StringCollection> SyntaxTrees { get; }

            public ConcurrentDictionary<string, SymbolInfo> DeclaredSymbols { get; }

            public ConcurrentDictionary<string, SymbolInfo> TypeSymbols { get; }

            //================================================================================//

            public ConcurrentDictionary<string, ConcurrentDictionary<string, AssignmentExpressionSyntax>> StaticAssignments { get; }

            public ConcurrentDictionary<string, ConcurrentDictionary<string, SymbolInfo>> Invocations { get; }

            public ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, AttributeSyntax>>> Attributes { get; }
        }

        public readonly struct SymbolInfo
        {
            readonly static SymbolInfo defaultValue = default;

            public bool IsNull() => this.Equals(defaultValue);

            public override string ToString() => Expression.ToCode(Syntax);

            public SymbolInfo(SyntaxNode syntax, ISymbol symbol, ISymbol declared, ISymbol source, SyntaxNode references, ConcurrentDictionary<string, ConcurrentDictionary<string, AttributeSyntax>> attributes, string assemblyName = null, IEnumerable<ITypeSymbol> genericArguments = null)
            {
                Syntax = syntax;
                Symbol = symbol;
                Declared = declared;
                //Type = type;
                Source = source;
                References = references;// symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                                        //Assignment = assignme-+nt;
                                        //Model = model;
                Attributes = attributes;
                AssemblyName = assemblyName;
                GenericArguments = genericArguments;
            }

            public SymbolInfo Clone(IEnumerable<ITypeSymbol> genericArguments) => new SymbolInfo(Syntax, Symbol, Declared, Source, References, Attributes, AssemblyName, genericArguments);

            public string GetFullName() => null != GenericArguments ? $"{Declared.GetFullName()}<{string.Join(", ", GenericArguments.Select(c => c.GetFullName()))}>" : null;

            public SyntaxNode Syntax { get; }

            public ISymbol Symbol { get; }

            public ISymbol Declared { get; }

            public ISymbol Source { get; }

            public SyntaxNode References { get; }

            public ConcurrentDictionary<string, ConcurrentDictionary<string, AttributeSyntax>> Attributes { get; }

            public IEnumerable<ITypeSymbol> GenericArguments { get; }

            public string AssemblyName { get; }
        }

        public static void Init(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver))
            {
                return;
            }

            var compilation = context.Compilation;
            var references = compilation.ReferencedAssemblyNames.Select(c => c.Name);

            #region AddSource

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
                if (!references.Contains(item.Value.AssemblyName))
                {
                    AnalysisInfo.DeclaredSymbols.TryRemove(item.Key, out _);
                }
            }

            foreach (var item in AnalysisInfo.TypeSymbols)
            {
                if (!references.Contains(item.Value.AssemblyName))
                {
                    AnalysisInfo.TypeSymbols.TryRemove(item.Key, out _);
                }
            }

            AnalysisInfo.Attributes.Clear();
            AnalysisInfo.StaticAssignments.Clear();
            AnalysisInfo.Invocations.Clear();

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
                        var attributes = AnalysisInfo.Attributes.GetOrAdd(item2.Key, c => new ConcurrentDictionary<string, ConcurrentDictionary<string, AttributeSyntax>>()).GetOrAdd(info.Syntax.GetFullName(), c => new ConcurrentDictionary<string, AttributeSyntax>());

                        foreach (var attr in item2.Value)
                        {
                            attributes.TryAdd(attr.Key, attr.Value);
                        }
                    }
                }

                switch (info.Syntax)
                {
                    case AssignmentExpressionSyntax node:
                        var assignment = node.Left.GetSymbolInfo().Symbol;

                        if (null != assignment && assignment.IsStatic)// && (assignment is IFieldSymbol || assignment is IPropertySymbol)
                        {
                            AnalysisInfo.StaticAssignments.GetOrAdd(assignment.GetFullName(), c => new ConcurrentDictionary<string, AssignmentExpressionSyntax>()).AddOrUpdate(node.GetFullName(), node, (x, y) => node);
                        }
                        break;
                    case InvocationExpressionSyntax node:
                        if (null != info.Symbol)
                        {
                            AnalysisInfo.Invocations.GetOrAdd(info.Symbol.GetFullNameOrig(), c => new ConcurrentDictionary<string, SymbolInfo>()).AddOrUpdate(info.Syntax.GetFullName(), info, (x, y) => info);
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

                        if (null != declared)
                        {
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
                                    } while (null == source && null != parent);

                                    if (null == source)
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

                            var syntaxKey = syntax.GetFullName();

                            var info = new SymbolInfo(syntax, symbol, declared, source, symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() ?? declared.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(), attrs, model.Compilation.AssemblyName);

                            if (declared is ITypeSymbol type)
                            {
                                var key = declared.GetFullName();

                                AnalysisInfo.TypeSymbols.AddOrUpdate(key, info, (x, y) =>
                                {
                                    if (info.Syntax.IsKind(SyntaxKind.InterfaceDeclaration) || info.Syntax.IsKind(SyntaxKind.StructDeclaration) || info.Syntax.IsKind(SyntaxKind.ClassDeclaration))
                                    {
                                        syntaxKey = y.Syntax.GetFullName();
                                        return info;
                                    }

                                    return y;
                                });
                            }

                            if (AnalysisInfo.DeclaredSymbols.ContainsKey(syntaxKey))
                            {
                                AnalysisInfo.DeclaredSymbols.TryRemove(syntaxKey, out _);
                            }
                            AnalysisInfo.DeclaredSymbols.TryAdd(syntax.GetFullName(), info);
                        }
                    }
                    break;
                default: break;
            }
        }
    }

    internal static class Expression
    {
        enum AsyncType
        {
            None,
            Task,
            TaskGeneric,
            ValueTask,
            ValueTaskGeneric,
            Other,
        }

        public static void Log(this GeneratorExecutionContext context, string message, DiagnosticSeverity diagnostic = DiagnosticSeverity.Warning) => context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("Business.SourceGenerator", string.Empty, message, string.Empty, diagnostic, true), Location.None));

        public static string GetMSBuildProperty(this GeneratorExecutionContext context, string name, string defaultValue = "") => context.AnalyzerConfigOptions.GlobalOptions.TryGetValue($"build_property.{name}", out var value) ? value : defaultValue;

        #region GetFullName GetSymbolInfo GetDefinition

        public static MetaData.SymbolInfo GetSymbolInfo(this SyntaxNode syntax) => GetSymbolInfo(syntax.GetFullName());

        public static MetaData.SymbolInfo GetSymbolInfo(this string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                throw new ArgumentException(nameof(fullName));
            }

            if (MetaData.AnalysisInfo.DeclaredSymbols.TryGetValue(fullName, out MetaData.SymbolInfo info))
            {
                return info;
            }

            return default;
        }

        public static IMethodSymbol GetDefinition(this IMethodSymbol method) => GetDefinition(method as ISymbol) as IMethodSymbol;

        static ISymbol GetDefinition(this ISymbol symbol)
        {
            if (symbol is IMethodSymbol method)
            {
                return method?.ReducedFrom?.OriginalDefinition ?? method?.OriginalDefinition ?? symbol;
            }

            return symbol?.OriginalDefinition ?? symbol;
        }

        public static string GetFullName(this SyntaxNode syntax)
        {
            if (syntax is null)
            {
                throw new ArgumentNullException(nameof(syntax));
            }

            return $"{syntax.SyntaxTree.FilePath}@{syntax.Span}@{syntax.RawKind}";
        }

        public readonly struct GetFullNameOpt
        {
            public GetFullNameOpt(bool noArgs = false, bool args = false, bool standardFormat = false, bool unboundGenericType = false, bool parameters = false, bool noPrefix = false, bool prefix = false)
            {
                NoArgs = noArgs;
                Args = args;
                StandardFormat = standardFormat;
                UnboundGenericType = unboundGenericType;
                Parameters = parameters;
                NoPrefix = noPrefix;
                Prefix = prefix;
            }

            public bool NoArgs { get; }

            public bool Args { get; }

            public bool StandardFormat { get; }

            public bool UnboundGenericType { get; }

            public bool Parameters { get; }

            public bool NoPrefix { get; }

            public bool Prefix { get; }
        }

        static readonly GetFullNameOpt standardFormatOpt = new GetFullNameOpt(standardFormat: true);

        public static string GetFullNameStandardFormat(this ISymbol symbol, Func<string, string> typeClean = default) => GetFullName(symbol, standardFormatOpt, typeClean);

        public static string GetFullName(this ISymbol symbol, GetFullNameOpt opt = default, Func<string, string> typeClean = default)
        {
            Func<string, string> typeClean2 = name => typeClean?.Invoke(name) ?? name;

            const string objectName = nameof(System) + "." + nameof(Object);
            var objectNameClean = typeClean2(objectName);
            var optArgs = opt.Args;
            if (opt.Args)
            {
                opt = new GetFullNameOpt(opt.NoArgs, false, opt.StandardFormat, opt.UnboundGenericType, opt.Parameters, opt.NoPrefix, opt.Prefix);
            }

            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            if (symbol is INamedTypeSymbol anonymousType && anonymousType.IsAnonymousType)
            {
                return string.Join(", ", anonymousType.Constructors.Select(c =>
                {
                    var parameters = c.Parameters.Select(c2 =>
                    {
                        var type = TypeKind.Dynamic == c2.Type.TypeKind && opt.StandardFormat ? objectNameClean : TypeKind.TypeParameter == c2.Type.TypeKind ? c2.Type.Name : GetFullName(c2.Type, opt, typeClean2);

                        return opt.StandardFormat ? type : $"{type} {c2.Name}";
                    });

                    return $"({string.Join(", ", parameters)})";
                }));
            }

            if (symbol is IArrayTypeSymbol array)
            {
                var rank = "[]";

                if (1 < array.Rank)
                {
                    rank = $"[{string.Join(string.Empty, Enumerable.Repeat(",", array.Rank))}]";
                }

                if (TypeKind.TypeParameter == array.ElementType.TypeKind)
                {
                    return $"{array.ElementType.Name}{rank}";
                }

                return $"{GetFullName(array.ElementType, opt, typeClean2)}{rank}";
            }

            if (symbol is IPointerTypeSymbol pointer)
            {
                var rank = "*";

                if (TypeKind.TypeParameter == pointer.PointedAtType.TypeKind)
                {
                    return $"{pointer.PointedAtType.Name}{rank}";
                }

                return $"{GetFullName(pointer.PointedAtType, opt, typeClean2)}{rank}";
            }
            //=============================================//

            //var prefix = $"{symbol.ContainingType?.ToDisplayString() ?? (null != symbol.ContainingNamespace && !symbol.ContainingNamespace.IsGlobalNamespace ? symbol.ContainingNamespace.ToDisplayString() : null)}";
            string prefix = default;

            if (null != symbol.ContainingType)
            {
                prefix = GetFullName(symbol.ContainingType, opt, typeClean2);

            }
            else if (null != symbol.ContainingNamespace && !symbol.ContainingNamespace.IsGlobalNamespace)
            {
                prefix = symbol.ContainingNamespace.ToDisplayString();
            }

            string prefix2 = default;

            if (opt.Prefix)
            {
                return prefix;
            }

            if (opt.NoPrefix)
            {
                prefix = default;
            }

            if (!string.IsNullOrEmpty(prefix))
            {
                prefix2 = $"{prefix}.";
            }

            //if (!opt.NoPrefix && !string.IsNullOrEmpty(prefix))
            //{
            //    prefix = $"{prefix}";
            //}
            //else
            //{
            //    prefix = null;
            //}

            if (opt.NoArgs)
            {
                return typeClean2($"{prefix2}{symbol.Name}");
            }

            if (SymbolKind.TypeParameter == symbol.Kind)
            {
                if (symbol.ContainingSymbol is IMethodSymbol)
                {
                    return typeClean2($"{GetFullName(symbol.ContainingSymbol, opt)}.{symbol.Name}");
                }

                return typeClean2($"{prefix2}{symbol.Name}");
            }

            var named = symbol as INamedTypeSymbol;

            string nullable = default;

            if (symbol is ITypeSymbol type && NullableAnnotation.Annotated == type.NullableAnnotation)
            {
                nullable = opt.StandardFormat ? string.Empty : "?";

                if ("System.Nullable".Equals($"{prefix2}{symbol.Name}") && 0 < named?.TypeArguments.Length)
                {
                    return $"{GetFullName(named.TypeArguments[0], opt, typeClean2)}{nullable}";
                }
            }

            string args = default;

            if (null != named && 0 < named.TypeArguments.Length)
            {
                if (named.IsUnboundGenericType)
                {
                    args = $"<{string.Join(",", Enumerable.Repeat(string.Empty, named.TypeArguments.Length))}>";
                }
                else if (named.IsTupleType)
                {
                    //if ("System.ValueTuple".Equals($"{prefix}.{symbol.Name}") && 0 < named?.TupleElements.Length)
                    //{
                    //    return $"({string.Join(", ", named.TupleElements.Select(c => (c.IsExplicitlyNamedTupleElement && !opt.StandardFormat) ? $"{(Microsoft.CodeAnalysis.TypeKind.Dynamic == c.Type.TypeKind && opt.StandardFormat ? objectName : GetFullName(c.Type, opt))} {c.Name}" : (Microsoft.CodeAnalysis.TypeKind.Dynamic == c.Type.TypeKind && opt.StandardFormat ? objectName : GetFullName(c.Type, opt))))})";
                    //}

                    if (0 < named?.TupleElements.Length)
                    {
                        args = $"<{string.Join(", ", named.TupleElements.Select(c => $"{(TypeKind.Dynamic == c.Type.TypeKind ? objectNameClean : GetFullName(c.Type, opt, typeClean2))}"))}>";
                    }
                }
                else
                {
                    args = opt.UnboundGenericType ? $"<{string.Join(",", Enumerable.Repeat(string.Empty, named.TypeArguments.Length))}>" : $"<{string.Join(", ", named.TypeArguments.Select(c => (TypeKind.Dynamic == c.TypeKind && opt.StandardFormat ? objectNameClean : TypeKind.TypeParameter == c.TypeKind ? c.Name : GetFullName(c, opt, typeClean2))))}>";
                }
            }

            var prefixFullName = $"{prefix2}{symbol.Name}";
            string parameters = default;

            if (symbol is IMethodSymbol method)
            {
                //parameters = $"({string.Join(", ", method.Parameters.Select(c => TypeKind.Dynamic == c.Type.TypeKind && opt.StandardFormat ? objectName : TypeKind.TypeParameter == c.Type.TypeKind ? $"{method.Name}.{c.Type.Name}" : GetFullName(c.Type, opt)))})";

                parameters = string.Join(", ", method.Parameters.Select(c =>
                {
                    if (TypeKind.Dynamic == c.Type.TypeKind && opt.StandardFormat)
                    {
                        return objectNameClean;
                    }

                    if (c.Type is ITypeParameterSymbol typeParameter)
                    {
                        switch (typeParameter.TypeParameterKind)
                        {
                            case TypeParameterKind.Type:
                                return $"{method.ContainingType.Name}.{typeParameter.Name}";
                            case TypeParameterKind.Method:
                                return $"{method.Name}.{typeParameter.Name}";
                            default: return typeParameter.Name;
                        }
                    }

                    return GetFullName(c.Type, opt, typeClean2);
                }));

                parameters = $"({parameters})";

                if (opt.Parameters)
                {
                    return parameters;
                }

                if (0 < method.TypeArguments.Length)
                {
                    args = opt.UnboundGenericType ? $"<{string.Join(",", Enumerable.Repeat(string.Empty, named.TypeArguments.Length))}>" : $"<{string.Join(", ", method.TypeArguments.Select(c => (TypeKind.Dynamic == c.TypeKind && opt.StandardFormat ? objectNameClean : TypeKind.TypeParameter == c.TypeKind ? c.Name : GetFullName(c, opt, typeClean2))))}>";
                }

                if (MethodKind.Constructor == method.MethodKind || MethodKind.StaticConstructor == method.MethodKind)
                {
                    prefixFullName = prefix;
                }
            }

            if (optArgs)
            {
                return args;
            }

            return typeClean2($"{prefixFullName}{args}{nullable}{parameters}");

            //return symbol.ToDisplayString();
        }

        readonly static Dictionary<string, string> systemTypeKeywords = new Dictionary<string, string>
        {
            ["System.Char"] = "char",
            ["System.String"] = "string",
            ["System.Boolean"] = "bool",
            ["System.SByte"] = "sbyte",
            ["System.Byte"] = "byte",
            ["System.Decimal"] = "decimal",
            ["System.Double"] = "double",
            ["System.Single"] = "float",
            ["System.Int16"] = "short",
            ["System.Int32"] = "int",
            ["System.Int64"] = "long",
            ["System.UInt16"] = "ushort",
            ["System.UInt32"] = "uint",
            ["System.UInt64"] = "ulong",
            ["System.Object"] = "object",
            ["void"] = "void",

            ["Char"] = "char",
            ["String"] = "string",
            ["Boolean"] = "bool",
            ["SByte"] = "sbyte",
            ["Byte"] = "byte",
            ["Decimal"] = "decimal",
            ["Double"] = "double",
            ["Single"] = "float",
            ["Int16"] = "short",
            ["Int32"] = "int",
            ["Int64"] = "long",
            ["UInt16"] = "ushort",
            ["UInt32"] = "uint",
            ["UInt64"] = "ulong",
            ["Object"] = "object",
        };

        public static string TypeNameClean(string value, params string[] skip)
        {
            if (systemTypeKeywords.TryGetValue(value, out string typeKeyword))
            {
                value = typeKeyword;
                return value;
            }

            if (skip?.Any() ?? false)
            {
                foreach (var item in skip)
                {
                    if (value.StartsWith(item))
                    {
                        value = value.Substring(item.Length);

                        var value2 = value;

                        var brackLeft = value2.IndexOf('<');
                        var brackRight = value2.LastIndexOf('>');

                        while (-1 != brackLeft && -1 != brackRight)
                        {
                            value2 = value2.Substring(0, brackLeft) + value2.Substring(brackRight + 1, value.Length - brackRight - 1);
                            brackLeft = value2.IndexOf('<');
                            brackRight = value2.LastIndexOf('>');
                        }

                        if (1 == value2.Split('.').Length)
                        {
                            break;
                        }
                    }
                }
            }

            const string System = "System.";

            if (value.StartsWith(System) && 2 == value.Split('.').Length)
            {
                value = value.Substring(System.Length);
            }

            return value;
        }

        public static IEnumerable<string> GetGenericArgs(this ISymbol symbol)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            if (symbol is INamedTypeSymbol named)
            {
                return named.TypeArguments.Select(c => GetFullName(c));
            }

            return Array.Empty<string>();
        }

        public static string GetFullNameOrig(this IMethodSymbol method) => GetFullName(GetDefinition(method));

        public static string GetFullNameOrig(this ISymbol symbol) => GetFullName(GetDefinition(symbol));

        #endregion

        #region ToCode

        public readonly struct ToCodeOpt
        {
            public ToCodeOpt(Func<SyntaxNode, string, string> replace = null, bool standardFormat = false)
            {
                Replace = replace;
                StandardFormat = standardFormat;
            }

            public Func<SyntaxNode, string, string> Replace { get; }

            public bool StandardFormat { get; }
        }

        /// <summary>
        /// Returns the string representation of this node, not including its leading and trailing trivia.
        /// <para>
        /// replace (node, value) => value
        /// </para>
        /// </summary>
        /// <param name="syntaxNode"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public static string ToCode(this SyntaxNode syntaxNode, ToCodeOpt opt = default)
        {
            if (syntaxNode is null)
            {
                return null;
            }

            var newLine = opt.StandardFormat ? Environment.NewLine : " ";
            var newLine2 = opt.StandardFormat ? $"{newLine}{newLine}" : newLine;
            string value = null;

            switch (syntaxNode)
            {
                #region case
                case UsingDirectiveSyntax node:
                    value = $"{node.UsingKeyword} {node.Name}{node.SemicolonToken}"; break;
                case BaseNamespaceDeclarationSyntax node:
                    string usings = null;

                    if (0 < node.Usings.Count)
                    {
                        usings = $"{string.Join(newLine, node.Usings.Select(c => ToCode(c, opt)))} ";
                    }

                    string members = null;

                    if (0 < node.Members.Count)
                    {
                        members = string.Join(newLine2, node.Members.Select(c => ToCode(c, opt)));
                    }

                    value = $"{usings}{node.NamespaceKeyword} {node.Name}{newLine}{{{newLine}{members}{newLine}}}";
                    break;
                case DefaultExpressionSyntax node:
                    value = $"{node.Keyword}{node.OpenParenToken}{ToCode(node.Type, opt)}{node.CloseParenToken}"; break;
                case ReturnStatementSyntax node:
                    value = node.ReturnKeyword.ToString();
                    if (null != node.Expression)
                    {
                        value = $"{value} {ToCode(node.Expression, opt)}";
                    }
                    value = $"{value}{Semicolon()}";
                    break;
                case BaseListSyntax node:
                    value = $"{node.ColonToken} {string.Join(", ", node.Types.Select(c => ToCode(c, opt)))}";
                    break;
                case ClassDeclarationSyntax node:
                    value = $"{node.Keyword} {node.Identifier}";

                    if (null != node.TypeParameterList)
                    {
                        value = $"{value}{ToCode(node.TypeParameterList, opt)}";
                    }

                    if (null != node.BaseList)
                    {
                        value = $"{value} {ToCode(node.BaseList, opt)}";
                    }

                    if (node.ConstraintClauses.Any())
                    {
                        value = $"{value} {string.Join(" ", node.ConstraintClauses.Select(c => ToCode(c, opt)))}";
                    }

                    if (node.Modifiers.Any())
                    {
                        value = $"{node.Modifiers} {value}";
                    }

                    value = $"{value}{newLine}{node.OpenBraceToken}";

                    if (node.Members.Any())
                    {
                        value = $"{value}{newLine}{string.Join(newLine2, node.Members.Select(c => ToCode(c, opt)))}";
                    }

                    value = $"{value}{newLine}{node.CloseBraceToken}";

                    break;
                case StructDeclarationSyntax node:
                    value = $"{node.Keyword} {node.Identifier}";

                    if (null != node.TypeParameterList)
                    {
                        value = $"{value}{ToCode(node.TypeParameterList, opt)}";
                    }

                    if (null != node.BaseList)
                    {
                        value = $"{value} {ToCode(node.BaseList, opt)}";
                    }

                    if (node.ConstraintClauses.Any())
                    {
                        value = $"{value} {string.Join(" ", node.ConstraintClauses.Select(c => ToCode(c, opt)))}";
                    }

                    if (node.Modifiers.Any())
                    {
                        value = $"{node.Modifiers} {value}";
                    }

                    value = $"{value}{newLine}{node.OpenBraceToken}";

                    if (node.Members.Any())
                    {
                        value = $"{value}{newLine}{string.Join(newLine2, node.Members.Select(c => ToCode(c, opt)))}";
                    }

                    value = $"{value}{newLine}{node.CloseBraceToken}";

                    break;
                case ConstructorInitializerSyntax node:
                    value = $"{node.ColonToken} {node.ThisOrBaseKeyword}{ToCode(node.ArgumentList, opt)}"; break;
                case ConstructorDeclarationSyntax node:
                    value = $"{GetSign(node, opt)}{GetBodyOrExpression(node, opt)}{Semicolon()}"; break;
                case MethodDeclarationSyntax node:
                    value = $"{GetSign(node, opt)}{GetBodyOrExpression(node, opt)}{Semicolon()}"; break;
                case LocalFunctionStatementSyntax node:
                    value = $"{GetSign(node, opt)}{GetBodyOrExpression(node, opt)}{Semicolon()}"; break;
                case PropertyDeclarationSyntax node:
                    value = $"{ToCode(node.Type, opt)} {ToCode(node.ExplicitInterfaceSpecifier, opt)}{node.Identifier} {ToCode(node.AccessorList, opt)}";

                    if (null != node.Initializer)
                    {
                        value = $"{value} {ToCode(node.Initializer, opt)}";
                    }

                    value = $"{value}{Semicolon()}";

                    if (node.Modifiers.Any())
                    {
                        value = $"{node.Modifiers} {value}";
                    }
                    break;
                case AccessorListSyntax node:
                    value = $"{node.OpenBraceToken} {string.Join(" ", node.Accessors.Select(c => ToCode(c, opt)))} {node.CloseBraceToken}";
                    break;
                case AccessorDeclarationSyntax node:
                    value = $"{node.Keyword}";

                    if (null != node.ExpressionBody)
                    {
                        value = $"{value}{ToCode(node.ExpressionBody, opt)}";
                    }
                    else if (null != node.Body)
                    {
                        value = $"{value} {ToCode(node.Body, opt)}";
                    }

                    value = $"{value}{Semicolon()}";

                    if (node.Modifiers.Any())
                    {
                        value = $"{node.Modifiers} {value}";
                    }

                    break;
                case FieldDeclarationSyntax node:
                    value = $"{ToCode(node.Declaration, opt)}{Semicolon()}";

                    if (node.Modifiers.Any())
                    {
                        value = $"{node.Modifiers} {value}";
                    }
                    break;
                case TryStatementSyntax node:
                    value = $"{node.TryKeyword} {ToCode(node.Block, opt)} {string.Join(" ", node.Catches.Select(c => ToCode(c, opt)))}";

                    if (null != node.Finally)
                    {
                        value = $"{value} {ToCode(node.Finally, opt)}";
                    }

                    break;
                case ThrowExpressionSyntax node:
                    value = $"{node.ThrowKeyword} {ToCode(node.Expression, opt)}"; break;
                case ThrowStatementSyntax node:
                    value = $"{node.ThrowKeyword} {ToCode(node.Expression, opt)}{node.SemicolonToken}"; break;
                case CatchClauseSyntax node:
                    value = $"{node.CatchKeyword}";

                    if (null != node.Declaration)
                    {
                        value = $"{value} {ToCode(node.Declaration, opt)}";
                    }
                    value = $"{value} {ToCode(node.Block, opt)}";

                    break;
                case CatchDeclarationSyntax node:
                    value = $"{node.OpenParenToken}{ToCode(node.Type, opt)} {node.Identifier}{node.CloseParenToken}"; break;
                //case CatchDeclarationSyntax node:
                //    value = null;
                //case CatchFilterClauseSyntax node:
                //    value = null;
                case FinallyClauseSyntax node:
                    value = $"{node.FinallyKeyword} {ToCode(node.Block, opt)}"; break;
                //case InterpolatedStringTextSyntax node: value = $"{node.TextToken}";
                case InterpolatedStringExpressionSyntax node:
                    value = $"{node.StringStartToken}{string.Join(string.Empty, node.Contents.Select(c => ToCode(c, opt)))}{node.StringEndToken}"; break;
                case InterpolationSyntax node:
                    //string.Format("{0}{1}", "aaa", "bbb");
                    //node.AlignmentClause node.FormatClause ??
                    //{expression[,alignment][:formatString]}
                    value = $"{node.OpenBraceToken}{ToCode(node.Expression, opt)}";

                    if (null != node.AlignmentClause)
                    {
                        value = $"{value}{ToCode(node.AlignmentClause, opt)}";
                    }
                    if (null != node.FormatClause)
                    {
                        value = $"{value}{ToCode(node.FormatClause, opt)}";
                    }

                    value = $"{value}{node.CloseBraceToken}";
                    break;
                case InterpolationAlignmentClauseSyntax node:
                    value = $"{node.CommaToken}{ToCode(node.Value, opt)}"; break;
                case InterpolationFormatClauseSyntax node:
                    value = $"{node.ColonToken}{node.FormatStringToken}"; break;
                case TupleElementSyntax node:
                    value = ToCode(node.Type, opt);

                    if (default != node.Identifier)
                    {
                        value = $"{value} {node.Identifier}";
                    }
                    break;
                case TupleExpressionSyntax node:
                    value = $"{node.OpenParenToken}{string.Join(", ", node.Arguments.Select(c => ToCode(c, opt)))}{node.CloseParenToken}"; break;
                case TupleTypeSyntax node:
                    value = $"{node.OpenParenToken}{string.Join(", ", node.Elements.Select(c => ToCode(c, opt)))}{node.CloseParenToken}"; break;
                case ClassOrStructConstraintSyntax node: value = $"{node.ClassOrStructKeyword}{node.QuestionToken}"; break;
                case TypeConstraintSyntax node: value = $"{ToCode(node.Type, opt)}"; break;
                case ConstructorConstraintSyntax node: value = $"{node.NewKeyword}{node.OpenParenToken}{node.CloseParenToken}"; break;
                case TypeParameterConstraintClauseSyntax node:
                    value = $"{node.WhereKeyword} {ToCode(node.Name, opt)} {node.ColonToken} {string.Join(", ", node.Constraints.Select(c => ToCode(c, opt)))}"; break;
                case CastExpressionSyntax node:
                    value = $"{node.OpenParenToken}{ToCode(node.Type, opt)}{node.CloseParenToken}{ToCode(node.Expression, opt)}"; break;
                case ParenthesizedExpressionSyntax node:
                    value = $"{node.OpenParenToken}{ToCode(node.Expression, opt)}{node.CloseParenToken}"; break;
                case CaseSwitchLabelSyntax node:
                    value = $"{node.Keyword} {ToCode(node.Value, opt)}{node.ColonToken}"; break;
                //case SwitchLabelSyntax node:
                //    value = $"{node.Keyword} {ToCode(node.l, opt)}{node.ColonToken}"; break;
                case CasePatternSwitchLabelSyntax node:
                    value = $"{node.Keyword} {ToCode(node.Pattern, opt)}";
                    if (null != node.WhenClause)
                    {
                        value = $"{value} {ToCode(node.WhenClause, opt)}";
                    }
                    value = $"{value}{node.ColonToken}";
                    break;
                case SwitchSectionSyntax node:
                    value = $"{string.Join(" ", node.Labels.Select(c => ToCode(c, opt)))} {string.Join(newLine, node.Statements.Select(c => ToCode(c, opt)))}";
                    break;
                case BreakStatementSyntax node: value = $"{node.BreakKeyword}{Semicolon()}"; break;
                //ExpressionStatementSyntax
                case SwitchStatementSyntax node:
                    value = $"{node.SwitchKeyword} {node.OpenParenToken}{ToCode(node.Expression, opt)}{node.CloseParenToken}{newLine}{node.OpenBraceToken}{newLine}{string.Join(newLine, node.Sections.Select(c => ToCode(c, opt)))}{newLine}{node.CloseBraceToken}"; break;
                case ForEachStatementSyntax node:
                    value = $"{node.ForEachKeyword} {node.OpenParenToken}{ToCode(node.Type, opt)} {node.Identifier} {node.InKeyword} {ToCode(node.Expression, opt)}{node.CloseParenToken} {ToCode(node.Statement, opt)}"; break;
                case ForStatementSyntax node:
                    value = $"{node.ForKeyword} {node.OpenParenToken}{ToCode(node.Declaration, opt)}{node.FirstSemicolonToken} {ToCode(node.Condition, opt)}{node.FirstSemicolonToken} {string.Join(" ", node.Incrementors.Select(c => ToCode(c, opt)))}{node.CloseParenToken} {ToCode(node.Statement, opt)}"; break;
                case PostfixUnaryExpressionSyntax node: value = $"{node.Operand}{node.OperatorToken}"; break;
                case PrefixUnaryExpressionSyntax node: value = $"{node.OperatorToken}{node.Operand}"; break;
                case ConditionalAccessExpressionSyntax node:
                    value = $"{ToCode(node.Expression, opt)}{node.OperatorToken}{ToCode(node.WhenNotNull, opt)}"; break;
                case ConditionalExpressionSyntax node:
                    value = $"{ToCode(node.Condition, opt)} {node.QuestionToken} {node.WhenTrue} {node.ColonToken} {node.WhenFalse}"; break;
                case IfStatementSyntax node:
                    value = $"{node.IfKeyword} {node.OpenParenToken}{ToCode(node.Condition, opt)}{node.CloseParenToken}{(node.Statement is BlockSyntax ? string.Empty : " ")}{ToCode(node.Statement, opt)}";

                    if (null != node.Else)
                    {
                        value = $"{value} {ToCode(node.Else, opt)}";
                    }
                    break;
                case ElseClauseSyntax node:
                    value = $"{node.ElseKeyword}{(node.Statement is BlockSyntax ? string.Empty : " ")}{ToCode(node.Statement, opt)}";
                    break;
                case BinaryExpressionSyntax node:
                    value = $"{ToCode(node.Left, opt)} {node.OperatorToken} {ToCode(node.Right, opt)}"; break;
                case AssignmentExpressionSyntax node:
                    value = $"{ToCode(node.Left, opt)} {node.OperatorToken} {ToCode(node.Right, opt)}"; break;
                case QualifiedNameSyntax node:
                    value = $"{ToCode(node.Left, opt)}{node.DotToken}{ToCode(node.Right, opt)}"; break;
                case NullableTypeSyntax node:
                    value = $"{ToCode(node.ElementType, opt)}{node.QuestionToken}"; break;
                case TypeArgumentListSyntax node:
                    value = $"{node.LessThanToken}{string.Join(", ", node.Arguments.Select(c => ToCode(c, opt)))}{node.GreaterThanToken}"; break;
                case TypeParameterListSyntax node:
                    if (node.Parameters.Any())
                    {
                        value = $"{node.LessThanToken}{string.Join(", ", node.Parameters.Select(c => ToCode(c, opt)))}{node.GreaterThanToken}";
                    }
                    break;
                case ArgumentListSyntax node:
                    value = $"{node.OpenParenToken}{string.Join(", ", node.Arguments.Select(c => ToCode(c, opt)))}{node.CloseParenToken}"; break;
                case ParenthesizedLambdaExpressionSyntax node:
                    {
                        value = $"{ToCode(node.ParameterList, opt)} {node.ArrowToken} {ToCode(node.Body, opt)}";

                        if (!node.AsyncKeyword.IsKind(SyntaxKind.None))
                        //if (null != node.AsyncKeyword)
                        {
                            value = $"{node.AsyncKeyword} {value}";
                        }
                    }
                    break;
                case ParameterListSyntax node:
                    value = $"{node.OpenParenToken}{string.Join(", ", node.Parameters.Select(c => ToCode(c, opt)))}{node.CloseParenToken}"; break;
                //case PredefinedTypeSyntax node:
                case ArrayRankSpecifierSyntax node:
                    value = $"{node.OpenBracketToken}{node.Sizes}{node.CloseBracketToken}"; break;
                case ArrayTypeSyntax node:
                    {
                        var array = node.RankSpecifiers.FirstOrDefault();

                        value = $"{ToCode(node.ElementType, opt)}{(null == array ? null : ToCode(array, opt))}";
                    }
                    break;
                case ArrayCreationExpressionSyntax node:
                    value = $"{node.NewKeyword} {ToCode(node.Type, opt)} {ToCode(node.Initializer, opt)}"; break;
                case ParameterSyntax node:
                    {
                        value = node.Identifier.ToString();

                        if (null != node.Type)
                        {
                            value = $"{ToCode(node.Type, opt)} {value}";
                        }

                        if (node.Modifiers.Any())
                        {
                            value = $"{node.Modifiers} {value}";
                        }

                        if (null != node.Default)
                        {
                            value = $"{value} {ToCode(node.Default, opt)}";
                        }
                    }
                    break;
                case ArgumentSyntax node:
                    {
                        value = $"{ToCode(node.Expression, opt)}";

                        //if (null != node.NameColon)
                        //{
                        //    value = $"{ToCode(node.NameColon, opt)} {value}";
                        //}

                        if (default == node.RefOrOutKeyword)
                        {
                            if (null != node.NameColon)
                            {
                                value = $"{ToCode(node.NameColon, opt)} {value}";
                            }
                        }
                        else
                        {
                            if (null != node.NameColon)
                            {
                                value = $"{value} {node.NameColon.Name}";
                            }

                            value = $"{node.RefOrOutKeyword} {value}";
                        }
                    }
                    break;
                case NameColonSyntax node:
                    value = $"{ToCode(node.Name, opt)}{node.ColonToken}"; break;
                case ElementAccessExpressionSyntax node:
                    value = $"{ToCode(node.Expression, opt)}{ToCode(node.ArgumentList, opt)}"; break;
                case BracketedArgumentListSyntax node:
                    value = $"{node.OpenBracketToken}{string.Join(", ", node.Arguments.Select(c => ToCode(c, opt)))}{node.CloseBracketToken}"; break;
                case EqualsValueClauseSyntax node:
                    value = $"{node.EqualsToken} {ToCode(node.Value, opt)}"; break;
                case MemberAccessExpressionSyntax node:
                    value = $"{node.Expression}{node.OperatorToken}{node.Name}"; break;
                case MemberBindingExpressionSyntax node:
                    value = $"{node.OperatorToken}{node.Name}"; break;
                case ExplicitInterfaceSpecifierSyntax node:
                    value = $"{ToCode(node.Name, opt)}{node.DotToken}"; break;
                case InvocationExpressionSyntax node:
                    value = $"{node.Expression}{node.ArgumentList.OpenParenToken}";
                    if (0 < node.ArgumentList.Arguments.Count)
                    {
                        value = $"{value}{string.Join(", ", node.ArgumentList.Arguments.Select(c => ToCode(c, opt)))}";
                    }
                    value = $"{value}{node.ArgumentList.CloseParenToken}";
                    break;
                case ObjectCreationExpressionSyntax node:
                    {
                        value = $"{node.NewKeyword} {ToCode(node.Type, opt)}";

                        if (null != node.ArgumentList)
                        {
                            value = $"{value}{node.ArgumentList.OpenParenToken}";

                            if (0 < node.ArgumentList.Arguments.Count)
                            {
                                value = $"{value}{string.Join(", ", node.ArgumentList.Arguments.Select(c => ToCode(c, opt)))}";
                            }

                            value = $"{value}{node.ArgumentList.CloseParenToken}";
                        }
                        else if (null != node.Initializer)
                        {
                            value = $"{value} {ToCode(node.Initializer, opt)}";
                        }
                    }
                    break;
                case InitializerExpressionSyntax node:
                    value = node.OpenBraceToken.ToString();

                    if (0 < node.Expressions.Count)
                    {
                        value = $"{value} {string.Join(", ", node.Expressions.Select(c => ToCode(c, opt)))}";
                    }

                    value = $"{value} {node.CloseBraceToken}";

                    break;
                case BlockSyntax node:
                    value = $"{newLine}{node.OpenBraceToken}";

                    if (node.Statements.Any())
                    {
                        value = $"{value}{newLine}{string.Join(newLine2, node.Statements.Select(c => ToCode(c, opt)))}";
                    }

                    value = $"{value}{newLine}{node.CloseBraceToken}";

                    break;
                case SimpleLambdaExpressionSyntax node:
                    value = $"{node.Parameter} {node.ArrowToken} {ToCode(node.Body, opt)}"; break;
                case AwaitExpressionSyntax node:
                    value = $"{node.AwaitKeyword} {ToCode(node.Expression, opt)}"; break;
                case ArrowExpressionClauseSyntax node:
                    value = $" {node.ArrowToken} {ToCode(node.Expression, opt)}"; break;
                case TypeOfExpressionSyntax node:
                    value = $"{node.Keyword}{node.OpenParenToken}{ToCode(node.Type, opt)}{node.CloseParenToken}"; break;
                case VariableDeclaratorSyntax node:
                    value = $"{node.Identifier}";

                    if (null != node.Initializer)
                    {
                        value = $"{value} {ToCode(node.Initializer, opt)}";
                    }
                    if (null != node.ArgumentList)
                    {
                        value = $"{value}{ToCode(node.ArgumentList, opt)}";
                    }

                    break;
                case GenericNameSyntax node:
                    value = $"{node.Identifier}";
                    //value = $"{GetPrefix(node)}";

                    if (node.TypeArgumentList.Arguments.Any())
                    {
                        value = $"{value}{ToCode(node.TypeArgumentList, opt)}";
                    }

                    //if (node.IsUnboundGenericName)??
                    //{

                    //}

                    break;
                //IsPatternExpressionSyntax IsPatternExpression 
                case IsPatternExpressionSyntax node:
                    value = $"{ToCode(node.Expression, opt)} {node.IsKeyword} {ToCode(node.Pattern, opt)}"; break;
                #endregion

                #region default

                default:
                    if (syntaxNode.ChildNodes().Any())
                    {
                        switch (syntaxNode)
                        {
                            //ExplicitInterfaceSpecifier = ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier
                            //ArrowExpressionClauseSyntax
                            //case MethodDeclarationSyntax node:
                            //    value = $"{node.Identifier} "; break;
                            //case ArrowExpressionClauseSyntax node:
                            //    value = $"{node.ArrowToken} "; break;
                            //case ExplicitInterfaceSpecifierSyntax node:
                            //    code = $"{node.Name}{node.DotToken}"; break;
                            //case SimpleLambdaExpressionSyntax node:
                            //    return $"{node.Parameter} {node.ArrowToken}";
                            //case ThrowStatementSyntax node:
                            //    value = $"{node.ThrowKeyword} "; break;
                            //case ReturnStatementSyntax node:
                            //    value = $"{node.ReturnKeyword} "; break;
                            //case ObjectCreationExpressionSyntax node:
                            //    code = $"{node.NewKeyword} "; break;
                            //case GenericNameSyntax node:
                            //    value = node.Identifier.ToString(); break;
                            case IdentifierNameSyntax node:
                                value = $"{node.Identifier} "; break;
                            //value = $"{GetPrefix(node)} "; break;
                            //GetPrefix
                            //case VariableDeclaratorSyntax node:
                            //    value = $"{node.Identifier} "; break;
                            //case EqualsValueClauseSyntax node:
                            //    code = $"{node.EqualsToken} "; break;
                            default: break;
                        }

                        value = $"{value}{string.Join(" ", syntaxNode.ChildNodes().Select(c => ToCode(c, opt)))}{Semicolon()}";
                    }
                    else
                    {
                        value = $"{syntaxNode}";
                    }

                    break;

                    #endregion
            }

            if (null != opt.Replace)
            {
                return opt.Replace(syntaxNode, value);
            }

            return value;

            SyntaxToken Semicolon()
            {
                switch (syntaxNode)
                {
                    case ClassDeclarationSyntax node:
                        return node.SemicolonToken;
                    case FieldDeclarationSyntax node:
                        return node.SemicolonToken;
                    case PropertyDeclarationSyntax node:
                        return node.SemicolonToken;
                    case AccessorDeclarationSyntax node:
                        return node.SemicolonToken;
                    case MethodDeclarationSyntax node:
                        return node.SemicolonToken;
                    case LocalDeclarationStatementSyntax node:
                        return node.SemicolonToken;
                    case ReturnStatementSyntax node:
                        return node.SemicolonToken;
                    case ExpressionStatementSyntax node:
                        return node.SemicolonToken;
                    case BreakStatementSyntax node:
                        return node.SemicolonToken;
                    default: return default;
                }
            }

            //string GetPrefix(SyntaxNode node)
            //{
            //    //GenericNameSyntax
            //    var name = node.GetFullName();
            //    if (!DeclaredSymbols.TryGetValue(name, out SymbolInfo targetInfo))
            //    {
            //        if (!TypeSymbols.TryGetValue(name, out ITypeSymbol genericType2))
            //        {
            //            //break;
            //        }

            //        return null;
            //    }

            //    var name2 = targetInfo.Declared.GetFullName();

            //    return name2;
            //}
        }

        static string GetSign(CSharpSyntaxNode syntaxNode, ToCodeOpt opt)
        {
            if (syntaxNode is null)
            {
                throw new ArgumentNullException(nameof(syntaxNode));
            }

            switch (syntaxNode)
            {
                case MethodDeclarationSyntax node:
                    {
                        var value = $"{ToCode(node.ReturnType, opt)}";

                        if (null != node.ExplicitInterfaceSpecifier)
                        {
                            value = $"{value} {ToCode(node.ExplicitInterfaceSpecifier, opt)}{node.Identifier}";
                        }
                        else
                        {
                            value = $"{value} {node.Identifier}";
                        }

                        if (null != node.TypeParameterList)
                        {
                            value = $"{value}{ToCode(node.TypeParameterList, opt)}";
                        }

                        value = $"{value}{ToCode(node.ParameterList, opt)}";

                        if (node.Modifiers.Any())
                        {
                            value = $"{node.Modifiers} {value}";
                        }

                        if (node.ConstraintClauses.Any())
                        {
                            value = $"{value} {string.Join(" ", node.ConstraintClauses.Select(c => ToCode(c, opt)))}";
                        }

                        return value;
                    }
                case ConstructorDeclarationSyntax node:
                    {
                        var value = $"{node.Identifier}{ToCode(node.ParameterList, opt)}";

                        if (null != node.Initializer)
                        {
                            value = $"{value} {ToCode(node.Initializer, opt)}";
                        }

                        if (node.Modifiers.Any())
                        {
                            value = $"{node.Modifiers} {value}";
                        }

                        return value;
                    }
                case LocalFunctionStatementSyntax node:
                    {
                        var value = $"{ToCode(node.ReturnType, opt)} {node.Identifier}";

                        if (null != node.TypeParameterList)
                        {
                            value = $"{value}{ToCode(node.TypeParameterList, opt)}";
                        }

                        value = $"{value}{ToCode(node.ParameterList, opt)}";

                        if (node.Modifiers.Any())
                        {
                            value = $"{node.Modifiers} {value}";
                        }

                        if (node.ConstraintClauses.Any())
                        {
                            value = $"{value} {string.Join(" ", node.ConstraintClauses.Select(c => ToCode(c, opt)))}";
                        }

                        return value;
                    }
                default: return null;
            }
        }

        static string GetBodyOrExpression(SyntaxNode syntaxNode, ToCodeOpt opt)
        {
            if (syntaxNode is null)
            {
                throw new ArgumentNullException(nameof(syntaxNode));
            }

            switch (syntaxNode)
            {
                case BaseMethodDeclarationSyntax node: return $"{(null != node.ExpressionBody ? ToCode(node.ExpressionBody, opt) : null != node.Body ? ToCode(node.Body, opt) : null)}";
                case LocalFunctionStatementSyntax node: return $"{(null != node.ExpressionBody ? ToCode(node.ExpressionBody, opt) : null != node.Body ? ToCode(node.Body, opt) : null)}";
                default: return null;
            }
        }

        #endregion

        public static Compilation AddSyntaxTree(this GeneratorExecutionContext context, string code, string path = null) => context.Compilation.AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(code, context.ParseOptions).WithFilePath(path ?? Guid.NewGuid().ToString("N")));

        public static Compilation AddSyntaxTree(this Compilation compilation, string code, out SyntaxTree tree, ParseOptions options = null, string path = null)
        {
            tree = SyntaxFactory.ParseSyntaxTree(code, options).WithFilePath(path ?? Guid.NewGuid().ToString("N"));
            return compilation.AddSyntaxTrees(tree);
        }

        public static Compilation AddSyntaxTree(this Compilation compilation, string code, ParseOptions options = null, string path = null) => AddSyntaxTree(compilation, code, out _, options, path);

        public static class Equality<T>
        {
            /// <summary>
            /// CreateComparer
            /// </summary>
            /// <typeparam name="V"></typeparam>
            /// <param name="keySelector"></param>
            /// <returns></returns>
            public static IEqualityComparer<T> CreateComparer<V>(Func<T, V> keySelector)
            {
                return new CommonEqualityComparer<V>(keySelector);
            }
            /// <summary>
            /// CreateComparer
            /// </summary>
            /// <typeparam name="V"></typeparam>
            /// <param name="keySelector"></param>
            /// <param name="comparer"></param>
            /// <returns></returns>
            public static IEqualityComparer<T> CreateComparer<V>(Func<T, V> keySelector, IEqualityComparer<V> comparer)
            {
                return new CommonEqualityComparer<V>(keySelector, comparer);
            }

            class CommonEqualityComparer<V> : IEqualityComparer<T>
            {
                private readonly Func<T, V> keySelector;
                private readonly IEqualityComparer<V> comparer;

                public CommonEqualityComparer(Func<T, V> keySelector, IEqualityComparer<V> comparer)
                {
                    this.keySelector = keySelector;
                    this.comparer = comparer;
                }
                public CommonEqualityComparer(Func<T, V> keySelector)
                    : this(keySelector, EqualityComparer<V>.Default) { }

                public bool Equals(T x, T y)
                {
                    return comparer.Equals(keySelector(x), keySelector(y));
                }
                public int GetHashCode(T obj)
                {
                    return comparer.GetHashCode(keySelector(obj));
                }
            }
        }

        /// <summary>
        /// Scale
        /// </summary>
        /// <param name="value"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static double Scale(this double value, int size = 2)
        {
            var p = Math.Pow(10, size);
            return (int)(value * (int)p) / p;
        }

        public static string GetName(this Enum value) => null == value ? null : Enum.GetName(value.GetType(), value);

        public static bool Equals(this ISymbol symbol, string fullName)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            if (fullName is null)
            {
                throw new ArgumentNullException(nameof(fullName));
            }

            return symbol.GetFullName().Equals(fullName);
        }

        static IDictionary<string, ITypeSymbol> GetTypes(this MetaData.AnalysisInfoModel analysisInfo)
        {
            var dict = new Dictionary<string, ITypeSymbol>();

            foreach (var item in analysisInfo.DeclaredSymbols.Values)
            {
                if (!(item.Declared is ITypeSymbol typeSymbol))
                {
                    continue;
                }

                //if (null == item.References || !item.Declared.DeclaringSyntaxReferences.Any())
                //{
                //    continue;
                //}

                //if (!(item.Declared is ITypeSymbol))
                //{
                //    continue;
                //}

                if (IsTypeParameter(typeSymbol))
                {
                    continue;
                }

                var prefix = typeSymbol.GetFullName(new GetFullNameOpt(prefix: true));

                if ("Business.SourceGenerator.Meta" == prefix || "Business.SourceGenerator.TypeNameFormatter" == prefix)
                {
                    continue;
                }

                var asyncType = GetAsyncType(typeSymbol);

                switch (asyncType)
                {
                    case AsyncType.Task:
                    case AsyncType.ValueTask:
                        continue;
                    case AsyncType.TaskGeneric:
                    case AsyncType.ValueTaskGeneric:
                        if (typeSymbol is INamedTypeSymbol named)
                        {
                            typeSymbol = named.TypeArguments.First();
                        }
                        break;
                    default: break;
                }

                var name = typeSymbol.GetFullNameStandardFormat();

                if (!dict.ContainsKey(name))
                {
                    dict.Add(name, typeSymbol);
                }
            }

            return dict;

            bool IsTypeParameter(ITypeSymbol typeSymbol)
            {
                if (TypeKind.TypeParameter == typeSymbol.TypeKind)
                {
                    return true;
                }

                if (typeSymbol is IArrayTypeSymbol arrayType && TypeKind.TypeParameter == arrayType.ElementType.TypeKind)
                {
                    return true;
                }

                if ((Accessibility.Public != typeSymbol.DeclaredAccessibility && SymbolKind.DynamicType != typeSymbol.Kind) || typeSymbol.IsStatic || typeSymbol.IsRefLikeType)
                {
                    return true;
                }

                if (SpecialType.System_Void == typeSymbol.SpecialType)
                {
                    return true;
                }

                if (TypeKind.Error == typeSymbol.TypeKind)
                {
                    return true;
                }

                if (typeSymbol.ContainingType is INamedTypeSymbol containingType && containingType.IsGenericType && containingType.TypeArguments.Any(c => IsTypeParameter(c)))
                {
                    return true;
                }

                if (typeSymbol is INamedTypeSymbol named)
                {
                    if (named.IsUnboundGenericType)
                    {
                        return true;
                    }

                    foreach (var item in named.TypeArguments)
                    {
                        if (IsTypeParameter(item))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public static IEnumerable<MetaData.SymbolInfo> GetDeclarations(this MetaData.AnalysisInfoModel analysisInfo, string assemblyName)
        {
            var list = new List<MetaData.SymbolInfo>();

            foreach (var item in analysisInfo.TypeSymbols.Values)
            {
                if (!item.AssemblyName.Equals(assemblyName))
                {
                    continue;
                }

                if (null == item.References || !item.Declared.DeclaringSyntaxReferences.Any())
                {
                    continue;
                }

                if (!(item.Syntax.IsKind(SyntaxKind.ClassDeclaration) || item.Syntax.IsKind(SyntaxKind.StructDeclaration)))
                {
                    continue;
                }

                list.Add(item);
            }

            if (0 < list.Count)
            {
                return list;
            }

            return Array.Empty<MetaData.SymbolInfo>();
        }

        //static readonly string AccessorKey = TypeNameFormatter.TypeName.GetFormattedName(typeof(IGeneratorAccessor), TypeNameFormatter.TypeNameFormatOptions.Namespaces);
        const string AccessorKey = "Business.SourceGenerator.Meta.IGeneratorAccessor";

        static bool HasGeneratorAccessor(MetaData.SymbolInfo info)
        {
            const string partialKey = "partial";

            if (!(info.Syntax is TypeDeclarationSyntax member))
            {
                return false;
            }

            if (!member.Modifiers.Any(c => partialKey.Equals(c.ValueText)))
            {
                return false;
            }

            var typeSymbol = info.Declared as ITypeSymbol;

            if (Accessibility.Public != typeSymbol.DeclaredAccessibility || typeSymbol.AllInterfaces.Any(c => AccessorKey.Equals(c.GetFullName())))
            {
                return false;
            }

            return true;
        }

        static bool HasInheritType(INamedTypeSymbol typeSymbol, IEnumerable<MetaData.SymbolInfo> infos)
        {
            var baseType = typeSymbol.BaseType;
            var isInherit = false;

            while (null != baseType)
            {
                if (infos.Any(c => c.Declared.Equals(baseType)))
                {
                    isInherit = true;
                    break;
                }

                baseType = baseType.BaseType;
            }

            return isInherit;
        }

        public static ITypeSymbol GetTypeParameter(ITypeParameterSymbol typeParameter)
        {
            if (typeParameter.HasValueTypeConstraint || typeParameter.HasConstructorConstraint)
            {
                return default;
            }

            var constraintTypes = typeParameter.ConstraintTypes.ToList();

            foreach (var item in typeParameter.ConstraintTypes)
            {
                if (default == item.AllInterfaces)
                {
                    continue;
                }

                var intersect = item.AllInterfaces.Intersect(constraintTypes);

                foreach (var item2 in intersect)
                {
                    constraintTypes.Remove(item2);
                }
            }

            var constraintType = constraintTypes.FirstOrDefault();

            if (null == constraintType)
            {
                return default; //nameof(Object);
            }
            else
            {
                return constraintType;
            }
        }

        static string ToDefaultValue(SpecialType specialType, object value)
        {
            switch (specialType)
            {
                case SpecialType.System_String:
                    return $"\"{value}\"";
                case SpecialType.System_Char:
                    return $"\'{value}\'";
                case SpecialType.System_Boolean:
                    return ((bool)value) ? "true" : "false";
                default: return $"{value ?? "default"}";
            }
        }

        static (string methodKey, string methodKeyClean, List<(string name, ITypeSymbol typeSymbol)> typeParameters) GetMethodKey(ISymbol symbol, Func<string, string> typeClean, params ITypeSymbol[] typeArgument)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            IList<ITypeSymbol> typeArguments = default;

            switch (symbol)
            {
                case INamedTypeSymbol named:
                    typeArguments = named.TypeArguments;
                    break;
                case IMethodSymbol method:
                    typeArguments = method.TypeArguments;
                    break;
                default: break;
            }

            var typeParameters = new List<(string name, ITypeSymbol typeSymbol)>(typeArguments.Count);

            for (int i = 0; i < typeArguments.Count; i++)
            {
                var item = typeArguments[i];

                if (null != typeArgument && i <= typeArgument.Length - 1)
                {
                    item = typeArgument[i];
                }

                var arg = item is ITypeParameterSymbol typeParameter ? GetTypeParameter(typeParameter) : item;

                typeParameters.Add((arg?.GetFullNameStandardFormat(typeClean) ?? "object", arg));
            }

            string key = default;
            string cleanKey = default;

            switch (symbol)
            {
                case INamedTypeSymbol named:
                    key = named.IsGenericType ? GeneratorType.GetGenericTypeName(named.GetFullNameStandardFormat(), typeParameters.Select(c => c.typeSymbol?.GetFullNameStandardFormat() ?? "System.Object")) : symbol.GetFullNameStandardFormat();
                    cleanKey = named.IsGenericType ? GeneratorType.GetGenericTypeName(named.GetFullNameStandardFormat(typeClean), typeParameters.Select(c => c.name)) : symbol.GetFullNameStandardFormat(typeClean);
                    break;
                case IMethodSymbol method:
                    key = method.IsGenericMethod ? $"{method.Name}<{string.Join(", ", typeParameters.Select(c => c.typeSymbol?.GetFullNameStandardFormat() ?? "System.Object"))}>" : method.Name;
                    cleanKey = method.IsGenericMethod ? $"{method.Name}<{string.Join(", ", typeParameters.Select(c => c.name))}>" : method.Name;
                    break;
                default: break;
            }

            if (null != typeClean)
            {
                cleanKey = typeClean(cleanKey);
            }

            return (key, cleanKey, typeParameters);
        }

        static (string methodSign, string constructors) GetMethodSign(ISymbol symbol, string key, List<(string name, ITypeSymbol typeSymbol)> typeParameters, IEnumerable<IParameterSymbol> parameters, int sign, bool hasConstructorKey, string argName, Func<string, string> typeClean, params ITypeSymbol[] typeArgument)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            if (parameters is null)
            {
                return (key, default);
            }

            var parameterList = new List<string>();
            var parameterList2 = new List<string>();
            var length = 0;

            foreach (var parameter in parameters)
            {
                var value = $"{argName}[{parameter.Ordinal}]";
                var type = parameter.Type;
                var typeFullName = type.GetFullNameStandardFormat();
                var typeFullNameClean = type.GetFullNameStandardFormat(typeClean);
                var isValueType = type.IsValueType;

                //if (typeFullName.StartsWith("System.Span") || typeFullName.StartsWith("System.ReadOnlySpan"))
                //{
                //    var named = type as INamedTypeSymbol;
                //    var argNameClean = named.TypeArguments[0].GetFullNameStandardFormat(typeClean);
                //    value = named.TypeArguments[0].IsValueType ? $"({argNameClean}[]){value}" : $"{value} as {argNameClean}[]";
                //}

                if (type is ITypeParameterSymbol typeParameter)
                {
                    if (symbol is IMethodSymbol && TypeParameterKind.Type == typeParameter.TypeParameterKind)
                    {
                        typeFullNameClean = typeParameter.Name;
                        isValueType = true;
                    }
                    else
                    {
                        if (typeParameter.Ordinal < typeParameters.Count)
                        {
                            var (name, typeSymbol) = typeParameters[typeParameter.Ordinal];

                            type = typeSymbol;
                            typeFullNameClean = name;
                            isValueType = type?.IsValueType ?? false;
                        }
                    }
                }

                //var typeKind = (type.DeclaringSyntaxReferences.Any() ? type.TypeKind : TypeKind.Unknown).GetName();
                var typeKind = (SpecialType.None == type?.SpecialType ? type.TypeKind : TypeKind.Unknown).GetName();

                switch (typeFullNameClean)
                {
                    case "System.Object":
                    case "Object":
                    case "object":
                        parameterList.Add(value);
                        parameterList2.Add($"typeof(object), TypeKind.{typeKind}, false, false, null");
                        break;
                    default:
                        if (SpecialType.System_Object == type.SpecialType || TypeKind.Dynamic == type.TypeKind)
                        {
                            parameterList.Add(value);
                            parameterList2.Add($"typeof(object), TypeKind.{typeKind}, false, false, null");
                        }
                        else
                        {
                            parameterList.Add(isValueType ? $"({typeFullNameClean}){value}" : $"{value} as {typeFullNameClean}");
                            parameterList2.Add($"typeof({typeFullNameClean}), TypeKind.{typeKind}, {(isValueType ? "true" : "false")}, {(parameter.HasExplicitDefaultValue ? "true" : "false")}, {(parameter.HasExplicitDefaultValue ? ToDefaultValue(parameter.Type.SpecialType, parameter.ExplicitDefaultValue) : "default")}");
                        }
                        break;
                }

                if (!parameter.HasExplicitDefaultValue && !parameter.IsParams)
                {
                    length += 1;
                }
            }

            var constructors = string.Join(", ", parameterList2.Select(c => $"new global::Business.SourceGenerator.Meta.Parameter({c})"));

            constructors = $"new global::Business.SourceGenerator.Meta.Constructor({(hasConstructorKey ? $"\"{symbol.GetFullNameStandardFormat()}\"" : "default")}, {sign}, {length}, {(0 < parameterList2.Count ? $"new global::Business.SourceGenerator.Meta.Parameter[] {{ {constructors} }}" : "global::System.Array.Empty<global::Business.SourceGenerator.Meta.Parameter>()")})";

            return ($"{key}({string.Join(", ", parameterList)})", constructors);
        }

        static (string methodSign, string constructors) GetMethodSign(ISymbol symbol, IEnumerable<IParameterSymbol> parameters, string argName, Func<string, string> typeClean, params ITypeSymbol[] typeArgument)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            (string key, string cleanKey, List<(string name, ITypeSymbol typeSymbol)> typeParameters) = GetMethodKey(symbol, typeClean, typeArgument);

            if (parameters is null)
            {
                return (cleanKey, default);
            }

            return GetMethodSign(symbol, key, typeParameters, parameters, default, false, argName, typeClean, typeArgument);
        }

        static string GetValueCode(IMethodSymbol method, string receiverType, IDictionary<string, ITypeParameterSymbol> typeParameters, bool asTask, Func<string, string> typeClean)
        {
            var sign = GetMethodSign(method, method.Parameters, "args", typeClean).methodSign;

            if (sign is null)
            {
                return "(obj, args) => default";
            }

            string result = method.IsStatic ? $"{receiverType}.{sign}" : $"(({receiverType})obj).{sign}";

            if (method.ReturnsVoid)
            {
                if (asTask)
                {
                    result = $"{result}; return Task.FromResult<object>(default);";
                }
                else
                {
                    result = $"{result}; return default;";
                }

                result = $"(obj, args) => {{ {result} }}";
            }
            else
            {
                if (asTask)
                {
                    var asyncType = GetAsyncType(method.ReturnType);
                    //async (obj, args) => Task.FromResult(await ((Library1.AAA)obj).BBB()))
                    switch (asyncType)
                    {
                        case AsyncType.None: result = $"(obj, args) => Task.FromResult<object>({result})"; break;
                        case AsyncType.Task: result = $"async (obj, args) => {{ await {result}; return default; }}"; break;
                        case AsyncType.TaskGeneric: result = $"async (obj, args) => await {result}"; break;
                        case AsyncType.ValueTask: result = $"async (obj, args) => {{ await {result}; return default; }}"; break;
                        case AsyncType.ValueTaskGeneric: result = $"async (obj, args) => await {result}"; break;
                        //case AsyncType.Other: break;
                        default: break;
                    }
                }
                else
                {
                    result = $"(obj, args) => {result}";
                }
            }

            return result;
        }

        public static string GeneratorAccessor(MetaData.AnalysisInfoModel analysisInfo, string assemblyName, ToCodeOpt opt)
        {
            var format = opt.StandardFormat ? Environment.NewLine : " ";
            Func<string, string> typeClean = type => TypeNameClean(type, $"{assemblyName}.", "System.Collections.Generic.");

            var declarations = GetDeclarations(analysisInfo, assemblyName);

            var sb = new System.Text.StringBuilder(null);

            var generatorAccessors = declarations.Where(c => HasGeneratorAccessor(c));

            foreach (var item in generatorAccessors)
            {
                var typeSymbol = item.Declared as INamedTypeSymbol;

                if (typeSymbol.IsStatic)
                {
                    continue;
                }

                if (0 < sb.Length)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                }

                if (!typeSymbol.ContainingNamespace.IsGlobalNamespace)
                {
                    sb.AppendFormat("namespace {1}{0}{{{0}", format, typeSymbol.ContainingNamespace.ToDisplayString());
                }

                var type = string.Empty;

                switch (typeSymbol.TypeKind)
                {
                    case TypeKind.Class:
                        type = "class"; break;
                    case TypeKind.Interface:
                        type = "interface"; break;
                    case TypeKind.Struct:
                        type = "struct"; break;
                    default: break;
                }

                sb.AppendFormat("public partial {1} {2} : IGeneratorAccessor{0}{{{0}", format, type, typeSymbol.GetFullName(new GetFullNameOpt(noPrefix: true)));
                sb.AppendFormat("static readonly global::System.Lazy<IAccessorNamedType> generatorAccessorType = new global::System.Lazy<IAccessorNamedType>(() => {1});{0}", format, typeSymbol.ToMeta(typeClean: typeClean));

                sb.AppendLine("public static IAccessorNamedType GeneratorAccessorType { get => generatorAccessorType.Value; }");
                sb.AppendLine("public IAccessorNamedType AccessorType() => GeneratorAccessorType;");
                sb.Append("}");

                if (!typeSymbol.ContainingNamespace.IsGlobalNamespace)
                {
                    sb.AppendFormat("{0}}}", format);
                }
            }

            return sb.ToString();
        }

        #region Temp

        const string makeGenericTypeTemp = @"case global::Business.SourceGenerator.Meta.GeneratorTypeOpt.MakeGenericType:
                {{ 
                    if (arg.makeType is null) {{ throw new global::System.ArgumentNullException(nameof(arg.makeType)); }} 
                    switch (arg.makeType) 
                    {{ 
                        {0} 
                        default: return default; 
                    }} 
                }}
                ";

        const string createGenericTypeTemp = @"case global::Business.SourceGenerator.Meta.GeneratorTypeOpt.CreateGenericType:
                {{
                    switch (arg.createType)
                    {{ 
                        {0} 
                        default: return default; 
                    }} 
                }}
                ";

        const string constructorsTemp = @"case global::Business.SourceGenerator.Meta.GeneratorTypeOpt.Constructors:
                {{ 
                    if (arg.makeType is null) {{ throw new global::System.ArgumentNullException(nameof(arg.makeType)); }} 
                    switch (arg.makeType) 
                    {{ 
                        {0}
                    }} 
                }}
                ";

        const string generatorTypeTemp = @"(arg, opt) => 
        {{ 
            switch (opt)
            {{
                {0}{1}case global::Business.SourceGenerator.Meta.GeneratorTypeOpt.ContainsType: return {2};
                {3}default: return default;
            }}
        }}";

        const string iGeneratorTypeTemp = @"public partial class BusinessSourceGenerator : global::Business.SourceGenerator.Meta.IGeneratorType
{{
    static readonly global::System.Lazy<global::Business.SourceGenerator.Meta.IGeneratorType> generator = new global::System.Lazy<global::Business.SourceGenerator.Meta.IGeneratorType>(() => new global::{0}BusinessSourceGenerator());

    public static global::Business.SourceGenerator.Meta.IGeneratorType Generator {{ get => generator.Value; }}

    static readonly global::System.Lazy<global::System.Collections.Generic.IReadOnlyDictionary<global::System.String, global::System.Func<global::Business.SourceGenerator.Meta.GeneratorTypeArg, global::Business.SourceGenerator.Meta.GeneratorTypeOpt, global::System.Object>>> generatorType = new global::System.Lazy<global::System.Collections.Generic.IReadOnlyDictionary<global::System.String, global::System.Func<global::Business.SourceGenerator.Meta.GeneratorTypeArg, global::Business.SourceGenerator.Meta.GeneratorTypeOpt, global::System.Object>>>(() => new global::System.Collections.ObjectModel.ReadOnlyDictionary<global::System.String, global::System.Func<global::Business.SourceGenerator.Meta.GeneratorTypeArg, global::Business.SourceGenerator.Meta.GeneratorTypeOpt, global::System.Object>>(new global::System.Collections.Generic.Dictionary<global::System.String, global::System.Func<global::Business.SourceGenerator.Meta.GeneratorTypeArg, global::Business.SourceGenerator.Meta.GeneratorTypeOpt, global::System.Object>> {{{1}}}));

    public static global::System.Collections.Generic.IReadOnlyDictionary<global::System.String, global::System.Func<global::Business.SourceGenerator.Meta.GeneratorTypeArg, global::Business.SourceGenerator.Meta.GeneratorTypeOpt, global::System.Object>> GeneratorTypeSingle {{ get => generatorType.Value; }}

    public global::System.Collections.Generic.IReadOnlyDictionary<global::System.String, global::System.Func<global::Business.SourceGenerator.Meta.GeneratorTypeArg, global::Business.SourceGenerator.Meta.GeneratorTypeOpt, global::System.Object>> GeneratorType {{ get => GeneratorTypeSingle; }}
}}";

        #endregion

        public static string GeneratorCode(MetaData.AnalysisInfoModel analysisInfo, string assemblyName, ToCodeOpt opt)
        {
            var format = opt.StandardFormat ? Environment.NewLine : " ";
            string typeClean(string type) => TypeNameClean(type, $"{assemblyName}.", "System.Collections.Generic.");

            var makeGenericTypes = GeneratorType.GetMakeGenericTypes(analysisInfo);
            var makeGenerics = makeGenericTypes.SelectMany(c => c.Value.definitions);
            var makeGenericsKeys = makeGenericTypes.Keys.Concat(makeGenericTypes.SelectMany(c => c.Value.definitions.Select(c2 => c2.GetFullNameStandardFormat()))).ToList();
            var types = GetTypes(analysisInfo);

            var sb = new System.Text.StringBuilder(null);
            var generatorTypes = new List<string>();
            string key = default;

            #region all type

            //types = new Dictionary<string, ITypeSymbol> { { types.First().Key, types.First().Value } }; // test first

            foreach (var item in types)
            {
                if (makeGenericsKeys.Contains(item.Value.GetFullNameOrig()))
                {
                    continue;
                }

                if (SpecialType.System_Void == item.Value.SpecialType || !(item.Value is INamedTypeSymbol named) || item.Value.IsAbstract)
                {
                    continue;
                }

                var constructorsList = new Dictionary<string, string>();
                var makeGenericsList = new List<string>();
                var constructorParametersList = new List<string>();
                var constructorSign = 0;

                var containsType = analysisInfo.TypeSymbols.TryGetValue(item.Value.GetFullName(), out MetaData.SymbolInfo info) && null != info.References;
                if (containsType)
                {
                    key = SetConstructor(named, constructorsList, out (string key, IEnumerable<string> parameters) constructorParameters, typeClean, ref constructorSign, false);

                    //if (!makeGenerics.Any() && default == key)
                    //{
                    //    continue;
                    //}

                    if (default != constructorParameters)
                    {
                        constructorParametersList.Add($"default: return new global::System.Collections.Generic.List<global::Business.SourceGenerator.Meta.Constructor> {{ {string.Join(", ", constructorParameters.parameters)} }};");
                    }
                }

                SetGenerics(makeGenerics, constructorsList, makeGenericsList, constructorParametersList, typeClean, ref constructorSign, item.Value);

                var makeGenericTypeCase = makeGenericsList.Any() ? string.Format(makeGenericTypeTemp, string.Join(" ", makeGenericsList)) : default;
                var createGenericTypeCase = constructorsList.Any() ? string.Format(createGenericTypeTemp, string.Join(" ", constructorsList.Values)) : default;
                var constructorsCase = constructorParametersList.Any() ? string.Format(constructorsTemp, string.Join(" ", constructorParametersList)) : default;

                sb.AppendFormat("{{ \"{0}\", ", item.Key);

                sb.AppendFormat(generatorTypeTemp, makeGenericTypeCase, createGenericTypeCase, containsType ? "true" : "false", constructorsCase);


                sb.Append(" }");

                generatorTypes.Add(sb.ToString());

                sb.Clear();
            }

            #endregion

            if (makeGenericTypes.Any())
            {
                foreach (var item in makeGenericTypes.Values)
                {
                    foreach (var reference in item.references)
                    {
                        if (1 == reference.TypeArguments.Length)
                        {
                            var arg = reference.TypeArguments[0] is ITypeParameterSymbol typeParameter ? GetTypeParameter(typeParameter) : reference.TypeArguments[0];

                            if (types.ContainsKey(null == arg ? "System.Object" : arg.GetFullNameStandardFormat()))
                            {
                                continue;
                            }

                            continue;//??? goto back
                        }

                        var constructorsList = new Dictionary<string, string>();
                        var makeGenericsList = new List<string>();
                        var constructorParametersList = new List<string>();
                        var constructorSign = 0;

                        var typeArgument = reference.TypeArguments.ToArray();
                        var typeArguments = reference.GetFullName(new GetFullNameOpt(standardFormat: true, args: true));

                        SetGenerics(item.definitions, constructorsList, makeGenericsList, constructorParametersList, typeClean, ref constructorSign, typeArgument);

                        var makeGenericTypeCase = makeGenericsList.Any() ? string.Format(makeGenericTypeTemp, string.Join(" ", makeGenericsList)) : default;
                        var createGenericTypeCase = constructorsList.Any() ? string.Format(createGenericTypeTemp, string.Join(" ", constructorsList.Values)) : default;
                        var constructorsCase = constructorParametersList.Any() ? string.Format(constructorsTemp, string.Join(" ", constructorParametersList)) : default;

                        sb.AppendFormat("{{ \"{0}\", ", typeArguments);

                        sb.AppendFormat(generatorTypeTemp, makeGenericTypeCase, createGenericTypeCase, "true", constructorsCase);

                        sb.Append(" }");

                        generatorTypes.Add(sb.ToString());

                        sb.Clear();
                    }
                }
            }

            if (!string.IsNullOrEmpty(assemblyName))
            {
                sb.AppendFormat("namespace {1}{0}", $"{format}{{{format}", assemblyName);
                sb.AppendFormat(iGeneratorTypeTemp, $"{assemblyName}.", generatorTypes.Any() ? $" {string.Join(", ", generatorTypes)} " : " ");
            }
            else
            {
                sb.AppendFormat(iGeneratorTypeTemp, null, generatorTypes.Any() ? $" {string.Join(", ", generatorTypes)} " : " ");
            }

            if (!string.IsNullOrEmpty(assemblyName))
            {
                sb.Append($"{format}}}");
            }

            return sb.ToString();

            static void SetGenerics(IEnumerable<INamedTypeSymbol> makeGenerics, Dictionary<string, string> constructorsList, List<string> makeGenericsList, List<string> constructorParametersList, Func<string, string> typeClean, ref int constructorSign, params ITypeSymbol[] typeArgument)
            {
                foreach (var makeGeneric in makeGenerics)
                {
                    if (makeGeneric.IsAbstract)
                    {
                        continue;
                    }

                    var key = SetConstructor(makeGeneric, constructorsList, out (string key, IEnumerable<string> parameters) constructorParameters, typeClean, ref constructorSign, true, typeArgument);

                    if (default == key)
                    {
                        continue;
                    }

                    makeGenericsList.Add($"case \"{makeGeneric.GetFullNameStandardFormat()}\": return typeof({key});");

                    if (default != constructorParameters)
                    {
                        if (!constructorParametersList.Any())
                        {
                            constructorParametersList.Add("default: return default;");
                        }

                        constructorParametersList.Add($"case \"{constructorParameters.key}\": return new List<Constructor> {{ {string.Join(", ", constructorParameters.parameters)} }};");
                    }
                }
            }
        }
        static string SetConstructor(INamedTypeSymbol named, IDictionary<string, string> constructors, out (string key, IEnumerable<string> parameters) constructorParameters, Func<string, string> typeClean, ref int constructorSign, bool hasConstructorKey, params ITypeSymbol[] typeArgument)
        {
            constructorParameters = default;
            (string methodKey, string methodKeyClean, List<(string name, ITypeSymbol typeSymbol)> typeParameters) = GetMethodKey(named, typeClean, typeArgument);

            var constructorSign2 = constructorSign;
            var parameters = new List<string>();

            foreach (var constructor in named.InstanceConstructors)
            {
                if (Accessibility.Public != constructor.DeclaredAccessibility)
                {
                    continue;
                }

                if (constructor.Parameters.Any(c =>
                {
                    var typeFullName = c.Type.GetFullNameStandardFormat();
                    return SymbolKind.PointerType == c.Type.Kind || typeFullName.StartsWith("System.Span") || typeFullName.StartsWith("System.ReadOnlySpan");
                }))
                {
                    continue;
                }

                var constructorKey = GetMethodSign(named, methodKeyClean, typeParameters, constructor.Parameters, constructorSign, hasConstructorKey, "arg.args", typeClean, typeArgument);

                if (constructors.ContainsKey(constructorKey.methodSign))
                {
                    constructors.Remove(constructorKey.methodSign);
                    continue;
                }

                constructors.Add(constructorKey.methodSign, $"case {constructorSign}: return new global::{constructorKey.methodSign};");
                parameters.Add(constructorKey.constructors);

                constructorSign++;
            }

            if (parameters.Any())
            {
                constructorParameters = (hasConstructorKey ? named.GetFullNameStandardFormat() : default, parameters);
            }

            if (constructorSign2 == constructorSign)
            {
                return default;
            }

            return methodKeyClean;
        }

        #region Analysis.Meta

        static AsyncType GetAsyncType(ITypeSymbol symbol)
        {
            var awaiters = symbol.GetMembers("GetAwaiter");

            if (awaiters.Any())
            {
                var awaiter = awaiters.FirstOrDefault(c => c is IMethodSymbol method && Accessibility.Public == method.DeclaredAccessibility && !method.Parameters.Any()) as IMethodSymbol;

                if (null != awaiter && awaiter.ReturnType is INamedTypeSymbol returnType)
                {
                    if (returnType.Interfaces.Any(c => "System.Runtime.CompilerServices.INotifyCompletion" == c.GetFullName()) && returnType.Interfaces.Any(c => "System.Runtime.CompilerServices.ICriticalNotifyCompletion" == c.GetFullName()))
                    {
                        if (returnType.GetMembers("GetResult").Any(c => Accessibility.Public == c.DeclaredAccessibility && c is IMethodSymbol method && (method.ReturnsVoid || returnType.TypeArguments.Any(c => c.Equals(method.ReturnType))) && !method.Parameters.Any()) && returnType.GetMembers("IsCompleted").Any(c => Accessibility.Public == c.DeclaredAccessibility && c is IPropertySymbol property && SpecialType.System_Boolean == property.Type.SpecialType && Accessibility.Public == property.GetMethod.DeclaredAccessibility))
                        {
                            var fullName = symbol.GetFullName();

                            switch (fullName)
                            {
                                case "System.Threading.Tasks.Task":
                                    return AsyncType.Task;
                                case "System.Threading.Tasks.ValueTask":
                                    return AsyncType.ValueTask;
                                default:
                                    if (fullName.StartsWith("System.Threading.Tasks.Task<"))
                                    {
                                        return AsyncType.TaskGeneric;
                                    }
                                    else if (fullName.StartsWith("System.Threading.Tasks.ValueTask<"))
                                    {
                                        return AsyncType.ValueTaskGeneric;
                                    }
                                    return AsyncType.Other;
                            }
                        }
                    }
                }
            }

            return AsyncType.None;
        }

        const int accessorDepth = 4;

        #region ToCode

        public static string ToMeta(this ISymbol symbol, int depth = default, string receiverType = default, bool isClone = default, StringCollection types = default, Func<string, string> typeClean = default)
        {
            switch (symbol)
            {
                case INamedTypeSymbol accessor:
                    {
                        depth++;
                        var isDeclaringSyntaxReferences = symbol.DeclaringSyntaxReferences.Any();
                        var skip = accessorDepth < depth || SpecialType.None != accessor.SpecialType;
                        var skip2 = skip || !isDeclaringSyntaxReferences;
                        var fullName = symbol.GetFullNameStandardFormat();

                        if (null == types)
                        {
                            types = new StringCollection { fullName };
                        }

                        #region members

                        var members = skip2 ? default : new Dictionary<string, string>();

                        if (!skip2)
                        {
                            foreach (var item in accessor.GetMembers())
                            {
                                if (Accessibility.Public != item.DeclaredAccessibility)
                                {
                                    continue;
                                }

                                switch (item)
                                {
                                    case IMethodSymbol member:
                                        if (MethodKind.Ordinary == member.MethodKind)
                                        {
                                            if (!members.ContainsKey(member.Name))
                                            {
                                                members.Add(member.Name, member.ToMeta(depth, fullName, true, typeClean: typeClean));
                                            }
                                            else
                                            {
                                                members.Add(member.GetFullNameStandardFormat(), member.ToMeta(depth, fullName, false, typeClean: typeClean));
                                            }
                                        }
                                        break;

                                    case IFieldSymbol member: if (member.IsImplicitlyDeclared) { continue; } members.Add(member.Name, member.ToMeta(depth, fullName, false, types, typeClean)); break;
                                    case IPropertySymbol member: if (member.IsImplicitlyDeclared) { continue; } members.Add(member.Name, member.ToMeta(depth, fullName, false, types, typeClean)); break;
                                    case IEventSymbol member: members.Add(member.Name, member.ToMeta(depth, fullName, false, types)); break;
                                    default: break;
                                }
                            }
                        }

                        #endregion

                        var sb = new System.Text.StringBuilder("new AccessorNamedType(");
                        //==================Meta==================//
                        sb.AppendFormat("Accessibility.{0}, ", accessor.DeclaredAccessibility.GetName());
                        sb.AppendFormat("{0}, ", accessor.CanBeReferencedByName ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsImplicitlyDeclared ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsExtern ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsSealed ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsAbstract ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsOverride ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsVirtual ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsStatic ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsDefinition ? "true" : "false");
                        sb.AppendFormat("\"{0}\", ", accessor.Name);
                        sb.AppendFormat("\"{0}\", ", fullName);
                        sb.AppendFormat("Kind.{0}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", isDeclaringSyntaxReferences ? "true" : "false");
                        //==================IAccessorType==================//
                        sb.AppendFormat("{0}, ", accessor.IsNamespace ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsType ? "true" : "false");
                        sb.AppendFormat("{0}, ", !(members?.Any() ?? false) ? "default" : $"new Dictionary<string, IAccessorMeta> {{ {string.Join(", ", members.Select(c => $"{{ \"{c.Key}\", {c.Value} }}"))} }}");
                        sb.AppendFormat("{0}, ", accessor.IsReferenceType ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsReadOnly ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsUnmanagedType ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsRefLikeType ? "true" : "false");
                        sb.AppendFormat("SpecialType.{0}, ", accessor.SpecialType.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsNativeIntegerType ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsTupleType ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsAnonymousType ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsValueType ? "true" : "false");
                        sb.AppendFormat("NullableAnnotation.{0}, ", accessor.NullableAnnotation.GetName());
                        sb.AppendFormat("{0}, ", (skip2 || !accessor.AllInterfaces.Any()) ? "default" : $"new string[] {{ {string.Join(", ", accessor.AllInterfaces.Select(c => $"\"{c.GetFullNameStandardFormat()}\""))} }}");
                        sb.AppendFormat("{0}, ", accessor.BaseType is null ? "default" : $"\"{accessor.BaseType.GetFullNameStandardFormat()}\"");
                        sb.AppendFormat("TypeKind.{0}, ", accessor.TypeKind.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsRecord ? "true" : "false");
                        sb.AppendFormat("AsyncType.{0}, ", GetAsyncType(accessor).GetName());
                        //==================IAccessorNamedType==================//
                        sb.AppendFormat("{0}, ", (skip2 || !accessor.TypeArgumentNullableAnnotations.Any()) ? "default" : $"new NullableAnnotation[] {{ {string.Join(", ", accessor.TypeArgumentNullableAnnotations.Select(c => $"NullableAnnotation.{c.GetName()}"))} }}");
                        sb.AppendFormat("{0}, ", (skip2 || null == accessor.TupleElements || !accessor.TupleElements.Any()) ? "default" : $"new IAccessorField[] {{ {string.Join(", ", accessor.TupleElements.Select(c => c.ToMeta(depth, accessor.Name)))} }}");
                        sb.AppendFormat("{0}, ", accessor.MightContainExtensionMethods ? "true" : "false");
                        sb.AppendFormat("{0}, ", (skip2 || !accessor.Constructors.Any()) ? "default" : $"new IAccessorMethod[] {{ {string.Join(", ", accessor.Constructors.Select(c => c.ToMeta(depth, typeClean: typeClean)))} }}");
                        sb.AppendFormat("{0}, ", (skip2 || accessor.EnumUnderlyingType is null) ? "default" : accessor.EnumUnderlyingType.ToMeta(depth));
                        sb.AppendFormat("{0}, ", (skip2 || accessor.DelegateInvokeMethod is null) ? "default" : accessor.DelegateInvokeMethod.ToMeta(depth));
                        sb.AppendFormat("{0}, ", accessor.IsSerializable ? "true" : "false");

                        sb.AppendFormat("{0}, ", (skip || !accessor.TypeParameters.Any()) ? "default" : $"new Dictionary<string, IAccessorTypeParameter> {{ {string.Join(", ", accessor.TypeParameters.Select(c => $"{{ \"{c.GetFullNameStandardFormat()}\", {c.ToMeta(depth)} }}"))} }}");
                        //sb.Append((MemberNames?.Any() ?? false) ? $"new string[] {{ {string.Join(", ", MemberNames.Select(c => $"\"{c}\""))} }}, " : "default, ");
                        sb.AppendFormat("{0}, ", accessor.IsComImport ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsImplicitClass ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsScriptClass ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsUnboundGenericType ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsGenericType ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.Arity);
                        sb.AppendFormat("{0}", (skip || !accessor.TypeArguments.Any()) ? "default" : $"new IAccessorType[] {{ {string.Join(", ", accessor.TypeArguments.Select(c => c.ToMeta(depth)))} }}");
                        return sb.Append(")").ToString();
                    }
                case ITypeParameterSymbol accessor:
                    {
                        depth++;
                        var isDeclaringSyntaxReferences = symbol.DeclaringSyntaxReferences.Any();
                        var skip = accessorDepth < depth || SpecialType.None != accessor.SpecialType;
                        var skip2 = skip || !isDeclaringSyntaxReferences;
                        var fullName = symbol.GetFullNameStandardFormat();

                        var sb = new System.Text.StringBuilder("new AccessorTypeParameter(");
                        //==================Meta==================//
                        sb.AppendFormat("Accessibility.{0}, ", accessor.DeclaredAccessibility.GetName());
                        sb.AppendFormat("{0}, ", accessor.CanBeReferencedByName ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsImplicitlyDeclared ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsExtern ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsSealed ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsAbstract ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsOverride ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsVirtual ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsStatic ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsDefinition ? "true" : "false");
                        sb.AppendFormat("\"{0}\", ", accessor.Name);
                        sb.AppendFormat("\"{0}\", ", fullName);
                        sb.AppendFormat("Kind.{0}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", isDeclaringSyntaxReferences ? "true" : "false");
                        //==================IAccessorType==================//
                        sb.AppendFormat("{0}, ", accessor.IsNamespace ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsType ? "true" : "false");
                        sb.Append("default, ");
                        sb.AppendFormat("{0}, ", accessor.IsReferenceType ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsReadOnly ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsUnmanagedType ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsRefLikeType ? "true" : "false");
                        sb.AppendFormat("SpecialType.{0}, ", accessor.SpecialType.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsNativeIntegerType ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsTupleType ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsAnonymousType ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsValueType ? "true" : "false");
                        sb.AppendFormat("NullableAnnotation.{0}, ", accessor.NullableAnnotation.GetName());
                        sb.AppendFormat("{0}, ", (skip2 || !accessor.AllInterfaces.Any()) ? "default" : $"new string[] {{ {string.Join(", ", accessor.AllInterfaces.Select(c => $"\"{c.GetFullNameStandardFormat()}\""))} }}");
                        sb.AppendFormat("{0}, ", accessor.BaseType is null ? "default" : $"\"{accessor.BaseType.GetFullNameStandardFormat()}\"");
                        sb.AppendFormat("TypeKind.{0}, ", accessor.TypeKind.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsRecord ? "true" : "false");
                        sb.AppendFormat("AsyncType.{0}, ", GetAsyncType(accessor).GetName());
                        //==================IAccessorTypeParameter==================//
                        sb.AppendFormat("{0}, ", accessor.Ordinal);
                        sb.AppendFormat("VarianceKind.{0}, ", accessor.Variance.GetName());
                        sb.AppendFormat("TypeParameterKind.{0}, ", accessor.TypeParameterKind.GetName());
                        sb.AppendFormat("{0}, ", accessor.HasReferenceTypeConstraint ? "true" : "false");
                        sb.AppendFormat("NullableAnnotation.{0}, ", accessor.ReferenceTypeConstraintNullableAnnotation.GetName());
                        sb.AppendFormat("{0}, ", accessor.HasValueTypeConstraint ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.HasUnmanagedTypeConstraint ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.HasNotNullConstraint ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.HasConstructorConstraint ? "true" : "false");
                        sb.Append((skip2 || !accessor.ConstraintTypes.Any()) ? "default" : $"new IAccessorType[] {{ {string.Join(", ", accessor.ConstraintTypes.Select(c => c.ToMeta(depth)))} }}");
                        return sb.Append(")").ToString();
                    }
                case ITypeSymbol accessor:
                    {
                        switch (symbol)
                        {
                            case INamedTypeSymbol typeSymbol: return ToMeta(typeSymbol, depth);
                            case ITypeParameterSymbol typeSymbol: return ToMeta(typeSymbol, depth);
                            default: break;
                        }

                        depth++;
                        var isDeclaringSyntaxReferences = symbol.DeclaringSyntaxReferences.Any();
                        var skip = accessorDepth < depth || SpecialType.None != accessor.SpecialType;
                        var skip2 = skip || !isDeclaringSyntaxReferences;
                        var fullName = symbol.GetFullNameStandardFormat();

                        if (null == types)
                        {
                            types = new StringCollection { fullName };
                        }

                        #region members

                        var members = skip2 ? default : new Dictionary<string, string>();

                        if (!skip2)
                        {
                            foreach (var item in accessor.GetMembers())
                            {
                                if (Accessibility.Public != item.DeclaredAccessibility)
                                {
                                    continue;
                                }

                                switch (item)
                                {
                                    case IMethodSymbol member:
                                        if (MethodKind.Ordinary == member.MethodKind)
                                        {
                                            if (!members.ContainsKey(member.Name))
                                            {
                                                members.Add(member.Name, member.ToMeta(depth, fullName, true, typeClean: typeClean));
                                            }
                                            else
                                            {
                                                members.Add(member.GetFullNameStandardFormat(), member.ToMeta(depth, fullName, false, typeClean: typeClean));
                                            }
                                        }
                                        break;

                                    case IFieldSymbol member: if (member.IsImplicitlyDeclared) { continue; } members.Add(member.Name, member.ToMeta(depth, fullName, false, types, typeClean)); break;
                                    case IPropertySymbol member: if (member.IsImplicitlyDeclared) { continue; } members.Add(member.Name, member.ToMeta(depth, fullName, false, types, typeClean)); break;
                                    case IEventSymbol member: members.Add(member.Name, member.ToMeta(depth, fullName, false, types)); break;
                                    default: break;
                                }
                            }
                        }

                        #endregion

                        var sb = new System.Text.StringBuilder("new AccessorType(");
                        //==================Meta==================//
                        sb.AppendFormat("Accessibility.{0}, ", accessor.DeclaredAccessibility.GetName());
                        sb.AppendFormat("{0}, ", accessor.CanBeReferencedByName ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsImplicitlyDeclared ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsExtern ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsSealed ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsAbstract ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsOverride ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsVirtual ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsStatic ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsDefinition ? "true" : "false");
                        sb.AppendFormat("\"{0}\", ", accessor.Name);
                        sb.AppendFormat("\"{0}\", ", fullName);
                        sb.AppendFormat("Kind.{0}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", isDeclaringSyntaxReferences ? "true" : "false");
                        //==================IAccessorType==================//
                        sb.AppendFormat("{0}, ", accessor.IsNamespace ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsType ? "true" : "false");
                        sb.AppendFormat("{0}, ", !(members?.Any() ?? false) ? "default" : $"new Dictionary<string, IAccessorMeta> {{ {string.Join(", ", members.Select(c => $"{{ \"{c.Key}\", {c.Value} }}"))} }}");
                        sb.AppendFormat("{0}, ", accessor.IsReferenceType ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsReadOnly ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsUnmanagedType ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsRefLikeType ? "true" : "false");
                        sb.AppendFormat("SpecialType.{0}, ", accessor.SpecialType.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsNativeIntegerType ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsTupleType ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsAnonymousType ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsValueType ? "true" : "false");
                        sb.AppendFormat("NullableAnnotation.{0}, ", accessor.NullableAnnotation.GetName());
                        sb.AppendFormat("{0}, ", (skip2 || !accessor.AllInterfaces.Any()) ? "default" : $"new string[] {{ {string.Join(", ", accessor.AllInterfaces.Select(c => $"\"{c.GetFullNameStandardFormat()}\""))} }}");
                        sb.AppendFormat("{0}, ", accessor.BaseType is null ? "default" : $"\"{accessor.BaseType.GetFullNameStandardFormat()}\"");
                        sb.AppendFormat("TypeKind.{0}, ", accessor.TypeKind.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsRecord ? "true" : "false");
                        sb.AppendFormat("AsyncType.{0}", GetAsyncType(accessor).GetName());
                        return sb.Append(")").ToString();
                    }
                case IParameterSymbol accessor:
                    {
                        depth++;
                        var isDeclaringSyntaxReferences = symbol.DeclaringSyntaxReferences.Any();
                        var skip = accessorDepth < depth;
                        var fullName = symbol.GetFullNameStandardFormat();

                        var sb = new System.Text.StringBuilder("new AccessorParameter(");
                        //==================Meta==================//
                        sb.AppendFormat("Accessibility.{0}, ", accessor.DeclaredAccessibility.GetName());
                        sb.AppendFormat("{0}, ", accessor.CanBeReferencedByName ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsImplicitlyDeclared ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsExtern ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsSealed ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsAbstract ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsOverride ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsVirtual ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsStatic ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsDefinition ? "true" : "false");
                        sb.AppendFormat("\"{0}\", ", accessor.Name);
                        sb.AppendFormat("\"{0}\", ", fullName);
                        sb.AppendFormat("Kind.{0}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", isDeclaringSyntaxReferences ? "true" : "false");
                        //==================IAccessorParameter==================//
                        sb.AppendFormat("RefKind.{0}, ", accessor.RefKind.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsParams ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsOptional ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsThis ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsDiscard ? "true" : "false");
                        sb.AppendFormat("{0}, ", skip ? "default" : accessor.Type.ToMeta(depth));
                        sb.AppendFormat("NullableAnnotation.{0}, ", accessor.NullableAnnotation.GetName());
                        sb.AppendFormat("{0}, ", accessor.Ordinal);
                        sb.AppendFormat("{0}, ", accessor.HasExplicitDefaultValue ? "true" : "false");
                        sb.Append(accessor.HasExplicitDefaultValue ? ToDefaultValue(accessor.Type.SpecialType, accessor.ExplicitDefaultValue) : "default");
                        return sb.Append(")").ToString();
                    }
                case IMethodSymbol accessor:
                    {
                        depth++;
                        var isDeclaringSyntaxReferences = symbol.DeclaringSyntaxReferences.Any();
                        var skip = accessorDepth < depth;
                        var fullName = symbol.GetFullNameStandardFormat();

                        var typeParameters = skip ? default : accessor.TypeParameters.ToDictionary(c => c.GetFullNameStandardFormat(), c => c);

                        var skip2 = string.IsNullOrEmpty(receiverType) || 2 < depth;
                        var getValue = !skip2 ? GetValueCode(accessor, receiverType, typeParameters, false, typeClean) : default;
                        var getValueAsync = !skip2 ? GetValueCode(accessor, receiverType, typeParameters, true, typeClean) : default;

                        var sb = new System.Text.StringBuilder("new AccessorMethod(");
                        //==================Meta==================//
                        sb.AppendFormat("Accessibility.{0}, ", accessor.DeclaredAccessibility.GetName());
                        sb.AppendFormat("{0}, ", accessor.CanBeReferencedByName ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsImplicitlyDeclared ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsExtern ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsSealed ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsAbstract ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsOverride ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsVirtual ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsStatic ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsDefinition ? "true" : "false");
                        sb.AppendFormat("\"{0}\", ", accessor.Name);
                        sb.AppendFormat("\"{0}\", ", fullName);
                        sb.AppendFormat("Kind.{0}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", isDeclaringSyntaxReferences ? "true" : "false");
                        //==================IAccessorMethod==================//
                        sb.AppendFormat("{0}, ", accessor.IsReadOnly ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsInitOnly ? "true" : "false");
                        sb.AppendFormat("{0}, ", (skip || !accessor.Parameters.Any()) ? "default" : $"new IAccessorParameter[] {{ {string.Join(", ", accessor.Parameters.Select(c => c.ToMeta(depth)))} }}");
                        sb.AppendFormat("{0}, ", accessor.IsPartialDefinition ? "true" : "false");
                        sb.AppendFormat("{0}, ", (skip || !accessor.TypeParameters.Any()) ? "default" : $"new Dictionary<string, IAccessorTypeParameter> {{ {string.Join(", ", typeParameters.Select(c => $"{{ \"{c.Key}\", {c.Value.ToMeta(depth)} }}"))} }}");
                        sb.AppendFormat("{0}, ", accessor.IsConditional ? "true" : "false");
                        sb.AppendFormat("MethodKind.{0}, ", accessor.MethodKind.GetName());
                        sb.AppendFormat("{0}, ", accessor.Arity);
                        sb.AppendFormat("{0}, ", accessor.IsGenericMethod ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsExtensionMethod ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsVararg ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsCheckedBuiltin ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsAsync ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.ReturnsVoid ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.ReturnsByRef ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.ReturnsByRefReadonly ? "true" : "false");
                        sb.AppendFormat("RefKind.{0}, ", accessor.RefKind.GetName());
                        sb.AppendFormat("{0}, ", skip ? "default" : accessor.ReturnType?.ToMeta(depth));
                        sb.AppendFormat("NullableAnnotation.{0}, ", accessor.ReturnNullableAnnotation.GetName());
                        sb.AppendFormat("{0}, ", accessor.HidesBaseMethodsByName ? "true" : "false");
                        sb.AppendFormat("{0}, ", isClone ? "true" : "false");
                        sb.AppendFormat("{0}, ", skip2 ? "default" : getValue);
                        sb.AppendFormat("{0}", skip2 ? "default" : getValueAsync);

                        return sb.Append(")").ToString();
                    }
                case IFieldSymbol accessor:
                    {
                        depth++;
                        var isDeclaringSyntaxReferences = symbol.DeclaringSyntaxReferences.Any();
                        var skip = accessorDepth < depth;
                        var fullName = symbol.GetFullNameStandardFormat();

                        var isRepeat = types?.Contains(fullName) ?? false;
                        if (!isRepeat && null != types)
                        {
                            types.Add(fullName);
                        }

                        var type = accessor.Type;
                        var typeFullName = type.GetFullName();

                        string getValue = default;
                        string setValue = default;
                        if (!string.IsNullOrEmpty(receiverType) && 2 >= depth)
                        {
                            var name = $"{receiverType}.{symbol.Name}";
                            var castName = $"(({receiverType})obj).{symbol.Name}";

                            getValue = symbol.IsStatic ? $"obj => {name}" : $"obj => {castName}";
                            var value = TypeKind.TypeParameter == type.TypeKind ? $"({type.Name})value" : type.IsValueType ? $"({typeClean(typeFullName)})value" : $"value as {typeClean(typeFullName)}";

                            if (symbol.IsStatic)
                            {
                                setValue = $"(obj, value) => {name} = {value}";
                            }
                            else
                            {
                                setValue = $"(obj, value) => {{ var obj2 = ({receiverType})obj; obj2.{symbol.Name} = {value}; }}";
                            }
                        }

                        var sb = new System.Text.StringBuilder("new AccessorField(");
                        //==================Meta==================//
                        sb.AppendFormat("Accessibility.{0}, ", accessor.DeclaredAccessibility.GetName());
                        sb.AppendFormat("{0}, ", accessor.CanBeReferencedByName ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsImplicitlyDeclared ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsExtern ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsSealed ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsAbstract ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsOverride ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsVirtual ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsStatic ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsDefinition ? "true" : "false");
                        sb.AppendFormat("\"{0}\", ", accessor.Name);
                        sb.AppendFormat("\"{0}\", ", fullName);
                        sb.AppendFormat("Kind.{0}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", isDeclaringSyntaxReferences ? "true" : "false");
                        //==================IAccessorMember==================//
                        sb.AppendFormat("{0}, ", accessor.IsReadOnly ? "true" : "false");
                        sb.AppendFormat("{0}, ", skip ? "default" : type.ToMeta(depth, types: types));
                        sb.AppendFormat("\"{0}\", ", typeFullName);
                        sb.AppendFormat("{0}, ", isRepeat ? "true" : "false");
                        //==================IAccessorField==================//
                        sb.AppendFormat("{0}, ", accessor.IsConst ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsVolatile ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsFixedSizeBuffer ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.FixedSize);
                        sb.AppendFormat("NullableAnnotation.{0}, ", accessor.NullableAnnotation.GetName());
                        sb.AppendFormat("{0}, ", accessor.HasConstantValue ? "true" : "false");
                        //sb.AppendObject(accessor.ConstantValue, ", ");
                        sb.AppendFormat("{0}, ", accessor.HasConstantValue ? ToDefaultValue(accessor.Type.SpecialType, accessor.ConstantValue) : "default");
                        sb.AppendFormat("{0}, ", accessor.IsExplicitlyNamedTupleElement ? "true" : "false");

                        if (!string.IsNullOrEmpty(getValue))
                        {
                            sb.AppendFormat("{0}, ", getValue);
                        }
                        else
                        {
                            sb.Append("default, ");
                        }

                        if (!string.IsNullOrEmpty(setValue))
                        {
                            sb.AppendFormat("{0}", setValue);
                        }
                        else
                        {
                            sb.Append("default");
                        }

                        return sb.Append(")").ToString();
                    }
                case IPropertySymbol accessor:
                    {
                        depth++;
                        var isDeclaringSyntaxReferences = symbol.DeclaringSyntaxReferences.Any();
                        var skip = accessorDepth < depth;
                        var fullName = symbol.GetFullNameStandardFormat();

                        var isRepeat = types?.Contains(fullName) ?? false;
                        if (!isRepeat && null != types)
                        {
                            types.Add(fullName);
                        }

                        var type = accessor.Type;
                        var typeFullName = type.GetFullName();

                        string getValue = default;
                        string setValue = default;
                        if (!string.IsNullOrEmpty(receiverType) && 2 >= depth && !accessor.IsReadOnly)
                        {
                            var name = $"{receiverType}.{symbol.Name}";
                            var castName = $"(({receiverType})obj).{symbol.Name}";
                            getValue = symbol.IsStatic ? $"obj => {name}" : $"obj => {castName}";
                            var value = TypeKind.TypeParameter == type.TypeKind ? $"({type.Name})value" : type.IsValueType ? $"({typeClean(typeFullName)})value" : $"value as {typeClean(typeFullName)}";

                            if (symbol.IsStatic)
                            {
                                setValue = $"(obj, value) => {name} = {value}";
                            }
                            else
                            {
                                setValue = $"(obj, value) => {{ var obj2 = ({receiverType})obj; obj2.{symbol.Name} = {value}; }}";
                            }
                        }

                        var sb = new System.Text.StringBuilder("new AccessorProperty(");
                        //==================Meta==================//
                        sb.AppendFormat("Accessibility.{0}, ", accessor.DeclaredAccessibility.GetName());
                        sb.AppendFormat("{0}, ", accessor.CanBeReferencedByName ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsImplicitlyDeclared ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsExtern ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsSealed ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsAbstract ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsOverride ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsVirtual ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsStatic ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsDefinition ? "true" : "false");
                        sb.AppendFormat("\"{0}\", ", accessor.Name);
                        sb.AppendFormat("\"{0}\", ", fullName);
                        sb.AppendFormat("Kind.{0}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", isDeclaringSyntaxReferences ? "true" : "false");
                        //==================IAccessorMember==================//
                        sb.AppendFormat("{0}, ", accessor.IsReadOnly ? "true" : "false");
                        sb.AppendFormat("{0}, ", skip ? "default" : type.ToMeta(depth, types: types));
                        sb.AppendFormat("\"{0}\", ", typeFullName);
                        sb.AppendFormat("{0}, ", isRepeat ? "true" : "false");
                        //==================IAccessorProperty==================//
                        sb.AppendFormat("{0}, ", accessor.IsIndexer ? "true" : "false");
                        sb.AppendFormat("NullableAnnotation.{0}, ", accessor.NullableAnnotation.GetName());
                        sb.AppendFormat("RefKind.{0}, ", accessor.RefKind.GetName());
                        sb.AppendFormat("{0}, ", accessor.ReturnsByRefReadonly ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.ReturnsByRef ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsWithEvents ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsWriteOnly ? "true" : "false");

                        if (!string.IsNullOrEmpty(getValue))
                        {
                            sb.AppendFormat("{0}, ", getValue);
                        }
                        else
                        {
                            sb.Append("default, ");
                        }

                        if (!string.IsNullOrEmpty(setValue))
                        {
                            sb.AppendFormat("{0}", setValue);
                        }
                        else
                        {
                            sb.Append("default");
                        }

                        return sb.Append(")").ToString();
                    }
                case IEventSymbol accessor:
                    {
                        depth++;
                        var isDeclaringSyntaxReferences = symbol.DeclaringSyntaxReferences.Any();
                        var skip = accessorDepth < depth;
                        var fullName = symbol.GetFullNameStandardFormat();

                        var isRepeat = types?.Contains(fullName) ?? false;
                        if (!isRepeat && null != types)
                        {
                            types.Add(fullName);
                        }

                        var type = accessor.Type;
                        var typeFullName = type.GetFullName();

                        string getValue = default;
                        string setValue = default;
                        if (!string.IsNullOrEmpty(receiverType) && 2 >= depth)
                        {
                            var name = $"{receiverType}.{symbol.Name}";
                            var castName = $"(({receiverType})obj).{symbol.Name}";
                            getValue = symbol.IsStatic ? $"obj => {name}" : $"obj => {castName}";
                        }

                        var sb = new System.Text.StringBuilder("new AccessorEvent(");
                        //==================Meta==================//
                        sb.AppendFormat("Accessibility.{0}, ", accessor.DeclaredAccessibility.GetName());
                        sb.AppendFormat("{0}, ", accessor.CanBeReferencedByName ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsImplicitlyDeclared ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsExtern ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsSealed ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsAbstract ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsOverride ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsVirtual ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsStatic ? "true" : "false");
                        sb.AppendFormat("{0}, ", accessor.IsDefinition ? "true" : "false");
                        sb.AppendFormat("\"{0}\", ", accessor.Name);
                        sb.AppendFormat("\"{0}\", ", fullName);
                        sb.AppendFormat("Kind.{0}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", isDeclaringSyntaxReferences ? "true" : "false");
                        //==================IAccessorMember==================//
                        sb.Append("false, ");
                        sb.AppendFormat("{0}, ", skip ? "default" : type.ToMeta(depth, types: types));
                        sb.AppendFormat("\"{0}\", ", typeFullName);
                        sb.AppendFormat("{0}, ", isRepeat ? "true" : "false");
                        //==================IAccessorEvent==================//
                        sb.AppendFormat("NullableAnnotation.{0}, ", accessor.NullableAnnotation.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsWindowsRuntimeEvent ? "true" : "false");
                        sb.AppendFormat("{0}, ", skip ? "default" : accessor.AddMethod?.ToMeta() ?? "default");
                        sb.AppendFormat("{0}, ", skip ? "default" : accessor.RemoveMethod?.ToMeta() ?? "default");

                        if (!string.IsNullOrEmpty(getValue))
                        {
                            sb.AppendFormat("{0}, ", getValue);
                        }
                        else
                        {
                            sb.AppendFormat("default, ");
                        }

                        if (!string.IsNullOrEmpty(setValue))
                        {
                            sb.AppendFormat("{0}", setValue);
                        }
                        else
                        {
                            sb.Append("default");
                        }

                        return sb.Append(")").ToString();
                    }
                default: return default;
            }
        }

        #endregion

        #endregion
    }
}
