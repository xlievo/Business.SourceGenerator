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
    using System.Buffers;
    using System.Collections.Generic;
    using System.Linq;
    using static Business.SourceGenerator.Analysis.AnalysisMeta;
    using static Business.SourceGenerator.Analysis.SymbolToMeta;
    using static Business.SourceGenerator.Analysis.SymbolTypeName;
    using static Business.SourceGenerator.Analysis.SyntaxToCode;

    internal class GeneratorType
    {
        public enum TypeKeyFormat
        {
            No,
            ToLower,
            ToUpper
        }

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

        public static IDictionary<INamedTypeSymbol, IEnumerable<INamedTypeSymbol>> GetMakeGenericTypes(AnalysisInfoModel analysisInfo)
        {
            var makeGenericTypes = new Dictionary<string, (INamedTypeSymbol, IEnumerable<INamedTypeSymbol>)>();

            foreach (var info in analysisInfo.Attributes)
            {
                if (Meta.Global.GeneratorTypeKey == info.Key)
                {
                    foreach (var item in info.Value)
                    {
                        var declarationInfo = item.Key.GetSymbolInfo();

                        var key = declarationInfo.Names.DeclaredStandard;

                        if (makeGenericTypes.ContainsKey(key))
                        {
                            continue;
                        }

                        //IResult<DataType, DataType2, DataType3>
                        if (!(declarationInfo.Declared is INamedTypeSymbol declared) || !(TypeKind.Interface == declared.TypeKind || TypeKind.Class == declared.TypeKind || TypeKind.Struct == declared.TypeKind) || declared.IsUnboundGenericType)
                        {
                            continue;
                        }

                        var typeSymbolName = declared.OriginalDefinition.GetFullName();

                        var dict = new Dictionary<string, INamedTypeSymbol>();

                        foreach (var item2 in analysisInfo.TypeSymbols.Values)
                        {
                            //if (!(item.Declared is ITypeSymbol declared))
                            //{
                            //    continue;
                            //}

                            //if (TypeKind.Interface != declared.TypeKind && TypeKind.Class != declared.TypeKind && TypeKind.Struct != declared.TypeKind)
                            //{
                            //    continue;
                            //}

                            if (!(item2.Declared is INamedTypeSymbol namedType) || namedType.IsUnboundGenericType)
                            {
                                continue;
                            }

                            string key2 = default;

                            switch (declared.TypeKind)
                            {
                                case TypeKind.Class:
                                    {
                                        var baseType = namedType;

                                        while (null != baseType)
                                        {
                                            //if (baseType.OriginalDefinition.Equals(typeSymbol.OriginalDefinition, SymbolEqualityComparer.Default))
                                            if (Expression.EqualsFullName(baseType.OriginalDefinition, typeSymbolName))
                                            {
                                                //key2 = namedType.GetFullNameStandardFormat();
                                                key2 = item2.Names.DeclaredStandard;
                                                break;
                                            }
                                            baseType = baseType.BaseType;
                                        }
                                        break;
                                    }
                                case TypeKind.Interface:
                                    {
                                        //if (typeSymbol2.OriginalDefinition.Equals(typeSymbol.OriginalDefinition) || typeSymbol2.AllInterfaces.Any(c => typeSymbol.OriginalDefinition.Equals(c.OriginalDefinition, SymbolEqualityComparer.Default)))
                                        if (Expression.EqualsFullName(namedType.OriginalDefinition, typeSymbolName) || namedType.AllInterfaces.Any(c => Expression.EqualsFullName(c.OriginalDefinition, typeSymbolName)))
                                        {
                                            //key2 = namedType.GetFullNameStandardFormat();
                                            key2 = item2.Names.DeclaredStandard;
                                        }
                                        break;
                                    }
                                default: break;
                            }

                            if (!(key2 is null) && !dict.ContainsKey(key2))
                            {
                                dict.Add(key2, namedType);
                            }
                        }

                        makeGenericTypes.Add(key, (declarationInfo.Declared as INamedTypeSymbol, dict.Values));
                    }
                }
            }

            return makeGenericTypes.ToDictionary(c => c.Value.Item1, c => c.Value.Item2);
        }
    }

    internal static class Expression
    {
        public static void Log(this GeneratorExecutionContext context, string message, DiagnosticSeverity diagnostic = DiagnosticSeverity.Warning) => context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("Business.SourceGenerator", string.Empty, message, string.Empty, diagnostic, true), Location.None));

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

        /*
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
        */

        public static string GetName(this Enum value) => null == value ? null : Enum.GetName(value.GetType(), value);

        public static bool EqualsFullName(this ISymbol symbol, string fullName)
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

        /*
        static IDictionary<string, ITypeSymbol> GetTypes2(this AnalysisInfoModel analysisInfo)
        {
            var dict = new Dictionary<string, ITypeSymbol>();

            foreach (var item in analysisInfo.TypeSymbols.Values)
            {
                var typeSymbol = item.Declared as ITypeSymbol;
                //if (!(item.Declared is ITypeSymbol typeSymbol))
                //{
                //    continue;
                //}

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

                var prefix = typeSymbol.GetFullName(GetFullNameOpt.Create(captureStyle: CaptureStyle.Prefix));

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

                var name = typeSymbol.GetFullNameStandardFormat(GetFullNameOpt.Create(noNullableQuestionMark: true));

                if (!dict.ContainsKey(name))
                {
                    dict.Add(name, typeSymbol);
                }
            }

            return dict;
        }

        static IEnumerable<AnalysisMeta.SymbolInfo> GetTypes(this AnalysisInfoModel analysisInfo)
        {
            return analysisInfo.TypeSymbols.Values.Where(c =>
            {
                var typeSymbol = c.Declared as ITypeSymbol;

                if (IsTypeParameter(typeSymbol))
                {
                    return false;
                }

                var prefix = typeSymbol.GetFullName(GetFullNameOpt.Create(captureStyle: CaptureStyle.Prefix));

                if (Meta.Global.BusinessSourceGeneratorMeta == prefix)
                {
                    return false;
                }

                return true;
            });
        }
        */
        static bool IsTypeParameter(ITypeSymbol typeSymbol)
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
                if (SymbolKind.ArrayType != typeSymbol.Kind)
                {
                    return true;
                }
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

        //static readonly string AccessorKey = TypeNameFormatter.TypeName.GetFormattedName(typeof(IGeneratorAccessor), TypeNameFormatter.TypeNameFormatOptions.Namespaces);

        static bool HasGeneratorAccessor(AnalysisMeta.SymbolInfo info)
        {
            if (!(info.Syntax is TypeDeclarationSyntax member))
            {
                return false;
            }

            if (!member.IsPartial())
            {
                return false;
            }

            var typeSymbol = info.Declared as ITypeSymbol;

            if (Accessibility.Public != typeSymbol.DeclaredAccessibility || typeSymbol.AllInterfaces.Any(c => Meta.Global.AccessorKey.Equals(c.GetFullName())))
            {
                return false;
            }

            return true;
        }

        /*
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
        */

        public static IDictionary<string, string> GeneratorAccessor(AnalysisInfoModel analysisInfo, string assemblyName, ToCodeOpt opt, string[] usings = default)
        {
            var format = opt.StandardFormat ? Environment.NewLine : " ";
            var usings2 = usings.Select(c => $"{c}.").Concat(new string[] { $"{assemblyName}." }).ToArray();
            string typeClean(string type, bool noGlobal = false) => !opt.Global ? TypeNameClean(type, usings2) : !noGlobal ? $"{GlobalConst.Global}{type}" : type;

            var declarations = analysisInfo.TypeSymbols.Values.Where(c => c.Names.AssemblyName.Equals(assemblyName) && c.IsDefinition && (c.Syntax.IsKind(SyntaxKind.ClassDeclaration) || c.Syntax.IsKind(SyntaxKind.StructDeclaration)));

            var sb = new System.Text.StringBuilder(null);

            var generatorAccessors = declarations.Where(c => HasGeneratorAccessor(c));

            var dict = new Dictionary<string, string>();

            foreach (var item in generatorAccessors)
            {
                var typeSymbol = item.Declared as INamedTypeSymbol;

                if (typeSymbol.IsStatic)
                {
                    continue;
                }

                //if (0 < sb.Length)
                //{
                //    sb.AppendLine();
                //    sb.AppendLine();
                //}

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

                var globalMeta = opt.GetGlobalName(GlobalName.Business_SourceGenerator_Meta);
                var globalSystem = opt.GetGlobalName(GlobalName.System);
                var globalString = opt.GetGlobalName(GlobalName.System_String);
                var globalObject = opt.GetGlobalName(GlobalName.System_Object);
                var globalBoolean = opt.GetGlobalName(GlobalName.System_Boolean);

                //sb.AppendFormat($"public partial {{1}} {{2}} : {globalMeta}IGeneratorAccessor{{0}}{{{{{{0}}", format, type, typeSymbol.GetFullName(new GetFullNameOpt(noPrefix: true)));

                //var IsUnsafe = typeSymbol.GetSymbolInfo().Syntax is TypeDeclarationSyntax typeDeclaration && typeDeclaration.Modifiers.Any(SyntaxKind.UnsafeKeyword);

                sb.AppendFormat($"public partial {{1}} {{2}} : {globalMeta}IGeneratorAccessor{{0}}{{{{{{0}}", format, type, typeSymbol.GetFullName(GetFullNameOpt.Create(captureStyle: CaptureStyle.NoPrefix)));
                sb.AppendFormat($"static readonly {globalSystem}Lazy<{globalMeta}IAccessorNamedType> generatorAccessorType = new {globalSystem}Lazy<{globalMeta}IAccessorNamedType>(() => {{1}});{{0}}", format, typeSymbol.ToMeta(opt, typeClean: typeClean));

                sb.AppendLine($"public static {globalMeta}IAccessorNamedType GeneratorAccessorType {{ get => generatorAccessorType.Value; }}");
                sb.AppendLine($"public {globalMeta}IAccessorNamedType AccessorType() => GeneratorAccessorType;");

                sb.AppendFormat(accessorTemp,
                    globalMeta,
                    $"{(typeSymbol.IsValueType ? $"{format}    this = ({typeSymbol.GetFullNameStandardFormat(typeClean: typeClean)})accessor;{format}" : default)}",
                    globalSystem,
                    globalString,
                    globalObject,
                    globalBoolean);
                sb.Append("}");

                if (!typeSymbol.ContainingNamespace.IsGlobalNamespace)
                {
                    sb.AppendFormat("{0}}}", format);
                }

                dict.Add($"{typeSymbol.GetFullName(GetFullNameOpt.Create(typeParameterBracketLeft: "[", typeParameterBracketRight: "]"))}.Gen", sb.ToString());
                sb.Clear();
            }

            return dict;
        }

        public static string GeneratorCode(AnalysisInfoModel analysisInfo, string assemblyName, ToCodeOpt opt, string[] usings = default)
        {
            var format = opt.StandardFormat ? Environment.NewLine : " ";
            //string typeClean(string type) => !opt.Global ? TypeNameClean(type, $"{assemblyName}.", "System.Collections.Generic.", "System.Collections.ObjectModel.") : $"{Global}{type}";// type;
            var usings2 = usings.Select(c => $"{c}.").Concat(new string[] { $"{assemblyName}." }).ToArray();
            string typeClean(string type, bool noGlobal = false) => !opt.Global ? TypeNameClean(type, usings2) : !noGlobal ? $"{GlobalConst.Global}{type}" : type;

            var globalSystem = opt.GetGlobalName(GlobalName.System);
            var globalMeta = opt.GetGlobalName(GlobalName.Business_SourceGenerator_Meta);
            var globalGeneric = opt.GetGlobalName(GlobalName.System_Collections_Generic);
            var globalObjectModel = opt.GetGlobalName(GlobalName.System_Collections_ObjectModel);
            var globalType = opt.GetGlobalName(GlobalName.System_Type);
            var globalObject = opt.GetGlobalName(GlobalName.System_Object);

            var makeGenericTypes = GeneratorType.GetMakeGenericTypes(analysisInfo);
            var makeGenerics = makeGenericTypes.SelectMany(c => c.Value);
            var makeGenericsKeys = makeGenericTypes.Keys.Select(c => c.GetFullNameStandardFormat()).Concat(makeGenericTypes.SelectMany(c => c.Value.Select(c2 => c2.GetFullNameStandardFormat()))).Distinct().ToList();
            //var types = GetTypes(analysisInfo);

            var sb = new System.Text.StringBuilder(null);
            var generatorTypes = new List<string>();

            #region all type

            var definitions = makeGenerics.Where(c => !c.TypeArguments.Any(c => !(c.TypeKind is TypeKind.TypeParameter)) && c.IsGenericType && !c.IsAbstract);

            //types = types.Where(c => c.Names.DeclaredStandard.Contains("MethodInvoke"));

            foreach (var info in analysisInfo.TypeSymbols)
            {
                var typeSymbol = info.Value.Declared as ITypeSymbol;

                if (Meta.Global.BusinessSourceGeneratorMeta == typeSymbol.GetFullName(GetFullNameOpt.Create(captureStyle: CaptureStyle.Prefix)))
                {
                    continue;
                }

                if (IsTypeParameter(typeSymbol))
                {
                    continue;
                }

                if (makeGenericsKeys.Contains(typeSymbol.GetFullNameOrig()))
                {
                    continue;
                }

                if (SpecialType.System_Void == typeSymbol.SpecialType || TypeKind.Delegate == typeSymbol.TypeKind || typeSymbol.IsAbstract)
                {
                    continue;
                }

                string key = default;

                var result = new List<string>();
                var noParameterConstructor = false;

                switch (typeSymbol.Kind)
                {
                    case SymbolKind.ArrayType:
                        key = SetConstructorArray(typeSymbol as IArrayTypeSymbol, result, typeClean, opt);
                        noParameterConstructor = true;
                        break;
                    case SymbolKind.PointerType:
                        break;
                    //case SymbolKind.DynamicType:
                    case SymbolKind.NamedType:
                        key = SetConstructor(typeSymbol as INamedTypeSymbol, result, out noParameterConstructor, typeClean, opt);
                        break;
                    default: continue;
                }

                sb.AppendLine($"#region {key}");
                sb.AppendFormat("[typeof({0})] = ", key);

                var definitions2 = definitions.Where(c =>
                {
                    var typeParameters = c.TypeParameters.First();

                    //class
                    if (typeParameters.HasReferenceTypeConstraint && !(typeSymbol.TypeKind is TypeKind.Class))
                    {
                        return false;
                    }

                    //struct
                    if (typeParameters.HasValueTypeConstraint && !(typeSymbol.TypeKind is TypeKind.Struct))
                    {
                        return false;
                    }

                    //new()
                    if (typeParameters.HasConstructorConstraint)
                    {
                        if (typeSymbol is INamedTypeSymbol namedType && !namedType.InstanceConstructors.Any(c => 0 == c.Parameters.Length))
                        {
                            return false;
                        }
                    }

                    //constraints
                    if (typeParameters.ConstraintTypes.Any())
                    {
                        var constraintTypes = typeParameters.ConstraintTypes.Select(c => c.GetFullName()).ToArray();

                        foreach (var item in constraintTypes)
                        {
                            if (!EqualsFullName(typeSymbol, item) && !typeSymbol.AllInterfaces.Any(c => EqualsFullName(c, item)))
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                });

                var makes = string.Join(", ", definitions2.Select(c =>
                {
                    var make = c.GetFullNameStandardFormat(GetFullNameOpt.Create(captureStyle: CaptureStyle.NoArgs), typeClean);
                    return $"[typeof({make}<>)] = typeof({make}<{key}>)";
                }));

                sb.AppendFormat(generatorTypeTemp,
                    globalMeta,
                    definitions2.Any() ? $"new {globalGeneric}Dictionary<{globalSystem}Type, {globalSystem}Type> {{ {makes} }}" : "default",
                    $"new {globalMeta}IMethodMeta[] {{ {string.Join(", ", result)} }}",
                    info.Value.IsDefinition ? "true" : "default", typeSymbol.IsValueType || noParameterConstructor ? "true" : "default", $"{globalMeta}TypeKind.{typeSymbol.TypeKind.GetName()}");

                sb.AppendLine();
                sb.AppendLine($"        #endregion");
                generatorTypes.Add(sb.ToString());

                sb.Clear();

                SetGenerics(definitions2, generatorTypes, typeClean, opt, typeSymbol);
            }

            #endregion

            if (!string.IsNullOrEmpty(assemblyName))
            {
                sb.AppendFormat("namespace {1}{0}", $"{format}{{{format}", assemblyName);
                sb.AppendFormat(iGeneratorTypeTemp,
                    opt.Global ? $"{GlobalConst.Global}{assemblyName}." : default,
                    generatorTypes.Any() ? $" {string.Join($"        , {format}        ", generatorTypes)} " : " ",
                    globalMeta,
                    globalSystem,
                    globalGeneric,
                    globalObjectModel,
                    globalType);
            }
            else
            {
                sb.AppendFormat(iGeneratorTypeTemp,
                    opt.Global ? GlobalConst.Global : default,
                    generatorTypes.Any() ? $" {string.Join(", ", generatorTypes)} " : " ",
                    globalMeta,
                    globalSystem,
                    globalGeneric,
                    globalObjectModel,
                    globalType);
            }

            if (!string.IsNullOrEmpty(assemblyName))
            {
                sb.Append($"{format}}}");
            }

            return sb.ToString();

            static void SetGenerics(IEnumerable<INamedTypeSymbol> makeGenerics, List<string> generatorTypes, Func<string, bool, string> typeClean, ToCodeOpt opt, params ITypeSymbol[] typeArgument)
            {
                var globalMeta = opt.GetGlobalName(GlobalName.Business_SourceGenerator_Meta);

                foreach (var makeGeneric in makeGenerics)
                {
                    if (makeGeneric.IsAbstract)
                    {
                        continue;
                    }

                    var sb = new System.Text.StringBuilder(null);
                    var result = new List<string>();
                    new Dictionary<Type, Type> { [typeof(int)] = typeof(int) };
                    var key = SetConstructor(makeGeneric, result, out bool noParameterConstructor, typeClean, opt, typeArgument);

                    sb.AppendLine($"#region {key}");
                    sb.AppendFormat("[typeof({0})] = ", key);

                    sb.AppendFormat(generatorTypeTemp,
                        globalMeta,
                        "default",
                        $"new {globalMeta}IMethodMeta[] {{ {string.Join(", ", result)} }}",
                        makeGeneric.GetSymbolInfo().IsDefinition ? "true" : "default", makeGeneric.IsValueType || noParameterConstructor ? "true" : "default", $"{globalMeta}TypeKind.{makeGeneric.TypeKind.GetName()}");

                    sb.AppendLine();
                    sb.AppendLine($"        #endregion");
                    generatorTypes.Add(sb.ToString());

                    sb.Clear();
                }
            }
        }

        static string SetConstructor(INamedTypeSymbol named, IList<string> result, out bool noParameterConstructor, Func<string, bool, string> typeClean, ToCodeOpt opt, params ITypeSymbol[] typeArgument)
        {
            noParameterConstructor = default;

            var typeArguments = named.SetTypeArguments(typeArgument);

            foreach (var constructor in named.InstanceConstructors)
            {
                if (Accessibility.Public != constructor.DeclaredAccessibility)
                {
                    continue;
                }

                if (constructor.Parameters.Any(c =>
                {
                    var typeFullName = c.Type.GetFullNameStandardFormat();
                    return c.Type.Kind is SymbolKind.PointerType || typeFullName.StartsWith("System.Span") || typeFullName.StartsWith("System.ReadOnlySpan");
                }))
                {
                    continue;
                }

                if (!constructor.Parameters.Any())
                {
                    noParameterConstructor = !constructor.Parameters.Any();
                }

                result.Add(GetConstructor(constructor, typeArguments, typeClean, opt));
            }

            return named.GetFullNameStandardFormat(GetFullNameOpt.Create(typeArguments: typeArguments), typeClean);
            //return (named.IsGenericType && SpecialType.System_Nullable_T != named.OriginalDefinition?.SpecialType) ? named.GetFullNameStandardFormat(GetFullNameOpt.Create(noNullableQuestionMark: true, typeArguments: typeArguments), typeClean) : named.GetFullNameStandardFormat(GetFullNameOpt.Create(noNullableQuestionMark: true), typeClean: typeClean);
        }

        static string SetConstructorArray(IArrayTypeSymbol array, IList<string> result, Func<string, bool, string> typeClean, ToCodeOpt opt)
        {
            var globalSystem = opt.GetGlobalName(GlobalName.System);
            var globalMeta = opt.GetGlobalName(GlobalName.Business_SourceGenerator_Meta);
            var globalInt32 = opt.GetGlobalName(GlobalName.System_Int32);
            var globalObject = opt.GetGlobalName(GlobalName.System_Object);
            var elementTypeClean = array.ElementType.GetFullNameStandardFormat(typeClean: typeClean);

            result.Add($"new {globalMeta}Constructor(0, 0, default, (obj, m, args) => {globalSystem}Array.Empty<{elementTypeClean}>())");

            result.Add($"new {globalMeta}Constructor(1, 1, new {globalMeta}IParameterMeta[] {{ {ToMetaParameter(globalMeta, "length", globalInt32)} }}, (obj, m, args) => new {elementTypeClean}[({globalInt32})m[0].v])");

            return array.GetFullNameStandardFormat(GetFullNameOpt.Create(noNullableQuestionMark: true), typeClean: typeClean);
        }

        #region Temp

        const string accessorTemp = @"public {5} AccessorSet({3} name, {4} value)
{{
    if (name is null)
    {{
        throw new {2}ArgumentNullException(nameof(name));
    }}

    if (!AccessorType().Members.TryGetValue(name, out {0}IAccessor meta) || !(meta is {0}IAccessorMember member) || member.SetValue is null)
    {{
        return default;
    }}

    {0}IGeneratorAccessor accessor = this;

    member.SetValue(ref accessor, value);
    {1}
    return true;
}}
";

        /*
        const string makeGenericTypeTemp = @"case {1}GeneratorTypeOpt.MakeGenericType:
                {{ 
                    if (arg.makeType is null) {{ throw new {2}ArgumentNullException(nameof(arg.makeType)); }} 
                    switch (arg.makeType) 
                    {{ 
                        {0}
                        default: return default; 
                    }} 
                }}
                ";

        const string createGenericTypeTemp = @"case {1}GeneratorTypeOpt.CreateGenericType:
                {{
                    switch (arg.createType)
                    {{ 
                        {0} 
                        default: return default; 
                    }} 
                }}
                ";

        const string constructorsTemp = @"case {1}GeneratorTypeOpt.Constructors:
                {{ 
                    if (arg.makeType is null) {{ throw new {2}ArgumentNullException(nameof(arg.makeType)); }} 
                    switch (arg.makeType) 
                    {{ 
                        {0}
                    }} 
                }}
                ";
        */

        const string generatorTypeTemp = @"new {0}GeneratorTypeMeta({1}, {2}, {3}, {4}, {5})";

        const string iGeneratorTypeTemp = @"public partial class BusinessSourceGenerator : {2}IGeneratorType
{{
    static readonly {3}Lazy<{2}IGeneratorType> generator = new {3}Lazy<{2}IGeneratorType>(() => new {0}BusinessSourceGenerator());

    public static {2}IGeneratorType Generator {{ get => generator.Value; }}

    static readonly {3}Lazy<{4}IReadOnlyDictionary<{6}, {2}GeneratorTypeMeta>> generatorType = new {3}Lazy<{4}IReadOnlyDictionary<{6}, {2}GeneratorTypeMeta>>(() => new {5}ReadOnlyDictionary<{6}, {2}GeneratorTypeMeta>(new {4}Dictionary<{6}, {2}GeneratorTypeMeta> 
    {{
       {1}
    }}));

    public {4}IReadOnlyDictionary<{6}, {2}GeneratorTypeMeta> GeneratorType {{ get => generatorType.Value; }}
}}";

        #endregion
    }
}