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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static Business.SourceGenerator.Analysis.SyntaxToCode;

    public static class SymbolTypeName
    {
        #region Symbol

        readonly static Dictionary<string, string> systemTypeKeywords = new Dictionary<string, string>
        {
            ["global::System.Char"] = "char",
            ["global::System.String"] = "string",
            ["global::System.Boolean"] = "bool",
            ["global::System.SByte"] = "sbyte",
            ["global::System.Byte"] = "byte",
            ["global::System.Decimal"] = "decimal",
            ["global::System.Double"] = "double",
            ["global::System.Single"] = "float",
            ["global::System.Int16"] = "short",
            ["global::System.Int32"] = "int",
            ["global::System.Int64"] = "long",
            ["global::System.UInt16"] = "ushort",
            ["global::System.UInt32"] = "uint",
            ["global::System.UInt64"] = "ulong",
            ["global::System.Object"] = "object",

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
                if (skip.Contains($"{value}."))
                {
                    return string.Empty;
                }
                /*
                foreach (var item in skip)
                {
                    if (value.StartsWith(item))
                    {
                        var value2 = value.Substring(item.Length);

                        //value = value.Substring(item.Length);
                        //var value2 = value;

                        var brackLeft = value2.IndexOf('<');
                        var brackRight = value2.LastIndexOf('>');

                        while (-1 != brackLeft && -1 != brackRight)
                        {
                            value2 = value2.Substring(0, brackLeft) + value2.Substring(brackRight + 1, value2.Length - brackRight - 1);
                            brackLeft = value2.IndexOf('<');
                            brackRight = value2.LastIndexOf('>');
                        }

                        //var sp = value2.Split('.');
                        if (!value2.Contains('.'))
                        {
                            value = value.Substring(item.Length);
                            break;
                        }
                    }
                }
                */
            }

            const string System = "System.";

            if (value.StartsWith(System) && 2 == value.Split('<')[0].Split('.').Length)
            {
                value = value.Substring(System.Length);
            }

            return value;
        }

        public readonly struct GetFullNameOpt
        {
            public static GetFullNameOpt Create() => new GetFullNameOpt(false);

            public static GetFullNameOpt Create(bool standardFormat = false, bool noNullableQuestionMark = false, AnonymousTypeStyle anonymousTypeStyle = AnonymousTypeStyle.Curly, CaptureStyle captureStyle = CaptureStyle.Pass, ParameterStyle parameterStyle = ParameterStyle.Type, TypeParameterStyle typeParameterStyle = TypeParameterStyle.FullName, string methodParameterVarName = default, string methodParameterRefVarName = default, Func<string, int, string> methodParameterVarNameCallback = default, IDictionary<string, ITypeSymbol> typeArguments = default, bool global = default, string typeParameterBracketLeft = default, string typeParameterBracketRight = default) => new GetFullNameOpt(standardFormat, noNullableQuestionMark, anonymousTypeStyle, captureStyle, parameterStyle, typeParameterStyle, methodParameterVarName, methodParameterRefVarName, methodParameterVarNameCallback, typeArguments, global, typeParameterBracketLeft, typeParameterBracketRight);

            GetFullNameOpt(bool standardFormat = false, bool noNullableQuestionMark = false, AnonymousTypeStyle anonymousTypeStyle = AnonymousTypeStyle.Curly, CaptureStyle captureStyle = CaptureStyle.Pass, ParameterStyle parameterStyle = ParameterStyle.Type, TypeParameterStyle typeParameterStyle = TypeParameterStyle.FullName, string methodParameterVarName = default, string methodParameterRefVarName = default, Func<string, int, string> methodParameterVarNameCallback = default, IDictionary<string, ITypeSymbol> typeArguments = default, bool global = default, string typeParameterBracketLeft = default, string typeParameterBracketRight = default)
            {
                StandardFormat = standardFormat;
                NoNullableQuestionMark = noNullableQuestionMark;
                AnonymousTypeStyle = anonymousTypeStyle;
                CaptureStyle = captureStyle;
                ParameterStyle = parameterStyle;
                TypeParameterStyle = typeParameterStyle;
                MethodParameterVarName = methodParameterVarName ?? "args";
                MethodParameterRefVarName = methodParameterRefVarName ?? "args";
                MethodParameterVarNameCallback = methodParameterVarNameCallback;
                TypeArguments = typeArguments;
                Global = global;
                TypeParameterBracketLeft = typeParameterBracketLeft ?? "<";
                TypeParameterBracketRight = typeParameterBracketRight ?? ">";
            }

            public GetFullNameOpt Set(bool? standardFormat = default, bool? noNullableQuestionMark = default, AnonymousTypeStyle? anonymousTypeStyle = default, CaptureStyle? captureStyle = default, ParameterStyle? parameterStyle = default, TypeParameterStyle? typeParameterStyle = default, string methodParameterVarName = default, string methodParameterRefVarName = default, Func<string, int, string> methodParameterVarNameCallback = default, IDictionary<string, ITypeSymbol> typeArguments = default, bool? global = default, string typeParameterBracketLeft = default, string typeParameterBracketRight = default) => Create(standardFormat ?? StandardFormat, noNullableQuestionMark ?? NoNullableQuestionMark, anonymousTypeStyle ?? AnonymousTypeStyle, captureStyle ?? CaptureStyle, parameterStyle ?? ParameterStyle, typeParameterStyle ?? TypeParameterStyle, methodParameterVarName ?? MethodParameterVarName, methodParameterRefVarName ?? MethodParameterRefVarName, methodParameterVarNameCallback ?? MethodParameterVarNameCallback, typeArguments ?? TypeArguments, global ?? Global, TypeParameterBracketLeft ?? typeParameterBracketLeft, TypeParameterBracketRight ?? typeParameterBracketRight);

            public bool StandardFormat { get; }

            public bool NoNullableQuestionMark { get; }

            public AnonymousTypeStyle AnonymousTypeStyle { get; }

            public CaptureStyle CaptureStyle { get; }

            public ParameterStyle ParameterStyle { get; }

            internal TypeParameterStyle TypeParameterStyle { get; }

            internal string MethodParameterVarName { get; }

            internal string MethodParameterRefVarName { get; }

            internal Func<string, int, string> MethodParameterVarNameCallback { get; }

            internal IDictionary<string, ITypeSymbol> TypeArguments { get; }

            internal bool Global { get; }

            internal string TypeParameterBracketLeft { get; }

            internal string TypeParameterBracketRight { get; }
        }

        static bool In(this AnonymousTypeStyle value, AnonymousTypeStyle flag) => 0 != (value & flag);
        static bool In(this CaptureStyle value, CaptureStyle flag) => 0 != (value & flag);

        /// <summary>
        /// AnonymousTypeStyle
        /// </summary>
        [Flags]
        public enum AnonymousTypeStyle
        {
            Round = 2,

            Curly = 4,

            Type = 8,

            Name = 16,
        }

        /// <summary>
        /// CaptureStyle
        /// </summary>
        [Flags]
        public enum CaptureStyle
        {
            /// <summary>
            /// CaptureStyle.Pass is the default value and will not be processed.
            /// </summary>
            Pass = 2,

            /// <summary>
            /// Returns the prefix of the symbol, which can be switched with CaptureStyle.Name.
            /// </summary>
            Prefix = 4,

            /// <summary>
            /// Returns the simple name of the symbol, which can be switched CaptureStyle.Prefix.
            /// </summary>
            Name = 8,

            /// <summary>
            /// Returns the string representation of all parameters of the method, with ().
            /// </summary>
            MethodParameters = 16,

            /// <summary>
            /// Used with TypeParameterStyle.FullName.
            /// </summary>
            MethodName = 32,

            /// <summary>
            /// (Use only once!) Returns a name without a symbol prefix.
            /// </summary>
            NoPrefix = 64,

            /// <summary>
            /// (Use only once!) Returns the prefix of an unsigned name.
            /// </summary>
            NoName = 128,

            /// <summary>
            /// Returns the type parameter of a generic type or method.
            /// <para>Only process the first layer CaptureStyle.Args, so, record and remove CaptureStyle.Args, Wait for subsequent INamedTypeSymbol processing.</para>
            /// </summary>
            Args = 256,

            /// <summary>
            /// Returns the type signature without type parameters.
            /// <para>Only process the first layer CaptureStyle.NoArgs.</para>
            /// </summary>
            NoArgs = 512
        }

        /// <summary>
        /// ParameterStyle
        /// </summary>
        public enum ParameterStyle
        {
            Type,

            Name,

            TypeName
        }

        /// <summary>
        /// Internal use only.
        /// </summary>
        public enum TypeParameterStyle : byte
        {
            /// <summary>
            /// Full generic type parameter string representation.
            /// <para>Invalid for type parameters in generic type definition and generic method definition.</para>
            /// </summary>
            FullName = 1,

            /// <summary>
            /// Simple generic type parameter string representation.
            /// <para>Invalid for type parameters in generic type definition and generic method definition.</para>
            /// </summary>
            Name = 2,

            /// <summary>
            /// Shows the current generic constraint type.
            /// </summary>
            Real = 3,

            /// <summary>
            /// MethodReal.
            /// </summary>
            MethodReal = 4
        }

        //internal const TypeParameterStyle MethodReal = TypeParameterStyle.Real + 1;

        public static string GetFullNameRealStandardFormat(this ISymbol symbol, GetFullNameOpt opt = default, Func<string, bool, string> typeClean = default)
        {
            if (opt.Equals(default(GetFullNameOpt)))
            {
                opt = GetFullNameOpt.Create();
            }

            return GetFullName(symbol, opt.Set(true, typeParameterStyle: TypeParameterStyle.Real), typeClean);
        }

        public static string GetFullNameMethodRealStandardFormat(this ISymbol symbol, GetFullNameOpt opt = default, Func<string, bool, string> typeClean = default)
        {
            if (opt.Equals(default(GetFullNameOpt)))
            {
                opt = GetFullNameOpt.Create();
            }

            return GetFullName(symbol, opt.Set(true, typeParameterStyle: TypeParameterStyle.MethodReal), typeClean);
        }

        public static string GetFullNameStandardFormat(this ISymbol symbol, GetFullNameOpt opt = default, Func<string, bool, string> typeClean = default)
        {
            if (opt.Equals(default(GetFullNameOpt)))
            {
                opt = GetFullNameOpt.Create();
            }

            return GetFullName(symbol, opt.Set(true, typeParameterStyle: TypeParameterStyle.Name), typeClean);
        }

        /// <summary>
        /// Complete the string representation of all ISymbol in a recursion.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="opt"></param>
        /// <param name="typeClean"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string GetFullName(this ISymbol symbol, GetFullNameOpt opt = default, Func<string, bool, string> typeClean = default)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            if (opt.Equals(default(GetFullNameOpt)))
            {
                opt = GetFullNameOpt.Create();
            }

            var real = opt.TypeParameterStyle is TypeParameterStyle.Real || opt.TypeParameterStyle is TypeParameterStyle.MethodReal;

            #region (Use only once!) Only valid for the current symbol, without prefix and subsequent processing.

            var captureStyleNoPrefix = opt.CaptureStyle.In(CaptureStyle.NoPrefix);
            var captureStyleNoName = opt.CaptureStyle.In(CaptureStyle.NoName);
            var captureStyleNoArgs = opt.CaptureStyle.In(CaptureStyle.NoArgs);

            if (captureStyleNoPrefix)
            {
                opt = opt.Set(captureStyle: opt.CaptureStyle ^ CaptureStyle.NoPrefix);
            }

            if (captureStyleNoName)
            {
                opt = opt.Set(captureStyle: opt.CaptureStyle ^ CaptureStyle.NoName);
            }

            if (captureStyleNoArgs)
            {
                opt = opt.Set(captureStyle: opt.CaptureStyle ^ CaptureStyle.NoArgs);
            }

            #region opt.CaptureStyle.Args

            /* Only process the first layer CaptureStyle.Args, so,
            record and remove CaptureStyle.Args, Wait for subsequent INamedTypeSymbol processing. */
            var captureStyleArgs = opt.CaptureStyle.In(CaptureStyle.Args);

            if (captureStyleArgs)
            {
                opt = opt.Set(captureStyle: opt.CaptureStyle ^ CaptureStyle.Args);
            }

            #endregion

            #region checked Null

            var nullable = symbol is ITypeSymbol typeSymbol && typeSymbol.NullableAnnotation is NullableAnnotation.Annotated;

            #endregion

            #endregion

            var objectNameClean = typeClean?.Invoke(GlobalConst.System_Object, captureStyleNoPrefix) ?? GlobalConst.System_Object;

            #region pretreatment

            //Return simple string representation of generic type parameters in advance and uniformly to avoid infinite recursion.
            if (symbol is ITypeParameterSymbol typeParameter2)// && opt.TypeParameterStyle is TypeParameterStyle.Name)
            {
                switch (opt.TypeParameterStyle)
                {
                    case TypeParameterStyle.Name:
                        if (nullable)
                        {
                            if (!opt.NoNullableQuestionMark)
                            {
                                return $"{symbol.Name}?";
                            }

                            var nullName = $"System.Nullable<{symbol.Name}>";
                            return typeClean?.Invoke(nullName, captureStyleNoPrefix) ?? nullName;
                        }
                        return symbol.Name;
                    case TypeParameterStyle.Real:
                        {
                            var constraintType = TypeParameterSymbolRealType(typeParameter2);
                            return constraintType is null ? objectNameClean : GetFullName(constraintType, opt, typeClean);
                            //return TypeParameterSymbolRealName(typeParameter2, opt, typeClean);
                        }
                    case TypeParameterStyle.MethodReal:
                        {
                            if (typeParameter2.TypeParameterKind is TypeParameterKind.Type)
                            {
                                return typeParameter2.Name;
                            }

                            var constraintType = TypeParameterSymbolRealType(typeParameter2);
                            return constraintType is null ? objectNameClean : GetFullName(constraintType, opt, typeClean);
                            //return TypeParameterSymbolRealName(typeParameter2, opt, typeClean);
                        }
                    default: break;
                }
            }

            //string typeClean2(string name)
            //{
            //    return typeClean?.Invoke(name) ?? name;

            //    //return $"{global}{name2}";
            //}



            if (SymbolKind.DynamicType == symbol.Kind && opt.StandardFormat)
            {
                return objectNameClean;
            }

            #endregion

            var prefix = string.Empty;

            if (null != symbol.ContainingType)
            {
                var typeParameterStyle = opt.TypeParameterStyle;

                if (!real)
                {
                    typeParameterStyle = TypeParameterStyle.Name;
                }

                if (symbol.ContainingSymbol is IMethodSymbol)
                {
                    //Handle the generic parameters of the method in advance to avoid infinite recursion.
                    prefix = GetFullName(symbol.ContainingSymbol, opt.Set(typeParameterStyle: typeParameterStyle, captureStyle: opt.CaptureStyle | CaptureStyle.MethodName), typeClean);
                }
                else
                {
                    prefix = GetFullName(symbol.ContainingSymbol, opt.Set(typeParameterStyle: typeParameterStyle), typeClean);
                }
            }
            else if (null != symbol.ContainingNamespace && !symbol.ContainingNamespace.IsGlobalNamespace)
            {
                //prefix = symbol.ContainingNamespace.ToDisplayString();
                prefix = typeClean?.Invoke(symbol.ContainingNamespace.ToDisplayString(), captureStyleNoPrefix) ?? symbol.ContainingNamespace.ToDisplayString();
            }

            var symbolName = typeClean?.Invoke(symbol.Name, true) ?? symbol.Name;

            if (symbol is IMethodSymbol methodSymbol)
            {
                switch (methodSymbol.MethodKind)
                {
                    case MethodKind.AnonymousFunction:
                        symbolName = methodSymbol.Locations.FirstOrDefault()?.SourceSpan.ToString();
                        break;
                    case MethodKind.Constructor:
                    case MethodKind.StaticConstructor:
                        //The constructor is simply represented as prefix.ContainingSymbol_Name<args>().
                        symbolName = symbol.ContainingSymbol.Name;
                        break;
                    default: break;
                }
            }

            var prefixFullName = !string.IsNullOrEmpty(prefix) ? $"{prefix}.{symbolName}" : typeClean?.Invoke(symbolName, captureStyleNoPrefix) ?? symbolName;

            //CaptureStyle.Pass is the default value and will not be processed.
            if (!opt.CaptureStyle.In(CaptureStyle.Pass))
            {
                if (opt.CaptureStyle.In(CaptureStyle.Prefix) && opt.CaptureStyle.In(CaptureStyle.Name))
                {
                    return typeClean?.Invoke(prefixFullName, captureStyleNoPrefix) ?? prefixFullName;
                    //return prefixFullName;
                }
                else if (opt.CaptureStyle.In(CaptureStyle.Prefix))
                {
                    return typeClean?.Invoke(prefix, captureStyleNoPrefix) ?? prefix;
                    //return prefix;
                }
                else if (opt.CaptureStyle.In(CaptureStyle.Name))
                {
                    return typeClean?.Invoke(symbolName, captureStyleNoPrefix) ?? symbolName;
                    //return symbolName;
                }
            }

            //The only place to change the prefixFullName
            if (captureStyleNoPrefix)
            {
                prefixFullName = symbolName;
            }

            if (captureStyleNoName)
            {
                prefixFullName = prefix;
            }

            switch (symbol)
            {
                case INamedTypeSymbol named:
                    {
                        if (named.IsAnonymousType)
                        {
                            #region AnonymousType

                            /* Anonymous objects need to resolve the parameters of the unique constructor named.Constructors to obtain type information.
                               The style of anonymous objects is controlled by AnonymousTypeStyle, which is applicable to the single responsibility.
                             */
                            var parameters = named.Constructors.FirstOrDefault().Parameters.Select(c =>
                            {
                                //Process Type directly to avoid infinite recursion and controlled by AnonymousTypeStyle.
                                var named = GetFullName(c.Type, opt, typeClean);

                                if (opt.AnonymousTypeStyle.In(AnonymousTypeStyle.Type) && !opt.AnonymousTypeStyle.In(AnonymousTypeStyle.Name))
                                {
                                    return named;
                                }
                                else if (opt.AnonymousTypeStyle.In(AnonymousTypeStyle.Name) && !opt.AnonymousTypeStyle.In(AnonymousTypeStyle.Type))
                                {
                                    return c.Name;
                                }

                                return $"{named} {c.Name}";
                            });

                            if (opt.AnonymousTypeStyle.In(AnonymousTypeStyle.Round))
                            {
                                return $"({string.Join(", ", parameters)})";
                            }
                            else if (opt.AnonymousTypeStyle.In(AnonymousTypeStyle.Curly))
                            {
                                return $"{{{string.Join(", ", parameters)}}}";
                            }

                            return string.Join(", ", parameters);

                            #endregion
                        }

                        if (0 < named.TypeArguments.Length)
                        {
                            string args = default;

                            if (named.IsUnboundGenericType)
                            {
                                args = string.Join(",", Enumerable.Repeat(string.Empty, named.TypeArguments.Length));
                            }
                            else if (named.IsTupleType && 0 < named.TupleElements.Length)
                            {
                                //"System.ValueTuple<,>" is more representative.
                                if (named.IsDefinition)
                                {
                                    //opt = opt.Set(typeParameterStyle: TypeParameterStyle.Name);
                                    args = string.Join(",", Enumerable.Repeat(string.Empty, named.TypeArguments.Length));
                                }
                                else
                                {
                                    args = string.Join(", ", named.TupleElements.Select(c => GetFullName(c.Type, opt, typeClean)));
                                }
                            }
                            else
                            {
                                //This is an additional process, Generic type definition, you need to manually set the generic parameter name to simple name.
                                //if (named.IsDefinition && opt.TypeParameterStyle == TypeParameterStyle.FullName)

                                var typeParameterStyle = opt.TypeParameterStyle;

                                if (named.IsDefinition && !real)
                                {
                                    //opt = opt.Set(typeParameterStyle: TypeParameterStyle.Name);
                                    typeParameterStyle = TypeParameterStyle.Name;
                                }

                                //Merge generic parameters
                                var typeArguments = GetTypeArguments(named.TypeArguments, opt);

                                args = string.Join(", ", typeArguments.Select(c => GetFullName(c, opt.Set(typeParameterStyle: typeParameterStyle), typeClean)));
                            }

                            //Returns the type parameter of a generic type.
                            if (captureStyleArgs)
                            {
                                return $"{opt.TypeParameterBracketLeft}{args}{opt.TypeParameterBracketRight}";
                            }

                            //if (System_Nullable.Equals($"{prefix}.{symbol.Name}") && !opt.NoNullableQuestionMark)
                            if (SpecialType.System_Nullable_T == named.ConstructedFrom?.SpecialType && !opt.NoNullableQuestionMark)
                            {
                                if (!named.Equals(named.ConstructedFrom, SymbolEqualityComparer.Default))
                                {
                                    //Question mark, otherwise it will be treated as System.Nullable<>.
                                    return $"{args}?";
                                }
                            }

                            //return $"{typeClean?.Invoke(prefixFullName, noPrefix) ?? prefixFullName}<{args}>";
                            if (captureStyleNoArgs)
                            {
                                return prefixFullName;
                            }

                            return $"{prefixFullName}{opt.TypeParameterBracketLeft}{args}{opt.TypeParameterBracketRight}";
                        }

                        return prefixFullName;
                    }
                case ITypeParameterSymbol typeParameter:
                    {
                        switch (opt.TypeParameterStyle)
                        {
                            case TypeParameterStyle.Name:
                                {
                                    if (nullable)
                                    {
                                        if (!opt.NoNullableQuestionMark)
                                        {
                                            return $"{symbol.Name}?";
                                        }

                                        var nullName = $"System.Nullable<{symbol.Name}>";
                                        return typeClean?.Invoke(nullName, captureStyleNoPrefix) ?? nullName;
                                    }
                                    return symbol.Name;
                                }
                            case TypeParameterStyle.Real:
                                {
                                    var constraintType = TypeParameterSymbolRealType(typeParameter);
                                    return constraintType is null ? objectNameClean : GetFullName(constraintType, opt, typeClean);
                                    //return TypeParameterSymbolRealName(typeParameter, opt, typeClean);
                                }
                            case TypeParameterStyle.MethodReal:
                                {
                                    if (typeParameter.TypeParameterKind is TypeParameterKind.Type)
                                    {
                                        return typeParameter.Name;
                                    }

                                    var constraintType = TypeParameterSymbolRealType(typeParameter);
                                    return constraintType is null ? objectNameClean : GetFullName(constraintType, opt, typeClean);
                                    //return TypeParameterSymbolRealName(typeParameter, opt, typeClean);
                                }
                            default: break;
                        }

                        var name = symbol.Name;

                        if (nullable)
                        {
                            if (!opt.NoNullableQuestionMark)
                            {
                                name = $"{name}?";
                            }
                            else
                            {
                                var nullName = $"System.Nullable<{name}>";
                                name = typeClean?.Invoke(nullName, captureStyleNoPrefix) ?? nullName;
                            }
                        }

                        return $"{GetFullName(typeParameter.ContainingSymbol, opt.Set(captureStyle: opt.CaptureStyle | CaptureStyle.MethodName), typeClean)}.{name}";
                    }
                case IArrayTypeSymbol array:
                    {
                        var rank = "[]";

                        if (1 < array.Rank)
                        {
                            rank = $"[{string.Join(string.Empty, Enumerable.Repeat(",", array.Rank))}]";
                        }

                        return $"{GetFullName(array.ElementType, opt, typeClean)}{rank}";
                    }
                case IPointerTypeSymbol pointer:
                    {
                        const string rank = "*";

                        return $"{GetFullName(pointer.PointedAtType, opt, typeClean)}{rank}";
                    }
                case IMethodSymbol method:
                    {
                        #region Method

                        string args = default;

                        if (0 < method.TypeArguments.Length)
                        {
                            var typeParameterStyle = opt.TypeParameterStyle;

                            if (!real)
                            {
                                typeParameterStyle = TypeParameterStyle.Name;
                            }

                            //Merge generic parameters
                            var typeArguments = GetTypeArguments(method.TypeArguments, opt);

                            args = $"{opt.TypeParameterBracketLeft}{string.Join(", ", typeArguments.Select(c => GetFullName(c, opt.Set(typeParameterStyle: typeParameterStyle), typeClean)))}{opt.TypeParameterBracketRight}";

                            //Returns the type parameter of a generic method.
                            if (captureStyleArgs)
                            {
                                return args;
                            }
                        }

                        //var methodName = $"{typeClean?.Invoke(prefixFullName, noPrefix) ?? prefixFullName}{args}";
                        var methodName = $"{prefixFullName}{args}";

                        if (opt.CaptureStyle.In(CaptureStyle.MethodName))
                        {
                            return methodName;
                        }

                        //var ordinal = 0;

                        var parameters = string.Join(", ", method.Parameters.Select(c =>
                        {
                            //Merge generic parameters
                            var type = (opt.TypeArguments?.Any() ?? false) && opt.TypeArguments.TryGetValue(c.Type.GetFullName(), out ITypeSymbol typeSymbol2) ? typeSymbol2 : c.Type;

                            string typeName = null;

                            switch (opt.ParameterStyle)
                            {
                                case ParameterStyle.Type:
                                    typeName = GetFullName(type, opt, typeClean);
                                    break;
                                case ParameterStyle.Name:
                                    typeName = c.Name;
                                    break;
                                case ParameterStyle.TypeName:
                                    typeName = $"{GetFullName(type, opt, typeClean)} {c.Name}";
                                    break;
                                default: break;
                            }

                            //arg.args -> arg
                            var refKey = opt.MethodParameterRefVarName.Split('.')[0];

                            switch (c.RefKind)
                            {
                                case RefKind.Ref:
                                    if (real)
                                    {
                                        typeName = $"ref {refKey}{c.Ordinal}";
                                        //ordinal++;
                                    }
                                    else
                                    {
                                        typeName = $"ref {typeName}";
                                    }
                                    break;
                                case RefKind.Out:
                                    typeName = real ? $"out {typeName} {refKey}{c.Ordinal}" : $"out {typeName}";
                                    break;
                                default: //Get specific constraints for generics.
                                    if (real)
                                    {
                                        var varName = string.Format(opt.MethodParameterVarName, $"[{c.Ordinal}]");// $"{opt.MethodParameterVarName}[{ordinal}]";
                                        typeName = GetTypeInfo(type, varName, typeName).typeName;

                                        if (!(opt.MethodParameterVarNameCallback is null))
                                        {
                                            typeName = opt.MethodParameterVarNameCallback(typeName, c.Ordinal);
                                        }

                                        //ordinal++;
                                    }
                                    break;
                            }

                            return typeName;
                        }));

                        parameters = $"({parameters})";

                        if (opt.CaptureStyle.In(CaptureStyle.MethodParameters))
                        {
                            return parameters;
                        }

                        return $"{methodName}{parameters}";

                        #endregion
                    }
                default: break;
            }

            return typeClean?.Invoke(prefixFullName, captureStyleNoPrefix) ?? prefixFullName;
        }

        /// <summary>
        /// Merge generic parameters
        /// </summary>
        /// <param name="typeArguments"></param>
        /// <param name="opt"></param>
        static IEnumerable<ITypeSymbol> GetTypeArguments(IEnumerable<ITypeSymbol> typeArguments, GetFullNameOpt opt) => (opt.TypeArguments?.Any() ?? false) ? typeArguments.Select(c => opt.TypeArguments.TryGetValue(c.GetFullName(), out ITypeSymbol typeSymbol) ? typeSymbol : c) : typeArguments;

        /// <summary>
        /// Set generic parameters for a generic type.
        /// </summary>
        /// <param name="named"></param>
        /// <param name="typeArgument"></param>
        /// <returns></returns>
        public static IDictionary<string, ITypeSymbol> SetTypeArguments(this INamedTypeSymbol named, params ITypeSymbol[] typeArgument)
        {
            if (named.IsGenericType && named.TypeArguments.Any(c => c.TypeKind is TypeKind.TypeParameter))
            {
                return named.TypeParameters.ToDictionary(c => named.TypeArguments[c.Ordinal].GetFullName(), c => c.Ordinal < typeArgument?.Length ? typeArgument[c.Ordinal] : named.TypeArguments[c.Ordinal]);
            }

            return default;
            //return named.IsGenericType && !named.TypeArguments.Any(c => c.TypeKind is TypeKind.TypeParameter) ? named.TypeParameters.ToDictionary(c => named.TypeArguments[c.Ordinal].GetFullName(), c => c.Ordinal < typeArgument?.Length ? typeArgument[c.Ordinal] : named.TypeArguments[c.Ordinal]) : default;
        }

        /// <summary>
        /// Set generic parameters for a generic method.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="typeArgument"></param>
        /// <returns></returns>
        public static IDictionary<string, ITypeSymbol> SetTypeArguments(this IMethodSymbol method, params ITypeSymbol[] typeArgument) => method.IsGenericMethod ? method.TypeParameters.ToDictionary(c => method.TypeArguments[c.Ordinal].GetFullName(), c => c.Ordinal < typeArgument?.Length ? typeArgument[c.Ordinal] : method.TypeArguments[c.Ordinal]) : default;

        public static (string typeName, bool isValueType) GetTypeInfo(this ITypeSymbol type, string varName, string typeName)
        {
            bool? isValueType = default;

            if (!(type.SpecialType is SpecialType.System_Object || type.TypeKind is TypeKind.Dynamic))
            {
                isValueType = type.IsValueType;

                if (type is ITypeParameterSymbol typeParameter)
                {
                    //Define generics, cast."(T)arg"
                    if (typeParameter.TypeParameterKind is TypeParameterKind.Type)
                    {
                        isValueType = true;
                    }
                    else
                    {
                        //Obtain specific constraints, otherwise return "args[i]".
                        var constraintType = typeParameter.TypeParameterSymbolRealType();

                        if (constraintType is null)
                        {
                            isValueType = default;
                        }
                        else
                        {
                            isValueType = constraintType.IsValueType;
                        }
                    }
                }
            }

            //var typeKind = (type.SpecialType is SpecialType.None ? type.TypeKind : TypeKind.Unknown).GetName();

            //if (!TypeConvert(type.SpecialType, varName, global, out string result))
            //{
            //    result = $"({typeName}){varName}";
            //}

            return (!isValueType.HasValue ? varName : isValueType.Value ? $"({typeName}){varName}" : $"{varName} as {typeName}", isValueType ?? false);
        }

        internal static (string refs, string refs2) GetMethodRef(this IEnumerable<IParameterSymbol> parameters, string methodParameterVarName, string methodParameterVarName2, Func<string, bool, string> typeClean, SyntaxToCode.ToCodeOpt opt)
        {
            var globalMeta = opt.GetGlobalName(GlobalName.Business_SourceGenerator_Meta);

            var refs = new System.Text.StringBuilder(null);
            var refs2 = new System.Text.StringBuilder(null);

            foreach (var item in parameters)
            {
                switch (item.RefKind)
                {
                    case RefKind.Ref:
                        {
                            var typeName = item.Type.GetFullNameMethodRealStandardFormat(GetFullNameOpt.Create(methodParameterVarName: methodParameterVarName), typeClean: typeClean);

                            //var argName = $"{methodParameterVarName}[{ordinal}]";
                            var varName = string.Format(methodParameterVarName, $"[{item.Ordinal}]");
                            var varName2 = $"{methodParameterVarName2}[{item.Ordinal}]";

                            typeName = item.Type.GetTypeInfo(varName, typeName).typeName;

                            //arg.args -> arg
                            var refKey = methodParameterVarName2.Split('.')[0];

                            refs.Append($"var {refKey}{item.Ordinal} = {typeName}; ");
                            //refs2.Append($"{varName2} = {refKey}{item.Ordinal}; ");
                            refs2.Append($"{varName2} = (({globalMeta}RefArg){varName2}).Set({refKey}{item.Ordinal}); ");
                            break;
                        }
                    case RefKind.Out:
                        {
                            var varName2 = $"{methodParameterVarName2}[{item.Ordinal}]";
                            var refKey = methodParameterVarName2.Split('.')[0];
                            refs2.Append($"{varName2} = (({globalMeta}RefArg){varName2}).Set({refKey}{item.Ordinal}); ");
                            //outs.Add($"out{item.Ordinal}");
                            break;
                        }
                    default: break;
                }
            }

            return (refs.ToString(), refs2.ToString());
        }

        /*
        static bool TypeConvert(SpecialType specialType, string varName, bool global, out string result)
        {
            var globalSystem = global ? $"{Global}System." : default;

            switch (specialType)
            {
                case SpecialType.System_Boolean:
                    result = $"{globalSystem}Convert.ToBoolean({varName})";
                    return true;
                case SpecialType.System_Char:
                    result = $"{globalSystem}Convert.ToChar({varName})";
                    return true;
                case SpecialType.System_SByte:
                    result = $"{globalSystem}Convert.ToSByte({varName})";
                    return true;
                case SpecialType.System_Byte:
                    result = $"{globalSystem}Convert.ToByte({varName})";
                    return true;
                case SpecialType.System_Int16:
                    result = $"{globalSystem}Convert.ToInt16({varName})";
                    return true;
                case SpecialType.System_UInt16:
                    result = $"{globalSystem}Convert.ToUInt16({varName})";
                    return true;
                case SpecialType.System_Int32:
                    result = $"{globalSystem}Convert.ToInt32({varName})";
                    return true;
                case SpecialType.System_UInt32:
                    result = $"{globalSystem}Convert.ToUInt32({varName})";
                    return true;
                case SpecialType.System_Int64:
                    result = $"{globalSystem}Convert.ToInt64({varName})";
                    return true;
                case SpecialType.System_UInt64:
                    result = $"{globalSystem}Convert.ToUInt64({varName})";
                    return true;
                case SpecialType.System_Decimal:
                    result = $"{globalSystem}Convert.ToDecimal({varName})";
                    return true;
                case SpecialType.System_Single:
                    result = $"{globalSystem}Convert.ToSingle({varName})";
                    return true;
                case SpecialType.System_Double:
                    result = $"{globalSystem}Convert.ToDouble({varName})";
                    return true;
                case SpecialType.System_String:
                    result = $"{globalSystem}Convert.ToString({varName})";
                    return true;
                case SpecialType.System_DateTime:
                    result = $"{globalSystem}Convert.ToDateTime({varName})";
                    return true;
                default: result = default; return default;
            }
        }
        */

        public static ITypeSymbol TypeParameterSymbolRealType(this ITypeParameterSymbol typeParameter)
        {
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

            return constraintTypes.FirstOrDefault();
        }

        public static bool IsPointerType(this ITypeSymbol typeSymbol) => TypeChecked(typeSymbol, type => type.TypeKind is TypeKind.Pointer || type.TypeKind is TypeKind.FunctionPointer);

        public static bool HasGenericType(this ITypeSymbol typeSymbol) => TypeChecked(typeSymbol, type => type.TypeKind is TypeKind.TypeParameter);

        public static bool TypeChecked(this ITypeSymbol typeSymbol, Func<ITypeSymbol, bool> check)
        {
            if (typeSymbol is null)
            {
                throw new ArgumentNullException(nameof(typeSymbol));
            }

            if (check is null)
            {
                throw new ArgumentNullException(nameof(check));
            }

            if (check(typeSymbol))
            {
                return true;
            }

            if (typeSymbol is IArrayTypeSymbol arrayType)
            {
                return TypeChecked(arrayType.ElementType, check);
            }

            if (typeSymbol is INamedTypeSymbol named)
            {
                if (named.IsUnboundGenericType)
                {
                    return false;
                }

                if (named.IsAnonymousType)
                {
                    return named.Constructors.FirstOrDefault().Parameters.Any(c => TypeChecked(c.Type, check));
                }

                if (named.IsTupleType)
                {
                    return named.TupleElements.Any(c => TypeChecked(c.Type, check));
                }

                return named.TypeArguments.Any(c => TypeChecked(c, check));
            }

            return false;
        }

        /*
        static string TypeParameterSymbolRealName(ITypeParameterSymbol typeParameter, GetFullNameOpt opt, Func<string, string> typeClean)
        {
            var constraintType = TypeParameterSymbolRealType(typeParameter);

            if (constraintType is null)
            {
                return typeClean(System_Object);
            }

            return GetFullName(constraintType, opt, typeClean);
        }
        */

        #endregion
    }
}