using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.SourceGenerator.UnitTest
{
    internal static class FullName
    {
        public readonly struct GetFullNameOpt
        {
            public GetFullNameOpt(bool noArgs = false, bool args = false, bool standardFormat = false, bool unboundGenericType = false, bool parameters = false, bool noPrefix = false, bool prefix = false, bool isGenericArg = false)
            {
                NoArgs = noArgs;
                Args = args;
                StandardFormat = standardFormat;
                UnboundGenericType = unboundGenericType;
                Parameters = parameters;
                NoPrefix = noPrefix;
                Prefix = prefix;
                IsGenericArg = isGenericArg;
            }

            public bool NoArgs { get; }

            public bool Args { get; }

            public bool StandardFormat { get; }

            public bool UnboundGenericType { get; }

            public bool Parameters { get; }

            public bool NoPrefix { get; }

            public bool Prefix { get; }

            public bool IsGenericArg { get; }
        }

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
                nullable = opt.StandardFormat && !opt.IsGenericArg ? string.Empty : "?";

                if ("System.Nullable".Equals($"{prefix2}{symbol.Name}") && 0 < named?.TypeArguments.Length)
                {
                    return $"{GetFullName(named.TypeArguments[0], opt, typeClean2)}{nullable}";
                }

                if (null == named)
                {
                    return $"{GetFullName(symbol.OriginalDefinition, opt, typeClean2)}{nullable}";
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
                        args = $"<{string.Join(", ", named.TupleElements.Select(c => $"{(TypeKind.Dynamic == c.Type.TypeKind && opt.StandardFormat ? objectNameClean : TypeKind.TypeParameter == c.Type.TypeKind ? c.Type.Name : GetFullName(c.Type, new GetFullNameOpt(opt.NoArgs, opt.Args, opt.StandardFormat, opt.UnboundGenericType, opt.Parameters, opt.NoPrefix, opt.Prefix, true), typeClean2))}"))}>";
                    }
                }
                else
                {
                    args = opt.UnboundGenericType ? $"<{string.Join(",", Enumerable.Repeat(string.Empty, named.TypeArguments.Length))}>" : $"<{string.Join(", ", named.TypeArguments.Select(c => (TypeKind.Dynamic == c.TypeKind && opt.StandardFormat ? objectNameClean : TypeKind.TypeParameter == c.TypeKind ? c.Name : GetFullName(c, new GetFullNameOpt(opt.NoArgs, opt.Args, opt.StandardFormat, opt.UnboundGenericType, opt.Parameters, opt.NoPrefix, opt.Prefix, true), typeClean2))))}>";
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
                    args = opt.UnboundGenericType ? $"<{string.Join(",", Enumerable.Repeat(string.Empty, named.TypeArguments.Length))}>" : $"<{string.Join(", ", method.TypeArguments.Select(c => (TypeKind.Dynamic == c.TypeKind && opt.StandardFormat ? objectNameClean : TypeKind.TypeParameter == c.TypeKind ? c.Name : GetFullName(c, new GetFullNameOpt(opt.NoArgs, opt.Args, opt.StandardFormat, opt.UnboundGenericType, opt.Parameters, opt.NoPrefix, opt.Prefix, true), typeClean2))))}>";
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
    }
}
