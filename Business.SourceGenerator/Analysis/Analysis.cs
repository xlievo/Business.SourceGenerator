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
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;

    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class GeneratorGenericType : Attribute
    {
        static readonly string Key = TypeNameFormatter.TypeName.GetFormattedName(typeof(GeneratorGenericType), TypeNameFormatter.TypeNameFormatOptions.Namespaces);

        static readonly IEqualityComparer<ITypeSymbol> typeSymbolEquality = Expression.Equality<ITypeSymbol>.CreateComparer(c => c.GetFullName());

        public enum TypeKeyFormat
        {
            No,
            ToLower,
            ToUpper
        }

        public static IReadOnlyDictionary<string, string> GetTypes(MetaData.AnalysisInfoModel analysisInfo)
        {
            var dict = new Dictionary<string, string>();

            foreach (var info in analysisInfo.Attributes)
            {
                if (Key == info.Key)
                {
                    foreach (var item in info.Value)
                    {
                        var declarationInfo = item.Key.GetSymbolInfo();
                        var declared = declarationInfo.Declared as ITypeSymbol;
                        var declarations = GetTypeReference(analysisInfo.TypeSymbols, declared, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration);
                        var references = GetTypeReference(analysisInfo.TypeSymbols, declared).Where(c => c is INamedTypeSymbol named && !named.TypeArguments.Any(c2 => c2.TypeKind == TypeKind.TypeParameter)).OrderBy(c => c.GetFullName());

                        foreach (var item2 in declarations)
                        {
                            var name = item2.GetFullName(new Expression.GetFullNameOpt(true));

                            foreach (var item3 in references)
                            {
                                if (item3 is INamedTypeSymbol named)
                                {
                                    var args = named.TypeArguments.Select(c => Expression.GetFullName(c));
                                    var argsKey = named.TypeArguments.Select(c => Expression.GetFullName(c, new Expression.GetFullNameOpt(standardFormat: true)));
                                    var key = GetGenericTypeName(name, argsKey);

                                    if (!dict.ContainsKey(key))
                                    {
                                        dict.Add(key, GetGenericTypeName(name, args));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (0 < dict.Count)
            {
                return dict;
            }

            return new ReadOnlyDictionary<string, string>(dict);
        }

        static IEnumerable<ITypeSymbol> GetTypeReference(ConcurrentDictionary<string, MetaData.SymbolInfo> typeSymbols, ITypeSymbol typeSymbol, params SyntaxKind[] kinds)
        {
            if (typeSymbol is null)
            {
                throw new ArgumentNullException(nameof(typeSymbol));
            }

            if (TypeKind.Interface != typeSymbol.TypeKind && TypeKind.Class != typeSymbol.TypeKind)
            {
                return Array.Empty<ITypeSymbol>();
            }

            var typeSymbolName = typeSymbol.OriginalDefinition.GetFullName();

            var list = new List<ITypeSymbol>();

            foreach (var item in typeSymbols.Values)
            {
                if (0 < kinds?.Length && !kinds.Any(c => item.Syntax.IsKind(c)))
                {
                    continue;
                }

                var typeSymbol2 = item.Declared as ITypeSymbol;

                if (TypeKind.Interface != typeSymbol2.TypeKind && TypeKind.Class != typeSymbol2.TypeKind && TypeKind.Struct != typeSymbol2.TypeKind)
                {
                    continue;
                }

                if (typeSymbol2 is INamedTypeSymbol namedType && namedType.IsUnboundGenericType)
                {
                    continue;
                }

                switch (typeSymbol.TypeKind)
                {
                    case TypeKind.Class:
                        var baseType = typeSymbol2.BaseType;

                        while (null != baseType)
                        {
                            //if (baseType.OriginalDefinition.Equals(typeSymbol.OriginalDefinition, SymbolEqualityComparer.Default))
                            if (Expression.Equals(baseType.OriginalDefinition, typeSymbolName))
                            {
                                list.Add(typeSymbol2);
                                break;
                            }
                            baseType = baseType.BaseType;
                        }
                        break;
                    case TypeKind.Interface:
                        //if (typeSymbol2.OriginalDefinition.Equals(typeSymbol.OriginalDefinition) || typeSymbol2.AllInterfaces.Any(c => typeSymbol.OriginalDefinition.Equals(c.OriginalDefinition, SymbolEqualityComparer.Default)))
                        if (Expression.Equals(typeSymbol2.OriginalDefinition, typeSymbolName) || typeSymbol2.AllInterfaces.Any(c => Expression.Equals(c.OriginalDefinition, typeSymbolName)))
                        {
                            list.Add(typeSymbol2);
                        }
                        break;
                    default: break;
                }
            }

            if (0 < list.Count)
            {
                return list.Distinct(typeSymbolEquality);
            }

            return Array.Empty<ITypeSymbol>();
        }

        static string GetGenericTypeName(string name, IEnumerable<string> typeSyntaxs)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }

            if (typeSyntaxs is null)
            {
                throw new ArgumentNullException(nameof(typeSyntaxs));
            }

            var args = SyntaxFactory.TypeArgumentList(new SeparatedSyntaxList<TypeSyntax>().AddRange(typeSyntaxs.Select(c => SyntaxFactory.ParseTypeName(c))));

            var sp = new ReadOnlySpan<string>(name.Split('.'));

            if (1 < sp.Length)
            {
                var prefix = string.Join(".", sp.Slice(0, sp.Length - 1).ToArray());
                var suffix = sp.Slice(sp.Length - 1)[0];

                return $"{prefix}.{SyntaxFactory.GenericName(SyntaxFactory.ParseToken(suffix), args).ToCode()}";
            }

            return SyntaxFactory.GenericName(SyntaxFactory.ParseToken(name), args).ToCode();
        }
    }

    public interface IGeneratorCode
    {
        //IEnumerable<Type> GeneratorGenericTypes { get; }

        //Type GetGenericType(string key);

        //string GetGenericType(Type type, params Type[] typeArguments);

        Type MakeGenericType(Type type, params Type[] typeArguments);

        object CreateGenericType(Type type, params Type[] typeArguments);

        //IEnumerable<string> Types { get; }

        public bool ContainsType(Type type);
    }

    //[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    //public sealed class SetGenericType : Attribute
    //{
    //    public const string Key2 = "Business.SourceGenerator.SetGenericType";

    //    public SetGenericType(Type genericType, int methodGenericPosition = 0, int genericPosition = 0) { }
    //}

    public class MetaData
    {
        public static AnalysisInfoModel AnalysisInfo = new AnalysisInfoModel(new ConcurrentDictionary<string, StringCollection>(), new ConcurrentDictionary<string, SymbolInfo>(), new ConcurrentDictionary<string, SymbolInfo>(), new ConcurrentDictionary<string, ConcurrentDictionary<string, AssignmentExpressionSyntax>>(), new ConcurrentDictionary<string, ConcurrentDictionary<string, SymbolInfo>>(), new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, AttributeSyntax>>>());

        public readonly struct AnalysisInfoModel
        {
            internal AnalysisInfoModel(ConcurrentDictionary<string, StringCollection> syntaxTrees, ConcurrentDictionary<string, SymbolInfo> declaredSymbols, ConcurrentDictionary<string, SymbolInfo> typeSymbols, ConcurrentDictionary<string, ConcurrentDictionary<string, AssignmentExpressionSyntax>> staticAssignments, ConcurrentDictionary<string, ConcurrentDictionary<string, SymbolInfo>> invocations, ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, AttributeSyntax>>> attributes)//, ConcurrentDictionary<string, ConcurrentDictionary<string, SetGenericTypeArgs>> genericTypeSign
            {
                SyntaxTrees = syntaxTrees;
                DeclaredSymbols = declaredSymbols;
                TypeSymbols = typeSymbols;
                StaticAssignments = staticAssignments;
                Invocations = invocations;
                Attributes = attributes;
                //GenericTypeSign = genericTypeSign;
            }

            public ConcurrentDictionary<string, StringCollection> SyntaxTrees { get; }
            public ConcurrentDictionary<string, SymbolInfo> DeclaredSymbols { get; }
            public ConcurrentDictionary<string, SymbolInfo> TypeSymbols { get; }

            //================================================================================//

            public ConcurrentDictionary<string, ConcurrentDictionary<string, AssignmentExpressionSyntax>> StaticAssignments { get; }
            public ConcurrentDictionary<string, ConcurrentDictionary<string, SymbolInfo>> Invocations { get; }

            public ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, AttributeSyntax>>> Attributes { get; }

            //public ConcurrentDictionary<string, ConcurrentDictionary<string, SetGenericTypeArgs>> GenericTypeSign { get; }
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

        /*
        public readonly struct SetGenericTypeArgs
        {
            public SetGenericTypeArgs(TypeParameterSyntax genericTypeParameter, int methodGenericPosition)
            {
                //this.GenericType = genericType;
                this.GenericTypeParameter = genericTypeParameter;
                this.MethodGenericPosition = methodGenericPosition;
            }

            //public ITypeSymbol GenericType { get; }

            public TypeParameterSyntax GenericTypeParameter { get; }

            public int MethodGenericPosition { get; }
        }
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
                //if (filePaths.Contains(item.Value.Syntax.SyntaxTree.FilePath))
                //{
                //    AnalysisInfo.DeclaredSymbols.TryRemove(item.Key, out _);
                //}
            }

            foreach (var item in AnalysisInfo.TypeSymbols)
            {
                if (!references.Contains(item.Value.AssemblyName))
                {
                    AnalysisInfo.TypeSymbols.TryRemove(item.Key, out _);
                }
                //if (filePaths.Contains(item.Value.Syntax.SyntaxTree.FilePath))
                //{
                //    AnalysisInfo.TypeSymbols.TryRemove(item.Key, out _);
                //}
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
            ////model.GetSpeculativeTypeInfo(0, null, SpeculativeBindingOption.BindAsTypeOrNamespace);

            //var tt = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("int");
            //var tt2 = new SeparatedSyntaxList<TypeSyntax>().Add(tt);
            //var aa = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TypeArgumentList(tt2);
            ////aa.Arguments.Add(tt);

            //var ddd = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseToken("sss"), aa);

            //var ddd2 = model.GetSpeculativeTypeInfo(0, ddd, SpeculativeBindingOption.BindAsTypeOrNamespace);
            ////var ddd2 = model.GetSymbolInfo(ddd);
            ///

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

                            var info = new SymbolInfo(syntax, symbol, declared, source, symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() ?? declared.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(), attrs, model.Compilation.AssemblyName);

                            //switch (syntax)
                            //{
                            //    case AssignmentExpressionSyntax node:
                            //        var assignment = model.GetSymbolInfo(node.Left).Symbol;

                            //        var assignment2 = node.Left.GetSymbolInfo();

                            //        var assignment3 = assignment2.Symbol;

                            //        if (null != assignment && assignment.IsStatic)// && (assignment is IFieldSymbol || assignment is IPropertySymbol)
                            //        {
                            //            //AnalysisInfo.StaticAssignments.GetOrAdd(assignment.GetFullName(), c => new ConcurrentDictionary<string, AssignmentExpressionSyntax>()).AddOrUpdate(node.GetFullName(), node, (x, y) => node);
                            //            AnalysisInfo.StaticAssignments.GetOrAdd(assignment.GetFullName(), c => new ConcurrentDictionary<string, AssignmentExpressionSyntax>()).AddOrUpdate(node.GetFullName(), node, (x, y) => node);
                            //        }
                            //        break;
                            //    case InvocationExpressionSyntax node:
                            //        if (null != info.Symbol)
                            //        {
                            //            //if (syntax.ToString().Contains("Help.MD5"))
                            //            //{

                            //            //}
                            //            //if (syntax.ToString().Contains("Help.Scale((decimal)value"))
                            //            //{

                            //            //}
                            //            AnalysisInfo.Invocations.GetOrAdd(info.Symbol.GetFullNameOrig(), c => new ConcurrentDictionary<string, SymbolInfo>()).AddOrUpdate(info.Syntax.GetFullName(), info, (x, y) => info);
                            //        }
                            //        //else
                            //        //{
                            //        //    if (syntax.ToString().Contains("Help.MD5"))
                            //        //    {

                            //        //        symbol = model.GetSymbolInfo(node.Expression).Symbol;
                            //        //        var s2 = model.GetTypeInfo(node.Expression);
                            //        //        var s3 = model.GetDeclaredSymbol(node.Expression);
                            //        //    }
                            //        //}
                            //        break;
                            //    case MethodDeclarationSyntax node:

                            //        SetGenericType(node, declared, model);

                            //        break;
                            //    default: break;
                            //}

                            if (declared is ITypeSymbol type)
                            {
                                var key = declared.GetFullName();

                                AnalysisInfo.TypeSymbols.TryAdd(key, info);

                                //if (type.DeclaringSyntaxReferences.Any())
                                //{
                                //    //TypeSymbols.AddOrUpdate(key, type, (x, y) => type);
                                //    AnalysisInfo.TypeSymbols.TryAdd(key, info);
                                //}
                            }

                            //DeclaredSymbols.AddOrUpdate(syntax.GetFullName(), info, (x, y) => info);
                            AnalysisInfo.DeclaredSymbols.TryAdd(syntax.GetFullName(), info);
                        }
                    }
                    break;
                default: break;
            }
        }
    }

    public static class Expression
    {
        #region AnalysisSymbol

        public static string GetMSBuildProperty(this GeneratorExecutionContext context, string name, string defaultValue = "")
        {
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue($"build_property.{name}", out var value);
            return value ?? defaultValue;
        }
        /*
        public static IEnumerable<string> GetMSBuildPropertys(this GeneratorExecutionContext context, string prefix)
        {
            var key = $"build_property.{prefix}";

            var options = context.AnalyzerConfigOptions.GlobalOptions.GetType().GetField("Options", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(context.AnalyzerConfigOptions.GlobalOptions) as System.Collections.Immutable.ImmutableDictionary<string, string>;

            foreach (var item in options)
            {
                if (item.Key.StartsWith(key))
                {
                    yield return item.Value;
                }
            }
        }
        */

        //public static void GetAttributes(string key, MemberDeclarationSyntax node, SemanticModel model)
        //{
        //    var atts = new List<AttributeSyntax>();

        //    foreach (var item in node.AttributeLists)
        //    {
        //        foreach (var item2 in item.Attributes)
        //        {
        //            if (model.GetSymbolInfo(item2).Symbol is IMethodSymbol method)
        //            {
        //                if (key == method.ReceiverType.GetFullName())
        //                {
        //                    AnalysisInfo.Attributes.GetOrAdd(node.GetFullName(), c => new ConcurrentDictionary<string, AttributeSyntax>()).AddOrUpdate(item2.GetFullName(), item2, (x, y) => item2);
        //                }
        //            }
        //        }
        //    }
        //}

        //public static IEnumerable<string> GetGenericAttr(string attrKey)
        //{
        //    if (string.IsNullOrEmpty(attrKey))
        //    {
        //        throw new ArgumentException(nameof(attrKey));
        //    }

        //    var list = new List<string>();

        //    foreach (var info in AnalysisInfo.Attributes)
        //    {
        //        if (attrKey == info.Key)
        //        {
        //            foreach (var item in info.Value)
        //            {
        //                var declarationInfo = item.Key.GetSymbolInfo();
        //                var declared = declarationInfo.Declared as ITypeSymbol;
        //                var declarations = GetTypeReference(declared, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration);
        //                var references = GetTypeReference(declared).Where(c => c is INamedTypeSymbol named && !named.TypeArguments.Any(c2 => c2.TypeKind == TypeKind.TypeParameter));

        //                foreach (var item2 in declarations)
        //                {
        //                    var name = item2.GetFullName(true);

        //                    foreach (var item3 in references)
        //                    {
        //                        var args = item3.GetGenericArgs();
        //                        var name2 = GetGenericTypeName(name, args);
        //                        if (!list.Contains(name2))
        //                        {
        //                            list.Add(name2);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    if (0 < list.Count)
        //    {
        //        return list;
        //    }

        //    return Array.Empty<string>();
        //}

        /*
        public static IReadOnlyDictionary<string, string> GetGenericTypeAttr(string attrKey)
        {
            if (string.IsNullOrEmpty(attrKey))
            {
                throw new ArgumentException(nameof(attrKey));
            }

            var dict = new Dictionary<string, string>();

            foreach (var info in AnalysisInfo.Attributes)
            {
                if (attrKey == info.Key)
                {
                    foreach (var item in info.Value)
                    {
                        var declarationInfo = item.Key.GetSymbolInfo();
                        var declared = declarationInfo.Declared as ITypeSymbol;
                        var declarations = GetTypeReference(declared, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration);
                        var references = GetTypeReference(declared).Where(c => c is INamedTypeSymbol named && !named.TypeArguments.Any(c2 => c2.TypeKind == TypeKind.TypeParameter)).OrderBy(c => c.GetFullName());

                        foreach (var item2 in declarations)
                        {
                            var name = item2.GetFullName(new GetFullNameOpt(true));

                            foreach (var item3 in references)
                            {
                                if (item3 is INamedTypeSymbol named)
                                {
                                    var args = named.TypeArguments.Select(c => GetFullName(c));
                                    var argsKey = named.TypeArguments.Select(c => GetFullName(c, new GetFullNameOpt(false, false, true, true)));
                                    var key = GetGenericTypeName(name, argsKey);
                                    if (!dict.ContainsKey(key))
                                    {
                                        dict.Add(key, GetGenericTypeName(name, args));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (0 < dict.Count)
            {
                return dict;
            }

            return new ReadOnlyDictionary<string, string>(dict);
        }

        public static void SetGenericType2(string key, ConcurrentDictionary<string, ConcurrentDictionary<string, AttributeSyntax>> value)
        {
            if (SetGenericTypeSign == key)
            {
                foreach (var item in value)
                {
                    var declared = item.Key.GetSymbolInfo().Declared;

                    foreach (var sign in item.Value.Values)
                    {
                        var signInfo = sign.GetSymbolInfo();

                        var ss = signInfo.Declared.GetFullNameOrig();

                        var parameters = GetMethodParameters(signInfo.Symbol, sign);

                        ITypeSymbol genericType = null;
                        int? genericPosition = 0;
                        int? methodGenericPosition = 0;

                        foreach (var item2 in parameters)
                        {
                            if (null != item2.Value)
                            {
                                switch (item2.Key.Name)
                                {
                                    case "genericType":
                                        if (item2.Value is TypeOfExpressionSyntax typeOf)
                                        {
                                            var type = typeOf.Type.GetSymbolInfo().Symbol as ITypeSymbol;

                                            genericType = type;
                                        }
                                        break;
                                    case "methodGenericPosition":
                                        {
                                            if (item2.Value is LiteralExpressionSyntax literal)
                                            {
                                                methodGenericPosition = (int?)literal.Token.Value;
                                            }
                                        }
                                        break;
                                    case "genericPosition":
                                        {
                                            if (item2.Value is LiteralExpressionSyntax literal)
                                            {
                                                genericPosition = (int?)literal.Token.Value;
                                            }
                                        }
                                        break;
                                    default: break;
                                }
                            }
                        }

                        if (TypeKind.Interface == genericType.TypeKind)
                        {
                            var interfaceType = GetTypeReference(genericType, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration);

                            var ddd = interfaceType.ElementAt(1);

                            var ddd3 = ddd.GetFullName(new GetFullNameOpt(true));

                            var interfaceType2 = GetTypeReference(genericType);

                            var interfaceType3 = GetTypeReference(ddd);

                            var dd = interfaceType2.Where(c => c is INamedTypeSymbol).Cast<INamedTypeSymbol>().Where(c => !c.TypeArguments.Any(c2 => c2.TypeKind == TypeKind.TypeParameter));

                            var list = new List<string>();
                            var list2 = new List<string>();

                            foreach (var item2 in dd)
                            {
                                var nn = item2.GetFullName();
                                list.Add(nn);

                                var args = item2.GetGenericArgs();
                                var nn2 = GetGenericTypeName(ddd3, args);
                                list2.Add(nn2);
                            }

                            if (interfaceType.Any())
                            {
                                foreach (var item2 in interfaceType)
                                {
                                    Set(declared, item2, methodGenericPosition, genericPosition);
                                }
                            }
                        }
                        else
                        {
                            if (!Set(declared, genericType, methodGenericPosition, genericPosition))
                            {
                                break;
                            }
                        }
                    }
                }
            }

            bool Set(ISymbol declared, ITypeSymbol genericType, int? methodGenericPosition, int? genericPosition)
            {
                if (!AnalysisInfo.TypeSymbols.TryGetValue(genericType.GetFullName(), out SymbolInfo genericType2))
                {
                    return false;
                }

                var genericSyntax = genericType2.Declared.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as TypeDeclarationSyntax;

                if (null == genericSyntax.TypeParameterList)
                {
                    return false;
                }

                var k = $"{genericType2.Declared.GetFullName()}->{methodGenericPosition}->{genericPosition}";

                var genericTypeValue = genericPosition.Value < genericSyntax.TypeParameterList.Parameters.Count ? genericSyntax.TypeParameterList.Parameters[genericPosition.Value] : null;

                var v = new SetGenericTypeArgs(genericTypeValue, methodGenericPosition.Value);

                AnalysisInfo.GenericTypeSign.GetOrAdd(declared.GetFullNameOrig(), new ConcurrentDictionary<string, SetGenericTypeArgs>()).AddOrUpdate(k, v, (x, y) => v);

                return true;
            }
        }

        public static string GetGenericTypeName(string name, IEnumerable<string> typeSyntaxs)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }

            if (typeSyntaxs is null)
            {
                throw new ArgumentNullException(nameof(typeSyntaxs));
            }

            var args = SyntaxFactory.TypeArgumentList(new SeparatedSyntaxList<TypeSyntax>().AddRange(typeSyntaxs.Select(c => SyntaxFactory.ParseTypeName(c))));

            var sp = new ReadOnlySpan<string>(name.Split('.'));

            if (1 < sp.Length)
            {
                var prefix = string.Join(".", sp.Slice(0, sp.Length - 1).ToArray());
                var suffix = sp.Slice(sp.Length - 1)[0];

                return $"{prefix}.{SyntaxFactory.GenericName(SyntaxFactory.ParseToken(suffix), args).ToCode()}";
            }

            return SyntaxFactory.GenericName(SyntaxFactory.ParseToken(name), args).ToCode();
        }
        */

        public static IEnumerable<MetaData.SymbolInfo> GetGenericType(this MetaData.SymbolInfo symbols, List<MetaData.SymbolInfo> path = null, IEnumerable<ITypeSymbol> genericArguments = null)
        {
            //var symbols = model.GetSymbols(syntax);

            if (null != genericArguments)
            {
                symbols = symbols.Clone(genericArguments);
            }

            if (null == path)
            {
                path = new List<MetaData.SymbolInfo>();
            }

            path.Add(symbols);

            var result = new List<MetaData.SymbolInfo>();

            switch (symbols.Syntax)
            {
                case TypeParameterSyntax node: result.Add(symbols); break;
                case TypeDeclarationSyntax node:
                    result.Add(symbols);
                    break;
                case PredefinedTypeSyntax node: result.Add(symbols); break;
                case CastExpressionSyntax node: result.AddRange(GetGenericType(GetSymbolInfo(node.Type), path)); break;
                case ConditionalExpressionSyntax node:
                    result.Add(symbols);
                    //result.AddRange(GetGenericType(node.WhenTrue.GetSymbolInfo()));
                    //result.AddRange(GetGenericType(node.WhenFalse.GetSymbolInfo()));
                    break;
                case DefaultExpressionSyntax node:
                    return GetGenericType(GetSymbolInfo(symbols.Declared), path);
                case VariableDeclaratorSyntax node:
                    {
                        //T = new()
                        if (null != node.Initializer?.Value)
                        {
                            if (node.Initializer.Value is ImplicitObjectCreationExpressionSyntax && node.Parent is VariableDeclarationSyntax node2)
                            {
                                result.Add(GetSymbolInfo(node2.Type));
                            }
                            else
                            {
                                result.AddRange(GetGenericType(GetSymbolInfo(node.Initializer.Value), path));
                            }
                        }
                        //else
                        //{
                        //    //result.Add(node.Type);
                        //}
                    }
                    break;
                case BinaryExpressionSyntax node:
                    return GetGenericType(GetSymbolInfo(node.Left), path);
                case ParenthesizedExpressionSyntax node:
                    return GetGenericType(GetSymbolInfo(node.Expression), path);
                case IdentifierNameSyntax node:
                    {
                        if (null != symbols.References)
                        {
                            if (SymbolKind.TypeParameter == symbols.Symbol.Kind)
                            {
                                result.AddRange(GetMethodGenericParameters(symbols.Source.GetFullName(), other: symbols.Symbol));
                            }
                            else
                            {
                                result.AddRange(GetGenericType(GetSymbolInfo(symbols.References), path));//IdentifierNameSyntax
                            }
                        }
                    }
                    break;
                case ObjectCreationExpressionSyntax node:
                    if (symbols.Declared.IsIDict(out KeyValuePair<ITypeSymbol, ITypeSymbol> kv))
                    {
                        return GetGenericType(GetSymbolInfo(kv.Value), path);
                    }
                    else if (symbols.Declared.IsIColl(out ITypeSymbol k))
                    {
                        return GetGenericType(GetSymbolInfo(k), path);
                    }
                    //return GetGenericType2(node.Type.GetSymbolInfo(), path);

                    IEnumerable<ITypeSymbol> genericTypeArguments = null;

                    if (symbols.Declared is INamedTypeSymbol namedType && namedType.IsGenericType)
                    {
                        genericTypeArguments = namedType.TypeArguments.Select(c => GetTypeParameter(symbols.Source, path, c as ITypeParameterSymbol));
                        //var type = GetTypeParameter(symbols.Source, path, namedType.TypeArguments[0] as ITypeParameterSymbol);

                        //if (null != type)
                        //{
                        //    //symbols = symbols.Clone(type);

                        //    var name = type.GetFullNameOrig();//Typetarget6<Typetarget2>

                        //    var old = namedType.TypeArguments.First();

                        //    var namedType2 = namedType.TypeArguments.Replace(old, type);

                        //    //var namedType4 = namedType.ConstructUnboundGenericType();
                        //    var namedType4 = namedType.SpecialType;

                        //    //namedType4.Construct(type);
                        //    //Microsoft.CodeAnalysis
                        //    //SpecialType
                        //    //namedType..
                        //    //_bound = new SyntheticBoundNodeFactory(null, compilationState.Type, node, compilationState, diagnostics);
                        //    // Microsoft.CodeAnalysis.CSharp.CSharpExtensions.
                        //    //new Microsoft.CodeAnalysis.CSharp.CSharpSyntaxRewriter().VisitGenericName();
                        //}
                    }

                    return GetGenericType(GetSymbolInfo(symbols.Declared), path, genericTypeArguments);
                case TypeOfExpressionSyntax node:
                    return GetGenericType(GetSymbolInfo(node.Type), path);
                //case ReturnStatementSyntax node:
                //    {

                //    }
                //    break;
                //case MethodDeclarationSyntax node:
                //    {

                //    }
                //    break;
                //case LiteralExpressionSyntax node:
                //    {
                //        if (node.IsKind(SyntaxKind.DefaultLiteralExpression))
                //        {
                //            return null;
                //        }
                //    }
                //    break;
                //PropertyDeclarationSyntax PropertyDeclaration
                case MemberAccessExpressionSyntax node:
                    {
                        if (null != symbols.Declared)
                        {
                            //PropertyDeclarationSyntax PropertyDeclaration public T C { get; set; } = GetT4(new Typetarget5() as T) as T;
                            //EqualsValueClauseSyntax EqualsValueClause = GetT4(new Typetarget5() as T) as T
                            result.AddRange(GetGenericType(GetSymbolInfo(symbols.Declared), path));
                        }

                        switch (symbols.References)
                        {
                            case PropertyDeclarationSyntax node2:
                                if (null != node2.Initializer)
                                {
                                    result.AddRange(GetGenericType(GetSymbolInfo(node2.Initializer.Value), path));
                                }
                                break;
                            case VariableDeclaratorSyntax node2:
                                result.AddRange(GetGenericType(GetSymbolInfo(node2), path));
                                break;
                            default: break;
                        }
                    }
                    break;
                case PropertyDeclarationSyntax node:
                    {
                        if (null != symbols.Declared)
                        {
                            result.AddRange(GetGenericType(GetSymbolInfo(symbols.Declared), path));
                        }

                        if (null != node.Initializer)
                        {
                            result.AddRange(GetGenericType(GetSymbolInfo(node.Initializer.Value), path));
                        }
                    }
                    break;
                case InvocationExpressionSyntax node:
                    {
                        var method = (symbols.Declared is IMethodSymbol ? symbols.Declared : symbols.Symbol) as IMethodSymbol;

                        if ("object.GetType()" == method.GetFullNameOrig())
                        {
                            if (node.Expression is MemberAccessExpressionSyntax memberAccess)
                            {
                                result.AddRange(GetGenericType(GetSymbolInfo(memberAccess.Expression), path));
                            }

                            break;
                        }

                        var methodSyntax = symbols.References as MethodDeclarationSyntax;
                        var returns = new List<ExpressionSyntax>();

                        if (null != methodSyntax?.Body)
                        {
                            foreach (var item in methodSyntax.Body.DescendantNodes())
                            {
                                if (item is ReturnStatementSyntax ret)
                                {
                                    returns.Add(ret.Expression);
                                }
                            }
                        }
                        else if (null != methodSyntax?.ExpressionBody)
                        {
                            returns.Add(methodSyntax.ExpressionBody.Expression);
                        }

                        if (0 < returns.Count)
                        {
                            foreach (var item in returns)
                            {
                                switch (item.RawKind)
                                {
                                    case (int)SyntaxKind.DefaultLiteralExpression:
                                        {
                                            var parameter = GetGenericReturnParameter(method, symbols);

                                            if (!parameter.Equals(default(KeyValuePair<IParameterSymbol, ExpressionSyntax>)))
                                            {
                                                var results = GetGenericType(GetSymbolInfo(parameter.Value), path);
                                                result.AddRange(results);
                                            }
                                            else if (SymbolKind.TypeParameter == method.ReturnType.Kind)
                                            //else if (method.TypeArguments.Any(c => c.Equals(method.ReturnType)))
                                            {
                                                var type = GetTypeParameterToArgument(symbols.Symbol, method.ReturnType);

                                                if (null != type)
                                                {
                                                    result.AddRange(GetGenericType(GetSymbolInfo(type), path));
                                                }
                                            }
                                            else
                                            {
                                                result.AddRange(GetGenericType(GetSymbolInfo(method.ReturnType), path));
                                            }
                                        }
                                        break;
                                    default:
                                        {
                                            var results = GetGenericType(GetSymbolInfo(item), path);

                                            if (results.Any())
                                            {
                                                var typeArgs = method.TypeArguments.Select(c => GetSymbolInfo(c));

                                                foreach (var item2 in results)
                                                {
                                                    if ((SymbolKind.TypeParameter == item2.Declared.Kind && method.TypeParameters.Any(c => c.Equals(item2.Declared))) || typeArgs.Any(c => c.Equals(item2.Declared)))
                                                    {
                                                        var parameters = GetMethodParameters(symbols);

                                                        if (0 < parameters.Count)
                                                        {
                                                            foreach (var item3 in parameters)
                                                            {
                                                                result.AddRange(GetGenericType(GetSymbolInfo(item3.Value), path));
                                                            }
                                                        }
                                                        else
                                                        {
                                                            var type = GetTypeParameterToArgument(symbols.Symbol, item2.Declared);

                                                            if (null != type)
                                                            {
                                                                result.AddRange(GetGenericType(GetSymbolInfo(type), path));
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        result.Add(item2);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //item = CastExpressionSyntax CastExpression (decimal)((int)(value * (int)p) / p)
                                                result.AddRange(GetGenericType(GetSymbolInfo(item), path));
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        else
                        {
                            var parameters = GetMethodParameters(symbols);

                            if (0 < parameters.Count)
                            {
                                foreach (var item in parameters)
                                {
                                    result.AddRange(GetGenericType(GetSymbolInfo(item.Value), path));
                                }
                            }
                            else if (SymbolKind.TypeParameter == method.ReturnType.Kind)
                            {
                                var type = GetTypeParameter(symbols.Source, path, method.ReturnType as ITypeParameterSymbol);

                                if (null != type)
                                {
                                    //result.AddRange(type);
                                    result.AddRange(GetGenericType(GetSymbolInfo(type), path));
                                }
                                //var last = symbols.Method.GetLastMethod(path);

                                //var type = last.Symbol.GetTypeParameterToArgument(method.ReturnType);

                                //if (null != type)
                                //{
                                //    result.AddRange(GetGenericType2(model, type.GetReferences(), path));
                                //}
                            }
                            else
                            {
                                return GetGenericType(GetSymbolInfo(method.ReturnType), path);
                            }
                        }
                    }
                    break;
                case ParameterSyntax node:
                    {
                        var methods = path.Where(c => c.Symbol is IMethodSymbol).TakeWhile(c => !symbols.Source.Equals(c.Source));
                        //var last = methods.LastOrDefault();

                        var last = GetLastMethod(symbols.Source, path);

                        //var nodes = (methods.Any() && null == last.Source && last.Syntax is InvocationExpressionSyntax) ? new List<SyntaxNode> { last.Syntax } : (methods.Any() ? last.Source?.GetReferences().DescendantNodes().Where(c => c is InvocationExpressionSyntax) : symbols.Syntax.SyntaxTree.GetRoot().DescendantNodes().Where(c => c is InvocationExpressionSyntax));

                        //var nodes = (methods.Any() && null == last.Source && last.Syntax is InvocationExpressionSyntax) ? new List<SyntaxNode> { last.Syntax } : (methods.Any() ? last.Source?.GetReferences().DescendantNodes().Where(c => c is InvocationExpressionSyntax) : ProxyGenerator.Invocations.TryGetValue(symbols.Source.GetFullName(), out ConcurrentDictionary<string, SymbolInfo> invocations) ? invocations.Values.Select(c => c.Syntax) : null);

                        IEnumerable<SyntaxNode> nodes = null;
                        //IEnumerable<SyntaxNode> nodes2 = null;

                        if (methods.Any() && null == last.Source && last.Syntax is InvocationExpressionSyntax)
                        {
                            nodes = new List<SyntaxNode> { last.Syntax };
                        }
                        else if (methods.Any())
                        {
                            //nodes = last.Source?.GetReferences().DescendantNodes().Where(c => c is InvocationExpressionSyntax);
                            nodes = last.Source?.GetSymbolInfo().References.DescendantNodes().Where(c => c is InvocationExpressionSyntax);
                        }
                        else if (MetaData.AnalysisInfo.Invocations.TryGetValue(symbols.Source.GetFullNameOrig(), out ConcurrentDictionary<string, MetaData.SymbolInfo> invocations))
                        {
                            //nodes = symbols.Syntax.SyntaxTree.GetRoot().DescendantNodes().Where(c => c is InvocationExpressionSyntax);
                            //nodes2 = invocations.Values.Select(c => c.Syntax).ToList();
                            nodes = invocations.Values.Select(c => c.Syntax);
                        }

                        if (null != nodes)
                        {
                            foreach (var item in nodes)
                            {
                                var callSymbols = GetSymbolInfo(item);

                                var call = callSymbols.Symbol?.OriginalDefinition ?? callSymbols.Symbol;

                                if (call?.Equals(symbols.Source) ?? false)
                                {
                                    var parameterSymbol = symbols.Declared as IParameterSymbol;
                                    var parameter = GetMethodParameters(callSymbols).FirstOrDefault(c => c.Key.Ordinal == parameterSymbol.Ordinal).Value;

                                    if (null != parameter)
                                    {
                                        result.AddRange(GetGenericType(GetSymbolInfo(parameter), path));
                                    }
                                }
                            }
                        }
                    }
                    break;
                case ElementAccessExpressionSyntax node:
                    result.AddRange(GetGenericType(GetSymbolInfo(node.Expression), path));
                    break;
                default: break;
            }

            if (null != symbols.Symbol)
            {
                if (symbols.Symbol.IsStatic)
                {
                    if ((symbols.Symbol is IFieldSymbol || symbols.Symbol is IPropertySymbol) && MetaData.AnalysisInfo.StaticAssignments.TryGetValue(symbols.Symbol.GetFullName(), out ConcurrentDictionary<string, AssignmentExpressionSyntax> assignments))
                    {
                        foreach (AssignmentExpressionSyntax item in assignments.Values)
                        {
                            result.AddRange(GetGenericType(GetSymbolInfo(item.Right), path));
                        }
                    }
                }// (SymbolKind.Local == symbols.Symbol.Kind || SymbolKind.Parameter == symbols.Symbol.Kind) &&
                else if (symbols.Syntax is IdentifierNameSyntax && null != symbols.Source && symbols.Source is IMethodSymbol)
                {
                    //var nodes = symbols.Source.GetReferences().DescendantNodes().Where(c => c is AssignmentExpressionSyntax);
                    var nodes = symbols.Source.GetSymbolInfo().References.DescendantNodes().Where(c => c is AssignmentExpressionSyntax);

                    foreach (var item in nodes)
                    {
                        if (item is AssignmentExpressionSyntax ass && ass.Left.IsEquivalentTo(symbols.Syntax, true))
                        {
                            result.AddRange(GetGenericType(GetSymbolInfo(ass.Right), path));
                        }
                    }
                }
            }

            //if (item is AssignmentExpressionSyntax ass && ass.Left.IsEquivalentTo(syntax, true))
            //{

            //}
            //if (symbols.Assignment.Any())
            //{
            //    foreach (AssignmentExpressionSyntax item in symbols.Assignment)
            //    {
            //        result.AddRange(GetGenericType2(item.Right.GetSymbolInfo(), path));
            //    }
            //}

            return result;
        }

        public static IEnumerable<MetaData.SymbolInfo> GetMethodGenericParameters(string methodFullName, SyntaxNode syntaxNode = null, int? ordinal = null, ISymbol? other = null)
        {
            if (methodFullName is null)
            {
                throw new ArgumentNullException(nameof(methodFullName));
            }

            var list = new List<MetaData.SymbolInfo>();
            IEnumerable<MetaData.SymbolInfo> nodes = null;

            if (null == syntaxNode)
            {
                if (MetaData.AnalysisInfo.Invocations.TryGetValue(methodFullName, out ConcurrentDictionary<string, MetaData.SymbolInfo> invocations))
                {
                    nodes = invocations.Values;
                }
            }
            else
            {
                nodes = syntaxNode.DescendantNodes().Where(c => c is InvocationExpressionSyntax).Select(c => c.GetSymbolInfo());
            }

            if (nodes.Any())
            {
                foreach (var item in nodes)
                {
                    var method = item.Symbol as IMethodSymbol;
                    var name = method.GetFullNameOrig();//method?.ReducedFrom?.ToDisplayString() ?? method?.ToDisplayString();
                                                        //Business.Core.Result.ResultFactory.ResultCreate<Business.Core.Result.ResultFactory.Data>(System.Type, Business.Core.Result.ResultFactory.Data, System.String, System.Int32, System.String, System.Boolean, System.Boolean, System.Boolean)

                    //{ResultCreate<Data>(resultTypeDefinition, state: state, message: message, callback: callback, checkData: false, hasDataResult: false)}
                    //Syntax = InvocationExpressionSyntax InvocationExpression ResultCreate<Data>(resultTypeDefinition, state: state, message: message, callback: callback, checkData: false, hasDataResult: false)
                    //Parent (Microsoft.CodeAnalysis.SyntaxNode) = MethodDeclarationSyntax MethodDeclaration public static IResult<Data> ResultCreate<Data>(this System.Type resultTypeDefinition, int state, string message, string callback = null) => ResultCreate<Data>(resultTypeDefinition, state: state, message: message, ca...
                    if (!methodFullName.Equals(name))
                    {
                        continue;
                    }

                    if (ordinal.HasValue)
                    {
                        var parameter = GetMethodParameter(item, ordinal.Value);

                        if (null != parameter.Value)
                        {
                            list.AddRange(parameter.Value.GetSymbolInfo().GetGenericType());
                        }
                        else if (SymbolKind.TypeParameter == item.Declared.Kind)
                        {
                            //var typeParameter = item.Declared.GetReferences().GetSymbolInfo();
                            var typeParameter = item.Declared.GetSymbolInfo();

                            list.AddRange(GetMethodGenericParameters(typeParameter.Source.GetFullNameOrig(), other: item.Declared));
                        }
                        else
                        {
                            list.AddRange(item.GetGenericType());
                        }
                    }
                    else if (null != other)
                    {
                        var parameters = GetGenericParameter(method, item, other);

                        if (parameters.Any())
                        {
                            foreach (var item2 in parameters)
                            {
                                if (null == item2.Value)
                                {
                                    continue;
                                }

                                list.AddRange(item2.Value.GetSymbolInfo().GetGenericType());
                            }
                        }
                        else if (SymbolKind.TypeParameter == item.Declared.Kind)
                        {
                            //var typeParameter = item.Declared.GetReferences().GetSymbolInfo();
                            var typeParameter = item.Declared.GetSymbolInfo();

                            list.AddRange(GetMethodGenericParameters(typeParameter.Source.GetFullNameOrig(), other: item.Declared));
                        }
                        else
                        {
                            list.AddRange(item.GetGenericType());
                        }
                    }
                    else
                    {
                        var parameters = GetMethodParameters(item);

                        foreach (var item2 in parameters)
                        {
                            if (null == item2.Value)
                            {
                                continue;
                            }

                            list.AddRange(item2.Value.GetSymbolInfo().GetGenericType());
                        }
                    }
                }
            }

            return list;
        }

        public static MetaData.SymbolInfo GetSymbolInfo(this ISymbol symbol)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            SyntaxNode syntax;

            var key = symbol.GetFullName();

            if (symbol is ITypeSymbol && MetaData.AnalysisInfo.TypeSymbols.TryGetValue(key, out MetaData.SymbolInfo type))
            {
                syntax = type.Declared.DeclaringSyntaxReferences.FirstOrDefault().GetSyntax();
            }
            else
            {
                syntax = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
            }

            if (null != syntax && MetaData.AnalysisInfo.DeclaredSymbols.TryGetValue(syntax.GetFullName(), out MetaData.SymbolInfo info))
            {
                return info;
            }

            return default;
        }

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

        static MetaData.SymbolInfo GetLastMethod(ISymbol source, List<MetaData.SymbolInfo> path)
        {
            var path2 = path.Where(c => c.Symbol is IMethodSymbol);

            //var path3 = path2.SkipWhile(c => !source.Equals(c.Source)).ToList();

            return path2.TakeWhile(c => !source.Equals(c.Source)).LastOrDefault();
        }

        static Dictionary<IParameterSymbol, ExpressionSyntax> GetMethodParameters(ISymbol symbol, AttributeSyntax node)
        {
            var method = symbol as IMethodSymbol;

            var nameColons = new Dictionary<string, ExpressionSyntax>();
            var colons = new List<ExpressionSyntax>();

            foreach (var arg in node.ArgumentList.Arguments)
            {
                if (null == arg.NameColon)
                {
                    colons.Add(arg.Expression);
                }
                else
                {
                    nameColons.Add(arg.NameColon.Name.ToString(), arg.Expression);
                }
            }

            //if (!method.IsStatic && null != method.ReducedFrom)
            //{
            //    var memberAccess = node.Expression as MemberAccessExpressionSyntax;
            //    colons.Insert(0, memberAccess.Expression);
            //    method = method.ReducedFrom.OriginalDefinition;
            //}

            var i = 0;

            return method.Parameters.ToDictionary(c => c, c =>
            {
                if (nameColons.TryGetValue(c.Name, out ExpressionSyntax col))
                {
                    return col;
                }

                if (i < colons.Count)
                {
                    return colons[i++];
                }

                return null;
            });
        }

        static Dictionary<IParameterSymbol, ExpressionSyntax> GetMethodParameters(MetaData.SymbolInfo symbols)
        {
            var method = symbols.Symbol as IMethodSymbol;
            var node = symbols.Syntax as InvocationExpressionSyntax;
            //node.Expression

            //IdentifierNameSyntax
            var nameColons = new Dictionary<string, ExpressionSyntax>();
            var colons = new List<ExpressionSyntax>();

            foreach (var arg in node.ArgumentList.Arguments)
            {
                if (null == arg.NameColon)
                {
                    colons.Add(arg.Expression);
                }
                else
                {
                    nameColons.Add(arg.NameColon.Name.ToString(), arg.Expression);
                }
            }

            if (!method.IsStatic && null != method.ReducedFrom)
            {
                var memberAccess = node.Expression as MemberAccessExpressionSyntax;
                colons.Insert(0, memberAccess.Expression);
                method = method.ReducedFrom.OriginalDefinition;
            }

            var i = 0;

            return method.Parameters.ToDictionary(c => c, c =>
            {
                if (nameColons.TryGetValue(c.Name, out ExpressionSyntax col))
                {
                    return col;
                }

                if (i < colons.Count)
                {
                    return colons[i++];
                }

                return null;
            });
        }

        static KeyValuePair<IParameterSymbol, ExpressionSyntax> GetMethodParameter(MetaData.SymbolInfo symbols, int ordinal)
        {
            var parameters = GetMethodParameters(symbols);

            if (ordinal < parameters.Count)
            {
                return parameters.ElementAt(ordinal);
            }

            return default;
        }

        static ITypeSymbol GetTypeParameter(ISymbol source, List<MetaData.SymbolInfo> path, ITypeSymbol symbol)
        {
            //var calls = path.Where(c => (c.Symbol is IMethodSymbol || c.Symbol is IPropertySymbol) && null != c.Source);
            var calls = path.Where(c => c.Symbol is IMethodSymbol || c.Symbol is IPropertySymbol || c.Symbol is IFieldSymbol);// || c.Syntax is FieldDeclarationSyntax

            var call = calls.LastOrDefault(c => c.Symbol.OriginalDefinition.Equals(source));

            if (!call.IsNull())
            {
                var symbol2 = symbol;

                if (symbol2 is ITypeParameterSymbol typeSymbol && null == typeSymbol.DeclaringMethod)
                {
                    //if (call.Source is IPropertySymbol || (call.Symbol is IPropertySymbol && call.Source is IMethodSymbol method))
                    symbol2 = GetTypeParameterToArgument(call.Symbol.ContainingType, symbol2);
                }
                else
                {
                    symbol2 = GetTypeParameterToArgument(call.Symbol, symbol2);
                }

                if (null == symbol2)
                {
                    return symbol;
                }

                return GetTypeParameter(call.Source, path, symbol2);
            }

            return symbol;
        }

        static ITypeSymbol GetTypeParameterToArgument(ISymbol symbol, ISymbol? other)
        {
            //if (!(symbol is IMethodSymbol method2))
            //{
            //    throw new ArgumentNullException(nameof(symbol));
            //}

            switch (symbol)
            {
                case IMethodSymbol node:
                    {
                        var parameter = node.GetDefinition().TypeParameters.FirstOrDefault(c => c.Equals(other));

                        if (null != parameter)
                        {
                            var arg = node.TypeArguments[parameter.Ordinal];

                            if (arg is ITypeParameterSymbol arg2 && null != arg2.DeclaringType)
                            {
                                switch (arg2.DeclaringType.TypeKind)
                                {
                                    case TypeKind.Class:
                                    case TypeKind.Interface:
                                    case TypeKind.Struct:
                                        return GetTypeParameterToArgument(node.ReceiverType, arg);
                                    default: break;
                                }
                            }

                            return arg;
                        }
                    }
                    break;
                case INamedTypeSymbol node:
                    {
                        switch (node.TypeKind)
                        {
                            case TypeKind.Class:
                            case TypeKind.Interface:
                            case TypeKind.Struct:
                                var parameter = node.TypeParameters.FirstOrDefault(c => c.Equals(other));

                                if (null != parameter)
                                {
                                    return node.TypeArguments[parameter.Ordinal];
                                }
                                break;
                            default: break;
                        }
                    }
                    break;
                default: break;
            }

            return null;
        }

        static IEnumerable<KeyValuePair<IParameterSymbol, ExpressionSyntax>> GetGenericParameter(IMethodSymbol method, MetaData.SymbolInfo call, ISymbol? other)
        {
            method = method.GetDefinition();

            var types = method.Parameters.Where(c => c.Type == other);

            var parameters = GetMethodParameters(call);

            return parameters.Where(c => types.Any(c2 => c2.Ordinal == c.Key.Ordinal));
        }

        static KeyValuePair<IParameterSymbol, ExpressionSyntax> GetGenericReturnParameter(IMethodSymbol method, MetaData.SymbolInfo node)
        {
            var parameters = GetMethodParameters(node);

            method = method.GetDefinition();

            var type = method.Parameters.FirstOrDefault(c => c.Type == method.ReturnType);

            var parameter = parameters.FirstOrDefault(c => c.Key.Ordinal == type?.Ordinal);

            return parameter;
        }

        const string dict = "System.Collections.Generic.IDictionary";
        const string coll = "System.Collections.Generic.ICollection";

        static bool IsIDict(this ISymbol symbol, out KeyValuePair<ITypeSymbol, ITypeSymbol> kv)
        {
            if (symbol is ITypeSymbol type)
            {
                foreach (var item in type.AllInterfaces)
                {
                    if (item.GetFullName().StartsWith(dict))
                    {
                        kv = new KeyValuePair<ITypeSymbol, ITypeSymbol>(item.TypeArguments[0], item.TypeArguments[1]);

                        return true;
                    }
                }
            }

            return false;
        }
        static bool IsIColl(this ISymbol symbol, out ITypeSymbol k)
        {
            k = default;

            if (symbol is ITypeSymbol type)
            {
                foreach (var item in type.AllInterfaces)
                {
                    if (item.GetFullName().StartsWith(coll))
                    {
                        k = item.TypeArguments[0];

                        return true;
                    }
                }
            }

            return false;
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

        public static bool FullNameIsFilePath(string fullName, string filePath)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                throw new ArgumentNullException(nameof(fullName));
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            return fullName.Split('@')[0].Equals(filePath);
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
            public GetFullNameOpt(bool noArgs = false, bool standardFormat = false)
            //bool isExplicitlyNamedTupleElement = true, bool dynamic2Object = false, bool noNullable = false, 
            {
                NoArgs = noArgs;
                StandardFormat = standardFormat;
                //IsExplicitlyNamedTupleElement = isExplicitlyNamedTupleElement;
                //Dynamic2Object = dynamic2Object;
                //NoNullable = noNullable;
            }

            public bool NoArgs { get; }

            public bool StandardFormat { get; }

            //public bool IsExplicitlyNamedTupleElement { get; }

            //public bool Dynamic2Object { get; }

            //public bool NoNullable { get; }
        }

        public static string GetFullName(this ISymbol symbol, GetFullNameOpt opt = default)
        {
            const string objectName = nameof(System) + "." + nameof(Object);

            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            var prefix = $"{symbol.ContainingType?.ToDisplayString() ?? (null != symbol.ContainingNamespace && !symbol.ContainingNamespace.IsGlobalNamespace ? symbol.ContainingNamespace.ToDisplayString() : null)}";

            if (!string.IsNullOrEmpty(prefix))
            {
                var named = symbol as INamedTypeSymbol;

                string nullable = null;

                if (symbol is ITypeSymbol type && NullableAnnotation.Annotated == type.NullableAnnotation)
                {
                    nullable = opt.StandardFormat ? string.Empty : "?";

                    if ("System.Nullable".Equals($"{prefix}.{symbol.Name}") && 0 < named?.TypeArguments.Length)
                    {
                        return $"{ GetFullName(named.TypeArguments[0], opt)}{nullable}";
                    }
                }

                string args = null;

                if (null != named && 0 < named.TypeArguments.Length)
                {
                    if (named.IsUnboundGenericType)
                    {
                        args = $"<{string.Join(",", Enumerable.Repeat(string.Empty, named.TypeArguments.Length))}>";
                    }
                    else if (named.IsTupleType)
                    {
                        if ("System.ValueTuple".Equals($"{prefix}.{symbol.Name}") && 0 < named?.TupleElements.Length)
                        {
                            return $"({string.Join(", ", named.TupleElements.Select(c => (c.IsExplicitlyNamedTupleElement && !opt.StandardFormat) ? $"{(TypeKind.Dynamic == c.Type.TypeKind && opt.StandardFormat ? objectName : GetFullName(c.Type, opt))} {c.Name}" : (TypeKind.Dynamic == c.Type.TypeKind && opt.StandardFormat ? objectName : GetFullName(c.Type, opt))))})";
                        }
                    }
                    else
                    {
                        args = $"<{string.Join(", ", named.TypeArguments.Select(c => (TypeKind.Dynamic == c.TypeKind && opt.StandardFormat ? objectName : GetFullName(c, opt))))}>";
                    }
                }

                string parameters = null;

                if (symbol is IMethodSymbol method)
                {
                    if (0 < method.TypeArguments.Length)
                    {
                        args = $"<{string.Join(", ", method.TypeArguments.Select(c => (TypeKind.Dynamic == c.TypeKind && opt.StandardFormat ? objectName : GetFullName(c, opt))))}>";
                    }

                    parameters = $"({string.Join(", ", method.Parameters.Select(c => (TypeKind.Dynamic == c.Type.TypeKind && opt.StandardFormat ? objectName : GetFullName(c.Type, opt))))})";
                }

                if (opt.NoArgs)
                {
                    return $"{prefix}.{symbol.Name}";
                }

                return $"{prefix}.{symbol.Name}{args}{nullable}{parameters}";
            }

            if (symbol is INamedTypeSymbol anonymousType && anonymousType.IsAnonymousType)
            {
                return string.Join(", ", anonymousType.Constructors.Select(c =>
                {
                    var parameters = c.Parameters.Select(c2 =>
                    {
                        var type = TypeKind.Dynamic == c2.Type.TypeKind && opt.StandardFormat ? objectName : GetFullName(c2.Type, opt);

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

                return $"{GetFullName(array.ElementType, opt)}{rank}";
            }

            return symbol.ToDisplayString();
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

        #region MakeGenericType

        /// <summary>
        /// replace generic
        /// </summary>
        /// <param name="syntaxParameter"></param>
        /// <param name="replace"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static (FileScopedNamespaceDeclarationSyntax Declaration, UsingDirectiveSyntax[] Usings) MakeGenericTypeAll(this TypeParameterSyntax syntaxParameter, BaseTypeDeclarationSyntax replace, string name = null)
        {
            if (syntaxParameter is null)
            {
                throw new ArgumentNullException(nameof(syntaxParameter));
            }

            var usings = new List<UsingDirectiveSyntax>(syntaxParameter.SyntaxTree.GetCompilationUnitRoot().Usings);

            if (MetaData.AnalysisInfo.DeclaredSymbols.TryGetValue(replace.GetFullName(), out MetaData.SymbolInfo replaceInfo))
            {
                var declared = replaceInfo.Declared;
                //var name2 = declared.GetFullName();
                //var replaceName = replaceInfo.Declared.GetFullName();
                //replaceIdentifier = SyntaxFactory.ParseToken(replaceName);
                var prefix = $"{declared.ContainingType?.ToDisplayString() ?? (null != declared.ContainingNamespace && !declared.ContainingNamespace.IsGlobalNamespace ? declared.ContainingNamespace.ToDisplayString() : null)}";

                usings.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(prefix)));
            }

            var declarationSyntax = syntaxParameter.Parent.Parent;
            FileScopedNamespaceDeclarationSyntax namespaceScoped = null;

            if (MetaData.AnalysisInfo.DeclaredSymbols.TryGetValue(declarationSyntax.GetFullName(), out MetaData.SymbolInfo origInfo))
            {
                namespaceScoped = SyntaxFactory.FileScopedNamespaceDeclaration(SyntaxFactory.ParseName(origInfo.Declared.ContainingNamespace.ToDisplayString()));
            }

            var nodes = GetNodes(syntaxParameter).Select(c =>
            {
                if (c.Parent is TypeParameterConstraintClauseSyntax)
                {
                    return c.Parent;
                }
                return c;
            });

            switch (declarationSyntax)
            {
                case ClassDeclarationSyntax node:
                    declarationSyntax = WithIdentifier(ClassOrStruct(node), name ?? $"{node.Identifier}_{replace.Identifier}"); break;
                case StructDeclarationSyntax node:
                    declarationSyntax = WithIdentifier(ClassOrStruct(node), name ?? $"{node.Identifier}_{replace.Identifier}"); break;
                case MethodDeclarationSyntax node:
                    declarationSyntax = WithIdentifier(ClassOrStruct(node), name ?? $"{node.Identifier}_{replace.Identifier}"); break;
                default: break;
            }

            return (namespaceScoped.AddMembers(declarationSyntax as MemberDeclarationSyntax), usings.ToArray());

            Syntax ClassOrStruct<Syntax>(Syntax node) where Syntax : MemberDeclarationSyntax
            {
                node = node.TrackNodes(nodes);

                foreach (var item in nodes)
                {
                    switch (node.GetCurrentNode(item))
                    {
                        case TypeParameterSyntax node2:
                            node = RemoveNode(node, node2);
                            break;
                        case TypeParameterConstraintClauseSyntax node2:
                            node = RemoveNode(node, node2);
                            break;
                        case IdentifierNameSyntax node2:
                            node = node.ReplaceToken(node2.Identifier, replace.Identifier);
                            break;
                        default: break;
                    }
                }

                return node;
            }
        }

        /// <summary>
        /// replace generic
        /// </summary>
        /// <param name="syntaxParameter"></param>
        /// <param name="replace"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static (FileScopedNamespaceDeclarationSyntax Declaration, UsingDirectiveSyntax[] Usings) MakeGenericTypeAll(this TypeParameterSyntax syntaxParameter, MetaData.SymbolInfo replace, string name = null)
        {
            if (syntaxParameter is null)
            {
                throw new ArgumentNullException(nameof(syntaxParameter));
            }

            var usings = new List<UsingDirectiveSyntax>(syntaxParameter.SyntaxTree.GetCompilationUnitRoot().Usings);

            var declared = replace.Declared;
            //var name2 = declared.GetFullName();
            //var replaceName = replaceInfo.Declared.GetFullName();
            //replaceIdentifier = SyntaxFactory.ParseToken(replaceName);
            var prefix = $"{declared.ContainingType?.ToDisplayString() ?? (null != declared.ContainingNamespace && !declared.ContainingNamespace.IsGlobalNamespace ? declared.ContainingNamespace.ToDisplayString() : null)}";

            usings.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(prefix)));

            var declarationSyntax = syntaxParameter.Parent.Parent;
            FileScopedNamespaceDeclarationSyntax namespaceScoped = null;

            if (MetaData.AnalysisInfo.DeclaredSymbols.TryGetValue(declarationSyntax.GetFullName(), out MetaData.SymbolInfo origInfo))
            {
                namespaceScoped = SyntaxFactory.FileScopedNamespaceDeclaration(SyntaxFactory.ParseName(origInfo.Declared.ContainingNamespace.ToDisplayString()));
            }

            var nodes = GetNodes(syntaxParameter).Select(c =>
            {
                if (c.Parent is TypeParameterConstraintClauseSyntax)
                {
                    return c.Parent;
                }
                return c;
            });

            var identifier = replace.GetFullName();

            switch (declarationSyntax)
            {
                case ClassDeclarationSyntax node:
                    declarationSyntax = WithIdentifier(ClassOrStruct(node), name ?? $"{node.Identifier}_{identifier}"); break;
                case StructDeclarationSyntax node:
                    declarationSyntax = WithIdentifier(ClassOrStruct(node), name ?? $"{node.Identifier}_{identifier}"); break;
                case MethodDeclarationSyntax node:
                    declarationSyntax = WithIdentifier(ClassOrStruct(node), name ?? $"{node.Identifier}_{identifier}"); break;
                default: break;
            }

            return (namespaceScoped.AddMembers(declarationSyntax as MemberDeclarationSyntax), usings.ToArray());

            Syntax ClassOrStruct<Syntax>(Syntax node) where Syntax : MemberDeclarationSyntax
            {
                node = node.TrackNodes(nodes);

                foreach (var item in nodes)
                {
                    switch (node.GetCurrentNode(item))
                    {
                        case TypeParameterSyntax node2:
                            node = RemoveNode(node, node2);
                            break;
                        case TypeParameterConstraintClauseSyntax node2:
                            node = RemoveNode(node, node2);
                            break;
                        case IdentifierNameSyntax node2:
                            //node = node.ReplaceToken(node2.Identifier, replace.s);
                            break;
                        default: break;
                    }
                }

                return node;
            }
        }
        /*
        public static SyntaxNode MakeGenericType(this TypeParameterSyntax syntaxParameter, BaseTypeDeclarationSyntax replace, string name = null)
        {
            var all = MakeGenericTypeAll(syntaxParameter, replace, name);
            return all.Declaration.AddUsings(all.Usings);
        }
        */
        public static SyntaxNode MakeGenericType(this TypeParameterSyntax syntaxParameter, BaseTypeDeclarationSyntax replace, string name = null)
        {
            if (syntaxParameter is null)
            {
                throw new ArgumentNullException(nameof(syntaxParameter));
            }

            var declarationSyntax = syntaxParameter.Parent.Parent;
            FileScopedNamespaceDeclarationSyntax namespaceScoped = null;

            if (MetaData.AnalysisInfo.DeclaredSymbols.TryGetValue(declarationSyntax.GetFullName(), out MetaData.SymbolInfo origInfo))
            {
                namespaceScoped = SyntaxFactory.FileScopedNamespaceDeclaration(SyntaxFactory.ParseName(origInfo.Declared.ContainingNamespace.ToDisplayString()));
            }

            foreach (var item in syntaxParameter.SyntaxTree.GetCompilationUnitRoot().Usings)
            {
                namespaceScoped = namespaceScoped.AddUsings(item);
            }

            if (MetaData.AnalysisInfo.DeclaredSymbols.TryGetValue(replace.GetFullName(), out MetaData.SymbolInfo replaceInfo))
            {
                var declared = replaceInfo.Declared;
                //var name2 = declared.GetFullName();
                //var replaceName = replaceInfo.Declared.GetFullName();
                //replaceIdentifier = SyntaxFactory.ParseToken(replaceName);
                var prefix = $"{declared.ContainingType?.ToDisplayString() ?? (null != declared.ContainingNamespace && !declared.ContainingNamespace.IsGlobalNamespace ? declared.ContainingNamespace.ToDisplayString() : null)}";

                namespaceScoped = namespaceScoped.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(prefix)));
            }

            var nodes = GetNodes(syntaxParameter).Select(c =>
            {
                if (c.Parent is TypeParameterConstraintClauseSyntax)
                {
                    return c.Parent;
                }
                return c;
            });

            switch (declarationSyntax)
            {
                case ClassDeclarationSyntax node:
                    declarationSyntax = WithIdentifier(ClassOrStruct(node), name ?? $"{node.Identifier}_{replace.Identifier}"); break;
                case StructDeclarationSyntax node:
                    declarationSyntax = WithIdentifier(ClassOrStruct(node), name ?? $"{node.Identifier}_{replace.Identifier}"); break;
                case MethodDeclarationSyntax node:
                    declarationSyntax = WithIdentifier(ClassOrStruct(node), name ?? $"{node.Identifier}_{replace.Identifier}"); break;
                default: break;
            }

            return namespaceScoped.AddMembers(declarationSyntax as MemberDeclarationSyntax);

            Syntax ClassOrStruct<Syntax>(Syntax node) where Syntax : MemberDeclarationSyntax
            {
                node = node.TrackNodes(nodes);

                foreach (var item in nodes)
                {
                    switch (node.GetCurrentNode(item))
                    {
                        case TypeParameterSyntax node2:
                            node = RemoveNode(node, node2);
                            break;
                        case TypeParameterConstraintClauseSyntax node2:
                            node = RemoveNode(node, node2);
                            break;
                        case IdentifierNameSyntax node2:
                            node = node.ReplaceToken(node2.Identifier, replace.Identifier);
                            break;
                        default: break;
                    }
                }

                return node;
            }
        }

        static bool IsEquivalentTo(SyntaxToken syntaxToken, SyntaxToken target, bool withoutTrivia = false) => withoutTrivia ? syntaxToken.WithoutTrivia().IsEquivalentTo(target) : syntaxToken.IsEquivalentTo(target);

        /// <summary>
        /// WithIdentifier
        /// </summary>
        /// <param name="syntaxNode"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        static SyntaxNode WithIdentifier(SyntaxNode syntaxNode, string name)
        {
            if (syntaxNode is null)
            {
                throw new ArgumentNullException(nameof(syntaxNode));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"\"{nameof(name)}\" cannot be null or empty.", nameof(name));
            }

            var identifier = SyntaxFactory.ParseToken(name);

            switch (syntaxNode)
            {
                case ClassDeclarationSyntax node:
                    {
                        var constructors = node.Members.Where(c => c is ConstructorDeclarationSyntax);
                        node = node.TrackNodes(constructors);

                        foreach (ConstructorDeclarationSyntax item in constructors)
                        {
                            node = node.WithMembers(node.Members.Replace(node.GetCurrentNode(item), item.WithIdentifier(identifier)));
                        }

                        return node.WithIdentifier(identifier);
                    }
                case StructDeclarationSyntax node:
                    {
                        var constructors = node.Members.Where(c => c is ConstructorDeclarationSyntax);
                        node = node.TrackNodes(constructors);

                        foreach (ConstructorDeclarationSyntax item in constructors)
                        {
                            node = node.WithMembers(node.Members.Replace(node.GetCurrentNode(item), item.WithIdentifier(identifier)));
                        }

                        return node.WithIdentifier(identifier);
                    }
                case MethodDeclarationSyntax node:
                    return node.WithIdentifier(identifier);
                default: return syntaxNode;
            }
        }

        public static IEnumerable<SyntaxToken> GetTokens(this TypeParameterSyntax syntaxParameter)
        {
            if (syntaxParameter is null)
            {
                throw new ArgumentNullException(nameof(syntaxParameter));
            }

            var triviaTarget = syntaxParameter.Identifier.WithoutTrivia();

            //ISymbol declared = null;

            if (!MetaData.AnalysisInfo.DeclaredSymbols.TryGetValue(syntaxParameter.GetFullName(), out MetaData.SymbolInfo info))
            {
                throw new ArgumentNullException(nameof(syntaxParameter));
            }

            switch (syntaxParameter.Parent.Parent)
            {
                case ClassDeclarationSyntax node: return ClassOrStruct(node);
                case StructDeclarationSyntax node: return ClassOrStruct(node);
                case MethodDeclarationSyntax node:
                    return node.DescendantTokens().Where(c => IsEquivalentTo(c, triviaTarget, true));
                default: return Array.Empty<SyntaxToken>();
            }

            IEnumerable<SyntaxToken> ClassOrStruct(TypeDeclarationSyntax node)
            {
                var exist = node.Members.SelectMany(c =>
                {
                    if (c is MethodDeclarationSyntax method)
                    {
                        var parameter = method.TypeParameterList?.Parameters.FirstOrDefault(c => IsEquivalentTo(c.Identifier, triviaTarget, true));

                        if (null != parameter)
                        {
                            return GetTokens(parameter);
                        }
                    }
                    return Array.Empty<SyntaxToken>();
                });

                var members = node.DescendantTokens().Where(c => IsEquivalentTo(c, triviaTarget, true) && !exist.Any(c2 => c2.Span == c.Span));

                var members2 = new List<SyntaxToken>();
                foreach (var item in members)
                {
                    if (MetaData.AnalysisInfo.DeclaredSymbols.TryGetValue(item.Parent.GetFullName(), out MetaData.SymbolInfo info2))
                    {
                        if (info.Declared.Equals(info2.Declared))
                        {
                            members2.Add(item);
                        }
                    }
                }

                return members2;
            }
        }

        public static IEnumerable<SyntaxNode> GetNodes(this TypeParameterSyntax syntaxParameter)
        {
            if (syntaxParameter is null)
            {
                throw new ArgumentNullException(nameof(syntaxParameter));
            }

            if (!MetaData.AnalysisInfo.DeclaredSymbols.TryGetValue(syntaxParameter.GetFullName(), out MetaData.SymbolInfo targetInfo))
            {
                throw new ArgumentNullException(nameof(syntaxParameter));
            }

            var triviaTarget = syntaxParameter.Identifier.WithoutTrivia();
            var owner = syntaxParameter.Parent.Parent;

            switch (owner.RawKind)
            {
                case (int)SyntaxKind.ClassDeclaration:
                case (int)SyntaxKind.StructDeclaration:
                case (int)SyntaxKind.MethodDeclaration: return FindNodes(owner);
                default: return Array.Empty<SyntaxNode>();
            }

            IEnumerable<SyntaxNode> FindNodes(SyntaxNode node)
            {
                var members = new List<SyntaxNode>();

                foreach (var item in node.DescendantTokens())
                {
                    if (!IsEquivalentTo(item, triviaTarget, true))
                    {
                        continue;
                    }

                    var parent = item.Parent;

                    if (MetaData.AnalysisInfo.DeclaredSymbols.TryGetValue(parent.GetFullName(), out MetaData.SymbolInfo itemInfo))
                    {
                        if (targetInfo.Declared.Equals(itemInfo.Declared))
                        {
                            members.Add(parent);
                        }
                    }
                }

                return members;
            }
        }

        public static VariableDeclaratorSyntax FindField(this IEnumerable<SyntaxNode> nodes, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }

            foreach (var item in nodes)
            {
                if (item is FieldDeclarationSyntax field)
                {
                    var variable = field.Declaration.Variables.FirstOrDefault(c2 => name == c2.Identifier.Text);

                    if (null != variable)
                    {
                        return variable;
                    }
                }
            }

            return null;
        }

        public static FieldDeclarationSyntax FindField2(this IEnumerable<SyntaxNode> nodes, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }

            foreach (var item in nodes)
            {
                if (item is FieldDeclarationSyntax field)
                {
                    if (field.Declaration.Variables.Any(c2 => name == c2.Identifier.Text))
                    {
                        return field;
                    }
                }
            }

            return null;
        }

        //public static VariableDeclaratorSyntax CollectInitializer(VariableDeclaratorSyntax variable, System.Collections.IDictionary dict = null)
        //{
        //    var init = variable.Initializer.Value as ObjectCreationExpressionSyntax;

        //    var args = init.ArgumentList.Arguments.FirstOrDefault();

        //    var argExp = args.Expression as ObjectCreationExpressionSyntax;

        //    var str = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.StringLiteralToken, "string", "string", SyntaxTriviaList.Empty));
        //    var type = SyntaxFactory.TypeOfExpression(SyntaxFactory.ParseTypeName("string"));
        //    //var element = SyntaxFactory.InitializerExpression(SyntaxKind.ComplexElementInitializerExpression, new SeparatedSyntaxList<ExpressionSyntax>().AddRange(new ExpressionSyntax[] { str, type }));
        //    var collect = new SeparatedSyntaxList<ExpressionSyntax>().Add(SyntaxFactoryEx.ParseElementInitializer(str, type));

        //    argExp = argExp.WithInitializer(argExp.Initializer.WithExpressions(collect));

        //    args = args.WithExpression(argExp);

        //    init = init.WithArgumentList(init.ArgumentList.WithArguments(new SeparatedSyntaxList<ArgumentSyntax>().Add(args)));

        //    return variable.WithInitializer(variable.Initializer.WithValue(init));
        //}



        public static string ToString(this IEnumerable<string> values, string separator) => string.Join(separator, values);
        public static string ToString(this ReadOnlySpan<string> values, string separator) => string.Join(separator, values.ToArray());
        public static string ToString(this Span<string> values, string separator) => string.Join(separator, values.ToArray());
        public static string ToString(this IEnumerable<string> values, char separator) => string.Join(new string(new char[] { separator }), values);
        public static string ToString(this ReadOnlySpan<string> values, char separator) => string.Join(new string(new char[] { separator }), values.ToArray());
        public static string ToString(this Span<string> values, char separator) => string.Join(new string(new char[] { separator }), values.ToArray());
        /*
        public static string ToCode2(this object obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            //new System.Collections.Generic.Dictionary<string, System.Type> { }
            var type = obj.GetType();

            var name = type.GetFormattedName(TypeNameFormatOptions.Namespaces | TypeNameFormatOptions.NoGeneric);


            switch (name)
            {
                case "System.Collections.ObjectModel.ReadOnlyDictionary":
                    //var obj2 = obj as System.Collections.IDictionary;



                    break;
                default:
                    if (obj is System.Collections.IDictionary dict)
                    {
                        var obj2 = obj as System.Collections.IDictionary;

                        var name2 = type.GetFormattedName();


                    }
                    break;
            }


            return null;
        }
        */
        #region RemoveNode

        static SyntaxRemoveOptions DefaultRemoveOptions
        {
            get { return SyntaxRemoveOptions.KeepExteriorTrivia | SyntaxRemoveOptions.KeepUnbalancedDirectives; }
        }
        static TRoot RemoveNode<TRoot>(TRoot root, SyntaxNode node) where TRoot : SyntaxNode
        {
            return root.RemoveNode(node, GetRemoveOptions(node));
        }
        static SyntaxRemoveOptions GetRemoveOptions(SyntaxNode node)
        {
            SyntaxRemoveOptions removeOptions = DefaultRemoveOptions;

            if (IsEmptyOrWhitespace(node.GetLeadingTrivia()))
                removeOptions &= ~SyntaxRemoveOptions.KeepLeadingTrivia;

            if (IsEmptyOrWhitespace(node.GetTrailingTrivia()))
                removeOptions &= ~SyntaxRemoveOptions.KeepTrailingTrivia;

            return removeOptions;
        }
        /// <summary>
        /// Returns true if the list of either empty or contains only whitespace (<see cref="SyntaxKind.WhitespaceTrivia"/> or <see cref="SyntaxKind.EndOfLineTrivia"/>).
        /// </summary>
        /// <param name="triviaList"></param>
        static bool IsEmptyOrWhitespace(SyntaxTriviaList triviaList)
        {
            foreach (SyntaxTrivia trivia in triviaList)
            {
                if (!IsWhitespaceOrEndOfLineTrivia(trivia))
                    return false;
            }

            return true;
        }
        /// <summary>
        /// Returns true if the trivia is either <see cref="SyntaxKind.WhitespaceTrivia"/> or <see cref="SyntaxKind.EndOfLineTrivia"/>.
        /// </summary>
        /// <param name="trivia"></param>
        static bool IsWhitespaceOrEndOfLineTrivia(SyntaxTrivia trivia)
        {
            return IsKind(trivia, SyntaxKind.WhitespaceTrivia, SyntaxKind.EndOfLineTrivia);
        }
        /// <summary>
        /// Returns true if a trivia's kind is one of the specified kinds.
        /// </summary>
        /// <param name="trivia"></param>
        /// <param name="kind1"></param>
        /// <param name="kind2"></param>
        static bool IsKind(SyntaxTrivia trivia, SyntaxKind kind1, SyntaxKind kind2)
        {
            SyntaxKind kind = trivia.Kind();

            return kind == kind1
                || kind == kind2;
        }

        #endregion

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
            var newLine2 = opt.StandardFormat ? $"{Environment.NewLine}{Environment.NewLine}" : newLine;
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
                        var array = node.RankSpecifiers.First();

                        value = $"{ToCode(node.ElementType, opt)}{ToCode(array, opt)}";
                    }
                    break;
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

                        if (null != node.NameColon)
                        {
                            value = $"{ToCode(node.NameColon, opt)} {value}";
                        }

                        if (default != node.RefOrOutKeyword)
                        {
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
                    value = $"{node.OpenBraceToken} {string.Join(", ", node.Expressions.Select(c => ToCode(c, opt)))} {node.CloseBraceToken}"; break;
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

        public static Compilation AddSyntaxTree(this Compilation compilation, string code, out SyntaxTree tree, ParseOptions? options = null, string path = null)
        {
            tree = SyntaxFactory.ParseSyntaxTree(code, options).WithFilePath(path ?? Guid.NewGuid().ToString("N"));
            return compilation.AddSyntaxTrees(tree);
        }

        public static Compilation AddSyntaxTree(this Compilation compilation, string code, ParseOptions? options = null, string path = null) => AddSyntaxTree(compilation, code, out _, options, path);

        /// <summary>
        /// Equality
        /// </summary>
        /// <typeparam name="T"></typeparam>
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

        public static IEnumerable<string> GetTypes(ConcurrentDictionary<string, MetaData.SymbolInfo> typeSymbols)
        {
            var list = new List<string>();

            foreach (var item in typeSymbols.Values)
            {
                if (null == item.References)
                {
                    continue;
                }

                if (!(item.Syntax.IsKind(SyntaxKind.IdentifierName) || item.Syntax.IsKind(SyntaxKind.GenericName) || item.Syntax.IsKind(SyntaxKind.InterfaceDeclaration) || item.Syntax.IsKind(SyntaxKind.ClassDeclaration) || item.Syntax.IsKind(SyntaxKind.StructDeclaration)))
                {
                    continue;
                }

                var typeSymbol = item.Declared as ITypeSymbol;

                if ((TypeKind.Interface != typeSymbol.TypeKind && TypeKind.Class != typeSymbol.TypeKind && TypeKind.Struct != typeSymbol.TypeKind))
                {
                    continue;
                }

                //if (typeSymbol2 is INamedTypeSymbol namedType && namedType.IsUnboundGenericType)
                //{
                //    continue;
                //}

                var name = typeSymbol.GetFullName(new Expression.GetFullNameOpt(standardFormat: true));

                if (!list.Contains(name))
                {
                    list.Add(name);
                }
            }

            if (0 < list.Count)
            {
                return list;
            }

            return Array.Empty<string>();
        }
    }
}
