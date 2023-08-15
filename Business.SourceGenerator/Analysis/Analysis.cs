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
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
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

                        if (declared.TypeKind is TypeKind.Struct)
                        {
                            dict.Add(key, declared);
                        }
                        else
                        {
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

            if ((Accessibility.Public != typeSymbol.DeclaredAccessibility && SymbolKind.DynamicType != typeSymbol.Kind) || typeSymbol.IsStatic || typeSymbol.IsRefLikePointerTypedReferenceTypeParameter())
            {
                //if (SymbolKind.ArrayType != typeSymbol.Kind)
                //{
                //    return true;
                //}
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

        /*
        public static IDictionary<string, string> GeneratorAccessor(AnalysisInfoModel analysisInfo, string assemblyName, ToCodeOpt opt, string[] usings = default)
        {
            var format = opt.StandardFormat ? Environment.NewLine : " ";
            var usings2 = usings.Select(c => $"{c}.").Concat(new string[] { $"{assemblyName}." }).ToArray();
            string typeClean(string type, bool noGlobal = false) => !opt.Global ? TypeNameClean(type, usings2) : !noGlobal ? $"{GlobalConst.Global}{type}" : type;

            var declarations = analysisInfo.TypeSymbols.Values.Where(c => c.Names.AssemblyName.Equals(assemblyName) && c.IsCustom && (c.Syntax.IsKind(SyntaxKind.ClassDeclaration) || c.Syntax.IsKind(SyntaxKind.StructDeclaration)));

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

                dict.Add($"{typeSymbol.GetFullName(GetFullNameOpt.Create(charSpecial: CharSpecial.Create(bracketLeft: '[', bracketRight: ']')))}.Gen", sb.ToString());
                sb.Clear();
            }

            return dict;
        }
        */

        public static (string key, string code) GeneratorAccessor(ITypeSymbol typeSymbol, string assemblyName, string makes, bool noParameterConstructor, List<string> result, bool isCustom, ToCodeOpt opt, string[] usings = default)
        {
            var format = opt.StandardFormat ? Environment.NewLine : " ";
            var space = Meta.Global.Space;
            var usings2 = usings.Select(c => $"{c}.").Concat(new string[] { $"{assemblyName}." }).ToArray();
            string typeClean(string type, bool noGlobal = false) => !opt.Global ? TypeNameClean(type, usings2) : !noGlobal ? $"{GlobalConst.Global}{type}" : type;

            var globalSystem = opt.GetGlobalName(GlobalName.System);
            var globalMeta = opt.GetGlobalName(GlobalName.Business_SourceGenerator_Meta);
            var globalGeneric = opt.GetGlobalName(GlobalName.System_Collections_Generic);

            var sb = new System.Text.StringBuilder(null);

            var key = $"{typeSymbol.GetFullNameStandardFormat(GetFullNameOpt.Create(noNullableQuestionMark: true, charSpecial: CharSpecial.Create('_')))}__{typeSymbol.Kind.GetName()}_{typeSymbol.TypeKind.GetName()}_{typeSymbol.SpecialType.GetName()}";

            string key2 = default;

            int spaceCount = 0;

            if (!string.IsNullOrEmpty(assemblyName))
            {
                sb.AppendFormat("namespace {1}{0}", $"{format}{{{format}", assemblyName);
                spaceCount++;
            }

            sb.AppendFormat($"{{0}}public partial class {{2}} {{1}}{{0}}{{{{{{1}}", space.Repeat(spaceCount), format, Meta.Global.GeneratorCodeName);

            spaceCount++;

            if (typeSymbol.TypeKind is TypeKind.Class || typeSymbol.TypeKind is TypeKind.Struct || typeSymbol.TypeKind is TypeKind.Enum)
            {
                key2 = typeSymbol.ToMeta3(opt, typeClean, default, assemblyName);// $"{key}_Accessor";

                //sb.AppendFormat($"{space.Repeat(spaceCount)}static readonly {globalSystem}Lazy<{globalMeta}IAccessorType> {{0}} = new {globalSystem}Lazy<{globalMeta}IAccessorType>(() => {{1}});", key2, typeSymbol.ToMeta3(opt, typeClean, default));

                //if (typeSymbol is INamedTypeSymbol named)
                //{
                //    var dd = named.ToMeta3(opt, typeClean, default);
                //    //var ss = typeSymbol.ToMeta2(opt, typeClean: typeClean);
                //}

                //sb.AppendLine(format);
            }

            //if (string.IsNullOrEmpty(key2))
            //{

            //}

            sb.AppendFormat($"{space.Repeat(spaceCount)}readonly static {globalSystem}Lazy<{globalMeta}TypeMeta> {{0}} = new {globalSystem}Lazy<{globalMeta}TypeMeta>(() => new {globalMeta}TypeMeta({{1}}, {{2}}, {{3}}, {{4}}, {{5}}, {{6}}));",
                key,
                !string.IsNullOrEmpty(makes) ? $"new {globalGeneric}Dictionary<{globalSystem}Type, {globalSystem}Type> {{ {makes} }}" : "default",
                result.Any() ? $"new {globalMeta}IMethod[] {{ {string.Join(", ", result)} }}" : $"{globalSystem}Array.Empty<{globalMeta}IMethod>()",
                isCustom ? "true" : "default",
                typeSymbol.IsValueType || noParameterConstructor ? "true" : "default", $"{globalMeta}TypeKind.{typeSymbol.TypeKind.GetName()}",
                !string.IsNullOrEmpty(key2) ? key2 : "default");

            spaceCount--;

            sb.AppendFormat("{0}{1}}}", format, space.Repeat(spaceCount));

            if (!string.IsNullOrEmpty(assemblyName))
            {
                sb.Append($"{format}}}");
            }

            //return (typeSymbol.GetFullNameStandardFormat(GetFullNameOpt.Create(noNullableQuestionMark: true, charSpecial: CharSpecial.Create(bracketLeft: '[', bracketRight: ']', asterisk: '_'))), key, sb.ToString());
            return (key, sb.ToString());
        }

        public static (string types, IEnumerable<string> code) GeneratorCode(AnalysisInfoModel analysisInfo, string assemblyName, ToCodeOpt opt, string[] usings = default)
        {
            var format = opt.StandardFormat ? Environment.NewLine : " ";
            var space = Meta.Global.Space;
            var space_x3 = space.Repeat(3);
            var usings2 = usings.Select(c => $"{c}.").Concat(new string[] { $"{assemblyName}." }).ToArray();
            string typeClean(string type, bool noGlobal = false) => !opt.Global ? TypeNameClean(type, usings2) : !noGlobal ? $"{GlobalConst.Global}{type}" : type;

            var global = opt.GetGlobalName(GlobalName.Globa);
            var globalSystem = opt.GetGlobalName(GlobalName.System);
            var globalMeta = opt.GetGlobalName(GlobalName.Business_SourceGenerator_Meta);
            var globalGeneric = opt.GetGlobalName(GlobalName.System_Collections_Generic);
            var globalObjectModel = opt.GetGlobalName(GlobalName.System_Collections_ObjectModel);
            var globalType = opt.GetGlobalName(GlobalName.System_Type);

            var makeGenericTypes = GeneratorType.GetMakeGenericTypes(analysisInfo);
            var makeGenerics = makeGenericTypes.SelectMany(c => c.Value);
            var makeGenericsKeys = makeGenericTypes.Keys.Select(c => c.GetFullNameStandardFormat()).Concat(makeGenericTypes.SelectMany(c => c.Value.Select(c2 => c2.GetFullNameStandardFormat()))).Distinct().ToList();
            //var types = GetTypes(analysisInfo);

            var sb = new System.Text.StringBuilder(null);
            var generators = new Dictionary<string, (string types, (string key, string code) accessors)>();

            #region all type

            var definitions = makeGenerics.Where(c => !c.TypeArguments.Any(c => !(c.TypeKind is TypeKind.TypeParameter)) && c.IsGenericType && !c.IsAbstract);

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

                //if (makeGenericsKeys.Contains(typeSymbol.GetFullNameOrig()))
                if (makeGenericsKeys.Contains(info.Value.Names.DeclaredStandard))
                {
                    continue;
                }

                if (typeSymbol.SpecialType is SpecialType.System_Void || typeSymbol.TypeKind is TypeKind.Delegate || typeSymbol.IsAbstract)
                {
                    continue;
                }

                string key = default;
                var result = new List<string>();
                var noParameterConstructor = false;

                switch (typeSymbol.Kind)
                {
                    case SymbolKind.ArrayType:
                        key = SetConstructorArray(typeSymbol as IArrayTypeSymbol, result, typeClean, opt, assemblyName);
                        noParameterConstructor = true;
                        break;
                    case SymbolKind.PointerType:
                        break;
                    //case SymbolKind.DynamicType:
                    case SymbolKind.NamedType:
                        key = SetConstructor(typeSymbol as INamedTypeSymbol, result, out noParameterConstructor, typeClean, opt, assemblyName);
                        break;
                    default: continue;
                }

                var definitions2 = definitions.Where(c => CheckConstraint(typeSymbol, c.TypeParameters.First()));

                var makes = string.Join(", ", definitions2.Select(c =>
                {
                    var make = c.GetFullNameStandardFormat(GetFullNameOpt.Create(captureStyle: CaptureStyle.NoArgs), typeClean);
                    return $"[typeof({make}<>)] = typeof({make}<{key}>)";
                }));

                var accessor = GeneratorAccessor(typeSymbol, assemblyName, makes, noParameterConstructor, result, info.Value.IsCustom, opt, usings);

                sb.AppendLine($"{space_x3}#region {key}");
                sb.AppendLine($"{space_x3}[typeof({key})] = {accessor.key}?.Value ?? default");
                sb.Append($"{space_x3}#endregion");

                if (generators.ContainsKey(key))
                {
                    generators.Remove(key);
                    generators.Add(key, (sb.ToString(), accessor));
                }
                else
                {
                    generators.Add(key, (sb.ToString(), accessor));
                }

                result.Clear();
                sb.Clear();

                foreach (var item in definitions2)
                {
                    var makeGeneric = item.ConstructedFrom.Construct(typeSymbol);

                    key = SetConstructor(makeGeneric, result, out noParameterConstructor, typeClean, opt, assemblyName);

                    accessor = GeneratorAccessor(makeGeneric, assemblyName, default, noParameterConstructor, result, item.GetSymbolInfo().IsCustom, opt, usings);

                    if (!generators.ContainsKey(key))
                    {
                        sb.AppendLine($"{space_x3}#region {key}");
                        sb.AppendLine($"{space_x3}[typeof({key})] = {accessor.key}?.Value ?? default");
                        sb.Append($"{space_x3}#endregion");

                        generators.Add(key, (sb.ToString(), accessor));

                        sb.Clear();
                    }

                    result.Clear();
                }

                //if (generators.Count > 0)
                //{
                //    //break;
                //}
            }

            #endregion

            #region Temp

            int spaceCount = 0;

            var globalAssemblyName = global;
            if (!string.IsNullOrEmpty(assemblyName))
            {
                globalAssemblyName = $"{global}{assemblyName}.";
                sb.AppendFormat("namespace {1}{0}", $"{format}{{{format}", assemblyName);
                spaceCount++;
            }

            sb.SetSummary(space.Repeat(spaceCount), Meta.Global.GeneratorCodeName);
            sb.AppendFormat("{0}public partial class {2} : {3}IGeneratorCode {1}{0}{{{1}", space.Repeat(spaceCount), format, Meta.Global.GeneratorCodeName, globalMeta);

            spaceCount++;

            sb.AppendFormat("{0}readonly static {1}Lazy<{2}{3}> generator = new {1}Lazy<{2}{3}>(() => new {2}{3}());", space.Repeat(spaceCount), globalSystem, globalAssemblyName, Meta.Global.GeneratorCodeName);
            sb.AppendLine(format);
            sb.SetSummary(space.Repeat(spaceCount), "Generator");
            sb.AppendFormat("{0}public static {1}IGeneratorCode Generator {{ get {{ return generator.Value; }} }}", space.Repeat(spaceCount), globalMeta);
            sb.AppendLine(format);

            sb.AppendFormat("{0}readonly static {2}Lazy<{3}IReadOnlyDictionary<{5}, {6}TypeMeta>> generatorType = new {2}Lazy<{3}IReadOnlyDictionary<{5}, {6}TypeMeta>>(() => new {4}ReadOnlyDictionary<{5}, {6}TypeMeta>(new {3}Dictionary<{5}, {6}TypeMeta>{1}{0}{{{1}{7}{1}{0}}}));", space.Repeat(spaceCount), format, globalSystem, globalGeneric, globalObjectModel, globalType, globalMeta, generators.Any() ? $"{string.Join($"{format}{space_x3},{format}", generators.Values.Select(c => c.types))}" : default);

            sb.AppendLine(format);

            sb.AppendFormat("{0}static {1}IReadOnlyDictionary<{2}, {3}TypeMeta> Singleton {{ get {{ return generatorType.Value; }} }}", space.Repeat(spaceCount), globalGeneric, globalType, globalMeta);
            sb.AppendLine(format);
            sb.SetSummary(space.Repeat(spaceCount), "GeneratorType");
            sb.AppendFormat("{0}public {1}IReadOnlyDictionary<{2}, {3}TypeMeta> GeneratorType {{ get {{ return Singleton; }} }}", space.Repeat(spaceCount), globalGeneric, globalType, globalMeta);

            spaceCount--;

            sb.AppendFormat("{0}{1}}}", format, space.Repeat(spaceCount));

            if (!string.IsNullOrEmpty(assemblyName))
            {
                sb.Append($"{format}}}");
            }

            #endregion

            return (sb.ToString(), generators.Values.Select(c => c.accessors.code));

            /*
            static Dictionary<string, string> SetGenerics(IEnumerable<INamedTypeSymbol> makeGenerics, Func<string, bool, string> typeClean, ToCodeOpt opt, params ITypeSymbol[] typeArgument)
            {
                var globalMeta = opt.GetGlobalName(GlobalName.Business_SourceGenerator_Meta);

                var types = new Dictionary<string, string>();

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

                    //var accessor = GeneratorAccessor2(makeGeneric, assemblyName, makes, noParameterConstructor, result, info.Value.IsCustom, opt, usings);

                    sb.AppendLine($"#region {key}");
                    sb.AppendFormat("[typeof({0})] = ", key);

                    //sb.AppendFormat(generatorTypeTemp,
                    //    globalMeta,
                    //    "default",
                    //    $"new {globalMeta}IMethodMeta[] {{ {string.Join(", ", result)} }}",
                    //    makeGeneric.GetSymbolInfo().IsCustom ? "true" : "default",
                    //    makeGeneric.IsValueType || noParameterConstructor ? "true" : "default", $"{globalMeta}TypeKind.{makeGeneric.TypeKind.GetName()}",
                    //    "default");

                    sb.AppendLine();
                    sb.AppendLine($"        #endregion");
                    //generatorTypes.Add(sb.ToString());

                    types.Add(key, sb.ToString());

                    sb.Clear();
                }

                return types;
            }
            */
        }

        static string SetConstructor(INamedTypeSymbol named, IList<string> result, out bool noParameterConstructor, Func<string, bool, string> typeClean, ToCodeOpt opt, string assemblyName, params ITypeSymbol[] typeArgument)
        {
            noParameterConstructor = default;

            var typeArguments = named.SetTypeArguments(typeArgument);

            foreach (var constructor in named.InstanceConstructors)
            {
                if (constructor.IsObsolete() || Accessibility.Public != constructor.DeclaredAccessibility)
                {
                    continue;
                }

                if (constructor.Parameters.Any(c => c.Type.IsRefLikePointerTypedReferenceValueTypeConstraint()))
                {
                    continue;
                }

                //if (constructor.Parameters.Any(c =>
                //{
                //    var typeFullName = c.Type.GetFullNameStandardFormat();
                //    return c.Type.Kind is SymbolKind.PointerType || typeFullName.StartsWith("System.Span") || typeFullName.StartsWith("System.ReadOnlySpan");
                //}))
                //{
                //    continue;
                //}

                if (!constructor.Parameters.Any())
                {
                    noParameterConstructor = !constructor.Parameters.Any();
                }

                result.Add(GetConstructor(constructor, typeArguments, typeClean, opt, assemblyName));
            }

            return named.GetFullNameStandardFormat(GetFullNameOpt.Create(typeArguments: typeArguments), typeClean);
            //return (named.IsGenericType && SpecialType.System_Nullable_T != named.OriginalDefinition?.SpecialType) ? named.GetFullNameStandardFormat(GetFullNameOpt.Create(noNullableQuestionMark: true, typeArguments: typeArguments), typeClean) : named.GetFullNameStandardFormat(GetFullNameOpt.Create(noNullableQuestionMark: true), typeClean: typeClean);
        }

        static string SetConstructorArray(IArrayTypeSymbol array, IList<string> result, Func<string, bool, string> typeClean, ToCodeOpt opt, string assemblyName)
        {
            var globalSystem = opt.GetGlobalName(GlobalName.System);
            var globalMeta = opt.GetGlobalName(GlobalName.Business_SourceGenerator_Meta);
            var globalInt32 = opt.GetGlobalName(GlobalName.System_Int32);
            var elementTypeClean = array.ElementType.GetFullNameStandardFormat(typeClean: typeClean);

            var type = $"{opt.GetGlobalName(GlobalName.Globa)}{assemblyName}.AccessorTypes.System_Int32.Singleton";
            //var type = $"{globalMeta}Types.{nameof(Meta.Types.Int32Type)}.Singleton";
            //var type = AnalysisInfo.BasrType.TryGetValue("System.Int32", out string v) ? v : "default";
            var p = $"new {globalMeta}AccessorParameter({type})";

            result.Add($"new {globalMeta}Constructor(default, 0, 0, (obj, m, args) => {globalSystem}Array.Empty<{elementTypeClean}>())");
            result.Add($"new {globalMeta}Constructor(new {globalMeta}IAccessorParameter[] {{ {p} }}, 1, 1, (obj, m, args) => new {elementTypeClean}[({globalInt32})m[0].v])");

            return array.GetFullNameStandardFormat(GetFullNameOpt.Create(noNullableQuestionMark: true), typeClean: typeClean);
        }

        static bool CheckConstraint(ITypeSymbol typeSymbol, ITypeParameterSymbol typeParameter)
        {
            //class
            if (typeParameter.HasReferenceTypeConstraint && !(typeSymbol.TypeKind is TypeKind.Class))
            {
                return false;
            }

            //struct
            if (typeParameter.HasValueTypeConstraint && !(typeSymbol.TypeKind is TypeKind.Struct))
            {
                return false;
            }

            //new()
            if (typeParameter.HasConstructorConstraint)
            {
                if (typeSymbol is INamedTypeSymbol namedType && !namedType.InstanceConstructors.Any(c => 0 == c.Parameters.Length && !c.IsObsolete()))
                {
                    return false;
                }
            }

            //constraints
            if (typeParameter.ConstraintTypes.Any())
            {
                var constraintTypes = typeParameter.ConstraintTypes.Select(c => c.GetFullName()).ToArray();

                foreach (var item in constraintTypes)
                {
                    if (!EqualsFullName(typeSymbol, item) && !typeSymbol.AllInterfaces.Any(c => EqualsFullName(c, item)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #region Temp

        const string accessorTemp = @"public {5} TryAccessorSet({3} name, {4} value)
{{
    if (name is null)
    {{
        throw new {2}ArgumentNullException(nameof(name));
    }}

    if (!AccessorType().Members.TryGetValue(name, out {0}IAccessor meta) || !(meta is {0}IAccessorMember member) || member.SetValue is null)
    {{
        return default;
    }}

    {4} accessor = this;

    member.SetValue(ref accessor, value);
    {1}
    return true;
}}
";

        #endregion
    }
}