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
    using System.Collections.Generic;
    using System.Linq;
    using static Business.SourceGenerator.Analysis.SymbolTypeName;
    using static Business.SourceGenerator.Analysis.SyntaxToCode;

    internal static class SymbolToMeta
    {
        public enum AsyncType
        {
            None,
            Task,
            TaskGeneric,
            ValueTask,
            ValueTaskGeneric,
            Other,
        }

        public static AsyncType GetAsyncType(ITypeSymbol symbol)
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

        static string ToDefaultValue(ISymbol symbol)
        {
            SpecialType specialType = SpecialType.None;
            object value = default;

            switch (symbol)
            {
                //case IFieldSymbol fieldSymbol:
                //    if (!fieldSymbol.HasConstantValue)
                //    {
                //        return "default";
                //    }

                //    specialType = fieldSymbol.Type.SpecialType;
                //    value = fieldSymbol.ConstantValue;
                //    break;
                case IParameterSymbol parameterSymbol:
                    if (!parameterSymbol.HasExplicitDefaultValue)
                    {
                        return "default";
                    }

                    specialType = parameterSymbol.Type.SpecialType;
                    value = parameterSymbol.ExplicitDefaultValue;
                    break;
                default: break;
            }

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

        public static string GetConstructor(IMethodSymbol constructorMethod, IDictionary<string, ITypeSymbol> typeArguments, Func<string, bool, string> typeClean, ToCodeOpt opt, string assemblyName)
        {
            var globalMeta = opt.GetGlobalName(GlobalName.Business_SourceGenerator_Meta);
            var globalObject = opt.GetGlobalName(GlobalName.System_Object);
            var globalSystem = opt.GetGlobalName(GlobalName.System);

            var mustLength = 0;

            var parameterList = constructorMethod.Parameters.Select(c =>
            {
                if (!c.HasExplicitDefaultValue && !c.IsParams)
                {
                    mustLength++;
                }

                //if (c.Type.SpecialType is SpecialType.System_Object || c.Type.TypeKind is TypeKind.Dynamic)
                //{
                //    return AnalysisMeta.AnalysisInfo.AccessorType.TryGetValue("System.Object", out string v) ? v : $"default({globalMeta}(AccessorParameter))";
                //    //return ToMetaParameter(globalMeta, globalObject, TypeKind.Unknown, c.RefKind, c.Ordinal, c.HasExplicitDefaultValue, ToDefaultValue(c), ImplicitDefaultValue(c), c.IsParams, $"c => c ? new {globalObject}() : default");
                //}

                //var type = (typeArguments?.Any() ?? false) && typeArguments.TryGetValue(c.Type.GetFullName(), out ITypeSymbol typeSymbol) ? typeSymbol : c.Type;

                //var typeName = $"{type.GetFullNameRealStandardFormat(typeClean: typeClean)}";

                //var typeKind = type.SpecialType is SpecialType.None ? type.TypeKind : TypeKind.Unknown;

                //(skip || !accessor.Parameters.Any()) ? "default" : $"new {globalMeta}IAccessorParameter[] {{ {string.Join(", ", accessor.Parameters.Select(c => c.ToMeta(opt, depth, typeClean: typeClean)))} }}";
                return c.ToMeta3(opt, typeClean, default, assemblyName);

                //return ToMetaParameter(globalMeta, typeName, typeKind, c.RefKind, c.Ordinal, c.HasExplicitDefaultValue, ToDefaultValue(c), ImplicitDefaultValue(c), c.IsParams, GetDefaultValue(type, typeClean, globalSystem), c.Type.IsGenericType());
            }).ToArray();

            const string methodParameterRefVarName = "args";
            const string methodParameterVarName = "m{0}.v";

            var sign = constructorMethod.GetFullNameMethodRealStandardFormat(GetFullNameOpt.Create(captureStyle: CaptureStyle.NoName, methodParameterVarName: methodParameterVarName, methodParameterRefVarName: methodParameterRefVarName, methodParameterVarNameCallback: MethodParameterVarNameCallback, typeArguments: typeArguments, global: opt.Global), typeClean: typeClean);

            var refs = constructorMethod.Parameters.GetMethodRef(methodParameterVarName, methodParameterRefVarName, typeClean, opt);

            return $"new {globalMeta}Constructor({(parameterList.Any() ? $"new {globalMeta}IAccessorParameter[] {{ {string.Join(", ", parameterList)} }}" : "default")}, {constructorMethod.Parameters.Length}, {mustLength}, (obj, m, {methodParameterRefVarName}) => {{ {refs.refs}var result = new {sign}; {refs.refs2}return result; }})";
        }

        static string GetRuntimeType(ITypeSymbol type, Func<string, bool, string> typeClean)
        {
            if (type.SpecialType is SpecialType.System_Void)
            {
                return "void";
            }

            string typeName = type.GetFullNameRealStandardFormat(typeClean: typeClean);

            if (typeName.Contains("*"))
            {
                return default;
            }

            return typeName;
        }

        static string GetDefaultValue(ITypeSymbol type, Func<string, bool, string> typeClean, string globalSystem)
        {
            if (type.SpecialType is SpecialType.System_Void)
            {
                return "c => default";
            }

            var typeName = GetRuntimeType(type, typeClean);

            if (typeName is null)
            {
                return "c => default";
            }

            var isArray = type.TypeKind is TypeKind.Array;

            var arrayTypeName = type is IArrayTypeSymbol arrayType ? GetRuntimeType(arrayType.ElementType, typeClean) : default;

            var defaultConstructor = !type.IsAbstract && (type.TypeKind is TypeKind.Class || type.TypeKind is TypeKind.Struct) && type is INamedTypeSymbol named && named.InstanceConstructors.Any(c => c.DeclaredAccessibility is Accessibility.Public && !c.Parameters.Any() && !c.IsObsolete());

            var typeDefault = type.IsValueType ? $"default({typeName})" : "default";
            var typeConstructor = defaultConstructor ? $"new {typeName}()" : isArray ? $"{globalSystem}Array.Empty<{arrayTypeName}>()" : typeDefault;
            var defaultValue = $"c => {(defaultConstructor || isArray ? $"c ? {typeConstructor} : {typeDefault}" : typeDefault)}";
            return defaultValue;
        }

        static bool ImplicitDefaultValue(IParameterSymbol parameterSymbol) => parameterSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is ParameterSyntax parameter && parameter.Default?.Value is LiteralExpressionSyntax literalExpression && literalExpression.Token.IsKind(SyntaxKind.DefaultKeyword);// || parameterSymbol.IsParams;

        static string MethodParameterVarNameCallback(string typeName, int ordinal) => $"!m[{ordinal}].c ? {typeName} : default";

        static string GetValueCode(IMethodSymbol method, string receiverType, bool asTask, Func<string, bool, string> typeClean, ToCodeOpt opt)
        {
            const string methodParameterRefVarName = "args";
            //!m[3].c ? global::Sl::System.ValueTuple<global::System.Int32?, global::System.String>)m[1].v; ystem.Convert.ToSingle(m[3].v) : default
            const string methodParameterVarName = "m{0}.v";
            const string methodParameterDeclaration = "(obj, m, args)";
            //var m{0}0 = m[0].v as global::System.String; var m{0}1 = (global::System.ValueTuple<global::System.Int32?, global::System.String>)m[1].v; 
            var globalObject = opt.GetGlobalName(GlobalName.System_Object);
            var globalTasks = opt.GetGlobalName(GlobalName.System_Threading_Tasks);

            var sign = method.GetFullNameMethodRealStandardFormat(GetFullNameOpt.Create(tupleStyle: TupleStyle.TypeName, captureStyle: CaptureStyle.NoPrefix, methodParameterVarName: methodParameterVarName, methodParameterRefVarName: methodParameterRefVarName, methodParameterVarNameCallback: MethodParameterVarNameCallback, global: opt.Global), typeClean: typeClean);

            var refs = method.Parameters.GetMethodRef(methodParameterVarName, methodParameterRefVarName, typeClean, opt);

            if (sign is null)
            {
                return $"{methodParameterDeclaration} => default";
            }

            var result = method.IsStatic ? $"{receiverType}.{sign}" : $"(({receiverType})obj).{sign}";

            //var make = $"var m = {globalSourceGenerator}Utils.CheckedMethodParameters(member, args);";

            if (method.ReturnsVoid)
            {
                if (asTask)
                {
                    result = $"{methodParameterDeclaration} => {{ {refs.refs}{result}; {refs.refs2}return {globalTasks}Task.FromResult<{globalObject}>(default); }}";
                }
                else
                {
                    result = $"{methodParameterDeclaration} => {{ {refs.refs}{result}; {refs.refs2}return default; }}";
                }
            }
            else
            {
                if (asTask)
                {
                    var asyncType = GetAsyncType(method.ReturnType);

                    switch (asyncType)
                    {
                        case AsyncType.None:
                            result = $"{methodParameterDeclaration} => {{ {refs.refs}object result = {result}; {refs.refs2}return {globalTasks}Task.FromResult(result); }}";
                            break;
                        case AsyncType.Task:
                            result = $"async {methodParameterDeclaration} => {{ {refs.refs}await {result}; {refs.refs2}return default; }}";
                            break;
                        case AsyncType.TaskGeneric:
                            result = $"async {methodParameterDeclaration} => {{ {refs.refs}var result = await {result}; {refs.refs2}return result; }}";
                            break;
                        case AsyncType.ValueTask:
                            result = $"async {methodParameterDeclaration} => {{ {refs.refs}await {result}; {refs.refs2}return default; }}";
                            break;
                        case AsyncType.ValueTaskGeneric:
                            result = $"async {methodParameterDeclaration} => {{ {refs.refs}var result = await {result}; {refs.refs2}return result; }}";
                            break;
                        case AsyncType.Other:
                            result = $"async {methodParameterDeclaration} => {{ {refs.refs}await {result}; {refs.refs2}return default; }}";
                            break;
                        default: break;
                    }
                }
                else
                {
                    result = $"{methodParameterDeclaration} => {{ {refs.refs}var result = {result}; {refs.refs2}return result; }}";
                }
            }

            return result;
        }

        static string GetEventCode(this IMethodSymbol method, string methodName, string receiverType, Func<string, bool, string> typeClean, ToCodeOpt opt)
        {
            const string methodParameterDeclaration = "(obj, arg)";

            var sign = method.GetFullNameMethodRealStandardFormat(GetFullNameOpt.Create(captureStyle: CaptureStyle.MethodParameters, methodParameterVarName: "arg", global: opt.Global), typeClean: typeClean);

            var refs = method.Parameters.GetMethodRef(default, default, typeClean, opt);

            if (sign is null)
            {
                return $"{methodParameterDeclaration} => {{ }}";
            }

            var result = method.IsStatic ? $"{receiverType}.{methodName}{sign}" : $"(({receiverType})obj).{methodName}{sign}";

            result = $"{methodParameterDeclaration} => {{ {refs.refs}{result}; }}";

            return result;
        }

        public static string ToMetaParameter(string globalMeta, string type, TypeKind typeKind = TypeKind.Unknown, RefKind refKind = RefKind.None, int ordinal = default, bool hasExplicitDefaultValue = default, string explicitDefaultValue = "default", bool implicitDefaultValue = default, bool isParams = default, string defaultValue = default, bool hasGenericType = default) => $"new {globalMeta}Parameter(typeof({type}), {globalMeta}TypeKind.{typeKind.GetName()}, {globalMeta}RefKind.{refKind.GetName()}, {ordinal}, {(hasExplicitDefaultValue ? "true" : "default")}, {explicitDefaultValue}, {(implicitDefaultValue ? "true" : "default")}, {(isParams ? "true" : "default")}, {defaultValue}, {(hasGenericType ? "true" : "default")})";

        //const int accessorDepth = 4;

        static void AppendFormat(this System.Text.StringBuilder sb, bool v, bool end = default) => sb.AppendFormat(end ? "{0}" : "{0}, ", v ? "true" : "default");
        static string ToName(this bool v) => v ? "true" : "default";

        public static string ToMeta3(this IEnumerable<IMethodSymbol> methodSymbols, ToCodeOpt opt, Func<string, bool, string> typeClean, string receiverType, string assemblyName)
        {
            var globalMeta = opt.GetGlobalName(GlobalName.Business_SourceGenerator_Meta);

            var methods = methodSymbols.Select(c => c.ToMeta3(opt, typeClean, receiverType, assemblyName)).Where(c => !(c is null)).ToArray();

            if (1 < methods.Length)
            {
                return $"new {globalMeta}AccessorMethodCollection(new {globalMeta}IAccessorMethod[] {{ {string.Join(", ", methods)} }})";
            }
            else if (1 == methods.Length)
            {
                return methods[0];
            }

            return default;
        }

        public static string ToMeta3(this ISymbol symbol, ToCodeOpt opt, Func<string, bool, string> typeClean, string receiverType, string assemblyName)
        {
            var space = Meta.Global.Space;
            var format = opt.StandardFormat ? Environment.NewLine : " ";
            var accessorTypes = $"{assemblyName}.AccessorTypes";
            var accessorTypes2 = $"{opt.GetGlobalName(GlobalName.Globa)}{accessorTypes}.";
            var globalMeta = opt.GetGlobalName(GlobalName.Business_SourceGenerator_Meta);
            var globalGeneric = opt.GetGlobalName(GlobalName.System_Collections_Generic);
            var globalObject = opt.GetGlobalName(GlobalName.System_Object);
            var globalSystem = opt.GetGlobalName(GlobalName.System);
            var globalLazy = opt.GetGlobalName(GlobalName.System_Lazy);
            var globalType = opt.GetGlobalName(GlobalName.System_Type);
            var globalString = opt.GetGlobalName(GlobalName.System_String);
            var globalBoolean = opt.GetGlobalName(GlobalName.System_Boolean);

            switch (symbol)
            {
                case INamedTypeSymbol accessor:
                    {
                        var fullName = symbol.GetFullNameStandardFormat();

                        //if (AnalysisMeta.AnalysisInfo.BasrType.TryGetValue(fullName, out string v))
                        //{
                        //    return v;
                        //}

                        var key = $"{symbol.GetFullNameStandardFormat(GetFullNameOpt.Create(noNullableQuestionMark: true, charSpecial: CharSpecial.Create('_')))}";
                        //var key2 = typeClean?.Invoke(key, false);

                        //if (accessor.TypeKind is TypeKind.Class || accessor.TypeKind is TypeKind.Struct || accessor.TypeKind is TypeKind.Interface || accessor.TypeKind is TypeKind.Enum)

                        if (AnalysisMeta.AnalysisInfo.AccessorType.ContainsKey(fullName))
                        {
                            return $"{accessorTypes2}{key}.Singleton";
                        }

                        AnalysisMeta.AnalysisInfo.AccessorType.TryAdd(fullName, default);

                        var runtimeType = GetRuntimeType(accessor, typeClean);
                        var isTupleType = accessor.IsTupleType && !accessor.IsDefinition;
                        var fullName2 = isTupleType ? symbol.GetFullNameRealStandardFormat(GetFullNameOpt.Create(tupleStyle: TupleStyle.TypeName), typeClean: typeClean) : symbol.GetFullNameRealStandardFormat(typeClean: typeClean);
                        var attrs = accessor.GetAttributes().Where(c => !(c.ApplicationSyntaxReference is null) && c.AttributeClass.DeclaredAccessibility is Accessibility.Public);

                        #region members

                        var members = new Dictionary<string, string>();
                        var methods = new Dictionary<string, IList<IMethodSymbol>>();

                        foreach (var item in accessor.GetMembers())
                        {
                            if (Accessibility.Public != item.DeclaredAccessibility)
                            {
                                continue;
                            }

                            switch (item)
                            {
                                case IMethodSymbol member:
                                    if (!symbol.DeclaringSyntaxReferences.Any()) { continue; }
                                    if (MethodKind.Ordinary == member.MethodKind)
                                    {
                                        if (!methods.TryGetValue(member.Name, out IList<IMethodSymbol> m))
                                        {
                                            methods.Add(member.Name, new List<IMethodSymbol> { member });
                                        }
                                        else
                                        {
                                            m.Add(member);
                                        }
                                    }
                                    break;
                                case IFieldSymbol member:
                                    {
                                        if (member.IsObsolete() || member.IsImplicitlyDeclared || member.Type.IsRefLikePointerTypedReferenceValueTypeConstraint()) { continue; }

                                        if (member.Type.Kind is SymbolKind.Event) { continue; }

                                        if (member.Type.IsValueTypeConstraint()) { continue; }

                                        members.Add(member.Name, member.ToMeta3(opt, typeClean, fullName2, assemblyName));
                                    }
                                    break;
                                case IPropertySymbol member:
                                    {
                                        if (member.IsObsolete() || member.IsImplicitlyDeclared || member.Type.IsRefLikePointerTypedReferenceValueTypeConstraint() || member.IsIndexer) { continue; }

                                        if (member.Type.Kind is SymbolKind.Event) { continue; }

                                        if (member.Type.IsValueTypeConstraint()) { continue; }

                                        members.Add(member.Name, member.ToMeta3(opt, typeClean, fullName2, assemblyName));
                                    }
                                    break;
                                case IEventSymbol member:
                                    {
                                        if (!symbol.DeclaringSyntaxReferences.Any() || member.IsObsolete()) { continue; }
                                        if (member.Type.IsRefLikePointerTypedReferenceValueTypeConstraint()) { continue; }

                                        if (member.Type.IsValueTypeConstraint()) { continue; }

                                        members.Add(member.Name, member.ToMeta3(opt, typeClean, fullName2, assemblyName));
                                    }
                                    break;
                                default: break;
                            }
                        }

                        foreach (var item in methods)
                        {
                            if (1 < item.Value.Count)
                            {
                                var method = item.Value.ToMeta3(opt, typeClean, fullName2, assemblyName);

                                if (method is null)
                                {
                                    continue;
                                }

                                members.Add(item.Key, method);
                            }
                            else
                            {
                                var method = item.Value.First().ToMeta3(opt, typeClean, fullName2, assemblyName);

                                if (method is null)
                                {
                                    continue;
                                }

                                members.Add(item.Key, method);
                            }
                        }

                        #endregion

                        #region Members

                        var sb = new System.Text.StringBuilder();
                        int spaceCount = 0;

                        if (!string.IsNullOrEmpty(assemblyName))
                        {
                            sb.AppendFormat("namespace {1}{0}", $"{format}{{{format}", accessorTypes);
                            spaceCount++;
                        }

                        sb.SetSummary(space.Repeat(spaceCount), key);
                        sb.AppendLine($"{space.Repeat(spaceCount)}public readonly struct {key} : {globalMeta}IAccessorNamedType");
                        sb.AppendLine($"{space.Repeat(spaceCount)}{{");
                        spaceCount++;
                        sb.AppendLine($"{space.Repeat(spaceCount)}readonly static {globalLazy}<{accessorTypes2}{key}> instance = new {globalLazy}<{accessorTypes2}{key}>(() => new {accessorTypes2}{key}());");
                        sb.SetSummary(space.Repeat(spaceCount), "Singleton");
                        sb.AppendLine($"public static {accessorTypes2}{key} Singleton => instance.Value;");
                        //sb.AppendLine($"{space.Repeat(spaceCount)}public {key}() {{ }}");
                        sb.SetSummary(space.Repeat(spaceCount), "ToString()");
                        sb.AppendLine($"{space.Repeat(spaceCount)}public override {globalString} ToString() => $\"{{Kind}}\";");
                        //==================Meta==================//
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsExtern", symbol.IsExtern.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsSealed", symbol.IsSealed.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsAbstract", symbol.IsAbstract.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsOverride", symbol.IsOverride.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsVirtual", symbol.IsVirtual.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsStatic", symbol.IsStatic.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), $"{globalMeta}Kind", "Kind", $"{globalMeta}Kind.{symbol.Kind}");
                        //==================IAccessorType==================//
                        sb.AddProperty(space.Repeat(spaceCount), $"{globalGeneric}IDictionary<{globalString}, {globalMeta}IAccessor>", "Members", members.Any() ? $"new {globalGeneric}Dictionary<{globalString}, {globalMeta}IAccessor> {{ {string.Join(", ", members.Select(c => $"{{ \"{c.Key}\", {c.Value} }}"))} }}" : "default");
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsReadOnly", accessor.IsReadOnly.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsUnmanagedType", accessor.IsUnmanagedType.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), $"{globalMeta}SpecialType", "SpecialType", $"{globalMeta}SpecialType.{accessor.SpecialType}");
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsTupleType", accessor.IsTupleType.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsAnonymousType", accessor.IsAnonymousType.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), $"{globalMeta}TypeKind", "TypeKind", $"{globalMeta}TypeKind.{accessor.TypeKind}");
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsRecord", accessor.IsRecord.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), $"{globalMeta}AsyncType", "AsyncType", $"{globalMeta}AsyncType.{GetAsyncType(accessor)}");
                        //sb.AddProperty(space.Repeat(spaceCount), globalString, "Name", $"\"{fullName}\"");
                        sb.AddProperty(space.Repeat(spaceCount), globalType, "RuntimeType", !(runtimeType is null) ? $"typeof({runtimeType})" : "default");
                        sb.AddProperty(space.Repeat(spaceCount), $"{globalMeta}DefaultValue", "DefaultValue", GetDefaultValue(accessor, typeClean, globalSystem));
                        //==================IAccessorNamedType==================//
                        sb.AddProperty(space.Repeat(spaceCount), $"{globalMeta}AccessorAttribute[]", "Attributes", attrs.Any() ? $"{GetAttributes(attrs, globalMeta, typeClean)}" : "default");
                        sb.AddProperty(space.Repeat(spaceCount), $"{globalGeneric}IEnumerable<{globalMeta}IAccessorField>", "TupleElements", (isTupleType && accessor.TupleElements.Any()) ? $"new {globalMeta}IAccessorField[] {{ {string.Join(", ", accessor.TupleElements.Select(c => c.ToMeta3(opt, typeClean, fullName2, assemblyName)))} }}" : "default");
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "MightContainExtensionMethods", accessor.MightContainExtensionMethods.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsSerializable", accessor.IsSerializable.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsPartial", accessor.IsPartial().ToName());
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "HasTypeParameter", accessor.IsTypeParameter().ToName());
                        sb.AddProperty(space.Repeat(spaceCount), $"{globalMeta}IAccessorType[]", "TypeArguments", accessor.TypeArguments.Any() ? $"new {globalMeta}IAccessorType[] {{ {string.Join(", ", accessor.TypeArguments.Select(c => c.ToMeta3(opt, typeClean, fullName2, assemblyName)))} }}" : "default");

                        spaceCount--;
                        sb.Append($"{space.Repeat(spaceCount)}}}");
                        if (!string.IsNullOrEmpty(assemblyName))
                        {
                            sb.AppendLine();
                            spaceCount--;
                            sb.Append($"{space.Repeat(spaceCount)}}}");
                        }

                        #endregion

                        if (AnalysisMeta.AnalysisInfo.AccessorType.ContainsKey(fullName))
                        {
                            AnalysisMeta.AnalysisInfo.AccessorType[fullName] = sb.ToString();
                        }

                        return $"{accessorTypes2}{key}.Singleton";
                    }
                case ITypeParameterSymbol accessor:
                    {
                        //var fullName = symbol.GetFullNameStandardFormat();
                        var runtimeType = accessor.GetFullNameRealStandardFormat(typeClean: typeClean);

                        var sb = new System.Text.StringBuilder($"new {globalMeta}AccessorTypeParameter(");
                        //==================Meta==================//
                        sb.AppendFormat(accessor.IsExtern);
                        sb.AppendFormat(accessor.IsSealed);
                        sb.AppendFormat(accessor.IsAbstract);
                        sb.AppendFormat(accessor.IsOverride);
                        sb.AppendFormat(accessor.IsVirtual);
                        sb.AppendFormat(accessor.IsStatic);
                        sb.AppendFormat($"{globalMeta}Kind.{{0}}, ", accessor.Kind.GetName());
                        //==================IAccessorType==================//
                        sb.Append("default, ");
                        sb.AppendFormat(accessor.IsReadOnly);
                        sb.AppendFormat(accessor.IsUnmanagedType);
                        sb.AppendFormat($"{globalMeta}SpecialType.{{0}}, ", accessor.SpecialType.GetName());
                        sb.AppendFormat(accessor.IsTupleType);
                        sb.AppendFormat(accessor.IsAnonymousType);
                        sb.AppendFormat($"{globalMeta}TypeKind.{{0}}, ", accessor.TypeKind.GetName());
                        sb.AppendFormat(accessor.IsRecord);
                        sb.AppendFormat($"{globalMeta}AsyncType.{{0}}, ", GetAsyncType(accessor).GetName());
                        //sb.AppendFormat("{0}, ", $"\"{fullName}\"");
                        sb.AppendFormat("{0}, ", (!(runtimeType is null) ? $"typeof({runtimeType})" : "default"));
                        sb.AppendFormat("{0}, ", GetDefaultValue(accessor, typeClean, globalSystem));
                        //==================IAccessorTypeParameter==================//
                        sb.AppendFormat("{0}, ", accessor.Ordinal);
                        sb.AppendFormat($"{globalMeta}VarianceKind.{{0}}, ", accessor.Variance.GetName());
                        sb.AppendFormat($"{globalMeta}TypeParameterKind.{{0}}, ", accessor.TypeParameterKind.GetName());
                        sb.AppendFormat(accessor.HasReferenceTypeConstraint);
                        sb.AppendFormat(accessor.HasValueTypeConstraint);
                        sb.AppendFormat(accessor.HasUnmanagedTypeConstraint);
                        sb.AppendFormat(accessor.HasNotNullConstraint);
                        sb.AppendFormat(accessor.HasConstructorConstraint);
                        sb.Append(accessor.ConstraintTypes.Any() ? $"new {globalMeta}IAccessorType[] {{ {string.Join(", ", accessor.ConstraintTypes.Select(c => c.ToMeta3(opt, typeClean, default, assemblyName)))} }}" : "default");
                        return sb.Append(")").ToString();
                    }
                case ITypeSymbol accessor:
                    {
                        var fullName = symbol.GetFullNameStandardFormat();

                        //if (AnalysisMeta.AnalysisInfo.BasrType.TryGetValue(fullName, out string v))
                        //{
                        //    return v;
                        //}

                        var key = $"{symbol.GetFullNameStandardFormat(GetFullNameOpt.Create(noNullableQuestionMark: true, charSpecial: CharSpecial.Create('_')))}";

                        if (AnalysisMeta.AnalysisInfo.AccessorType.ContainsKey(fullName))
                        {
                            return $"{accessorTypes2}{key}.Singleton";
                        }

                        AnalysisMeta.AnalysisInfo.AccessorType.TryAdd(fullName, default);

                        var runtimeType = GetRuntimeType(accessor, typeClean);
                        var isTupleType = accessor.IsTupleType && !accessor.IsDefinition;
                        var fullName2 = isTupleType ? symbol.GetFullNameRealStandardFormat(GetFullNameOpt.Create(tupleStyle: TupleStyle.TypeName), typeClean: typeClean) : symbol.GetFullNameRealStandardFormat(typeClean: typeClean);

                        #region members

                        var members = new Dictionary<string, string>();
                        var methods = new Dictionary<string, IList<IMethodSymbol>>();

                        foreach (var item in accessor.GetMembers())
                        {
                            if (Accessibility.Public != item.DeclaredAccessibility)
                            {
                                continue;
                            }

                            switch (item)
                            {
                                case IMethodSymbol member:
                                    if (!symbol.DeclaringSyntaxReferences.Any()) { continue; }
                                    if (MethodKind.Ordinary == member.MethodKind)
                                    {
                                        if (!methods.TryGetValue(member.Name, out IList<IMethodSymbol> m))
                                        {
                                            methods.Add(member.Name, new List<IMethodSymbol> { member });
                                        }
                                        else
                                        {
                                            m.Add(member);
                                        }
                                    }
                                    break;
                                case IFieldSymbol member:
                                    {
                                        if (member.IsObsolete() || member.IsImplicitlyDeclared || member.Type.IsRefLikePointerTypedReferenceValueTypeConstraint()) { continue; }

                                        if (member.Type.Kind is SymbolKind.Event) { continue; }

                                        if (member.Type.IsValueTypeConstraint()) { continue; }

                                        members.Add(member.Name, member.ToMeta3(opt, typeClean, fullName2, assemblyName));
                                    }
                                    break;
                                case IPropertySymbol member:
                                    {
                                        if (member.IsObsolete() || member.IsImplicitlyDeclared || member.Type.IsRefLikePointerTypedReferenceValueTypeConstraint() || member.IsIndexer) { continue; }

                                        if (member.Type.Kind is SymbolKind.Event) { continue; }

                                        if (member.Type.IsValueTypeConstraint()) { continue; }

                                        members.Add(member.Name, member.ToMeta3(opt, typeClean, fullName2, assemblyName));
                                    }
                                    break;
                                case IEventSymbol member:
                                    {
                                        if (!symbol.DeclaringSyntaxReferences.Any() || member.IsObsolete()) { continue; }
                                        if (member.Type.IsRefLikePointerTypedReferenceValueTypeConstraint()) { continue; }

                                        if (member.Type.IsValueTypeConstraint()) { continue; }

                                        members.Add(member.Name, member.ToMeta3(opt, typeClean, fullName2, assemblyName));
                                    }
                                    break;
                                default: break;
                            }
                        }

                        foreach (var item in methods)
                        {
                            if (1 < item.Value.Count)
                            {
                                var method = item.Value.ToMeta3(opt, typeClean, fullName2, assemblyName);

                                if (method is null)
                                {
                                    continue;
                                }

                                members.Add(item.Key, method);
                            }
                            else
                            {
                                var method = item.Value.First().ToMeta3(opt, typeClean, fullName2, assemblyName);

                                if (method is null)
                                {
                                    continue;
                                }

                                members.Add(item.Key, method);
                            }
                        }

                        #endregion

                        #region Members

                        var sb = new System.Text.StringBuilder();
                        int spaceCount = 0;

                        if (!string.IsNullOrEmpty(assemblyName))
                        {
                            sb.AppendFormat("namespace {1}{0}", $"{format}{{{format}", accessorTypes);
                            spaceCount++;
                        }

                        sb.SetSummary(space.Repeat(spaceCount), key);
                        sb.AppendLine($"{space.Repeat(spaceCount)}public readonly struct {key} : {globalMeta}IAccessorType");
                        sb.AppendLine($"{space.Repeat(spaceCount)}{{");
                        spaceCount++;
                        sb.AppendLine($"{space.Repeat(spaceCount)}readonly static {globalLazy}<{accessorTypes2}{key}> instance = new {globalLazy}<{accessorTypes2}{key}>(() => new {accessorTypes2}{key}());");
                        sb.SetSummary(space.Repeat(spaceCount), "Singleton");
                        sb.AppendLine($"public static {accessorTypes2}{key} Singleton => instance.Value;");
                        //sb.AppendLine($"{space.Repeat(spaceCount)}public {key}() {{ }}");
                        sb.SetSummary(space.Repeat(spaceCount), "ToString()");
                        sb.AppendLine($"{space.Repeat(spaceCount)}public override {globalString} ToString() => $\"{{Kind}}\";");
                        //==================Meta==================//
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsExtern", symbol.IsExtern.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsSealed", symbol.IsSealed.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsAbstract", symbol.IsAbstract.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsOverride", symbol.IsOverride.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsVirtual", symbol.IsVirtual.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsStatic", symbol.IsStatic.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), $"{globalMeta}Kind", "Kind", $"{globalMeta}Kind.{symbol.Kind}");
                        //==================IAccessorType==================//
                        sb.AddProperty(space.Repeat(spaceCount), $"{globalGeneric}IDictionary<{globalString}, {globalMeta}IAccessor>", "Members", members.Any() ? $"new {globalGeneric}Dictionary<{globalString}, {globalMeta}IAccessor> {{ {string.Join(", ", members.Select(c => $"{{ \"{c.Key}\", {c.Value} }}"))} }}" : "default");
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsReadOnly", accessor.IsReadOnly.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsUnmanagedType", accessor.IsUnmanagedType.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), $"{globalMeta}SpecialType", "SpecialType", $"{globalMeta}SpecialType.{accessor.SpecialType}");
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsTupleType", accessor.IsTupleType.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsAnonymousType", accessor.IsAnonymousType.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), $"{globalMeta}TypeKind", "TypeKind", $"{globalMeta}TypeKind.{accessor.TypeKind}");
                        sb.AddProperty(space.Repeat(spaceCount), globalBoolean, "IsRecord", accessor.IsRecord.ToName());
                        sb.AddProperty(space.Repeat(spaceCount), $"{globalMeta}AsyncType", "AsyncType", $"{globalMeta}AsyncType.{GetAsyncType(accessor)}");
                        //sb.AddProperty(space.Repeat(spaceCount), globalString, "Name", $"\"{fullName}\"");
                        sb.AddProperty(space.Repeat(spaceCount), globalType, "RuntimeType", !(runtimeType is null) ? $"typeof({runtimeType})" : "default");
                        sb.AddProperty(space.Repeat(spaceCount), $"{globalMeta}DefaultValue", "DefaultValue", GetDefaultValue(accessor, typeClean, globalSystem));

                        spaceCount--;
                        sb.Append($"{space.Repeat(spaceCount)}}}");
                        if (!string.IsNullOrEmpty(assemblyName))
                        {
                            sb.AppendLine();
                            spaceCount--;
                            sb.Append($"{space.Repeat(spaceCount)}}}");
                        }

                        #endregion

                        if (AnalysisMeta.AnalysisInfo.AccessorType.ContainsKey(fullName))
                        {
                            AnalysisMeta.AnalysisInfo.AccessorType[fullName] = sb.ToString();
                        }

                        return $"{accessorTypes2}{key}.Singleton";
                    }
                case IFieldSymbol accessor:
                    {
                        //var fullName = symbol.GetFullNameStandardFormat();
                        var symbolName = "event" == symbol.Name ? $"@{symbol.Name}" : symbol.Name;
                        var type = accessor.Type;
                        var typeFullName = type.GetFullNameRealStandardFormat(typeClean: typeClean);

                        string getValue = default;
                        string setValue = default;

                        var name = $"{receiverType}.{symbolName}";
                        var castName = $"(({receiverType})obj).{symbolName}";
                        getValue = symbol.IsStatic ? $"() => {name}" : $"obj => {castName}";

                        if (!accessor.IsReadOnly && !accessor.IsConst)
                        {
                            var value = TypeKind.TypeParameter == type.TypeKind ? $"({typeFullName})value" : type.IsValueType ? $"({typeFullName})value" : $"value as {typeFullName.TrimEnd('?')}";

                            if (symbol.IsStatic)
                            {
                                setValue = $"value => {name} = {value}";
                            }
                            else
                            {
                                setValue = $"(ref {globalMeta}InstanceMeta obj, {globalObject} value) => {{ var obj2 = ({receiverType})obj.Instance; obj2.{symbolName} = {value}; obj = new {globalMeta}InstanceMeta(obj.TypeMeta, obj2); }}";
                            }
                        }

                        var attrs = accessor.GetAttributes().Where(c => !(c.ApplicationSyntaxReference is null) && c.AttributeClass.DeclaredAccessibility is Accessibility.Public);
                        var sb = new System.Text.StringBuilder($"new {globalMeta}AccessorField(");
                        //==================Meta==================//
                        sb.AppendFormat(accessor.IsExtern);
                        sb.AppendFormat(accessor.IsSealed);
                        sb.AppendFormat(accessor.IsAbstract);
                        sb.AppendFormat(accessor.IsOverride);
                        sb.AppendFormat(accessor.IsVirtual);
                        sb.AppendFormat(accessor.IsStatic);
                        sb.AppendFormat($"{globalMeta}Kind.{{0}}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", attrs.Any() ? $"{GetAttributes(attrs, globalMeta, typeClean)}" : "default");
                        //==================IAccessorMember==================//
                        sb.AppendFormat(accessor.IsReadOnly);
                        sb.AppendFormat("{0}, ", type.ToMeta3(opt, typeClean, default, assemblyName));
                        //==================IAccessorField==================//
                        sb.AppendFormat(accessor.IsConst);
                        sb.AppendFormat(accessor.IsVolatile);
                        sb.AppendFormat(accessor.HasConstantValue);
                        sb.AppendFormat("{0}, ", accessor.HasConstantValue ? name : "default");
                        sb.AppendFormat(accessor.IsExplicitlyNamedTupleElement);
                        //==================Get, Set==================//
                        sb.AppendFormat("{0}, ", !symbol.IsStatic && !string.IsNullOrEmpty(getValue) ? getValue : "default");
                        sb.AppendFormat("{0}, ", !symbol.IsStatic && !string.IsNullOrEmpty(setValue) ? setValue : "default");
                        sb.AppendFormat("{0}, ", symbol.IsStatic && !string.IsNullOrEmpty(getValue) ? getValue : "default");
                        sb.AppendFormat("{0}", symbol.IsStatic && !string.IsNullOrEmpty(setValue) ? setValue : "default");
                        return sb.Append(")").ToString();
                    }
                case IPropertySymbol accessor:
                    {
                        //var fullName = symbol.GetFullNameStandardFormat();
                        var symbolName = "event" == symbol.Name ? $"@{symbol.Name}" : symbol.Name;
                        var type = accessor.Type;
                        var typeFullName = type.GetFullNameRealStandardFormat(typeClean: typeClean);

                        string getValue = default;
                        string setValue = default;

                        var name = $"{receiverType}.{symbolName}";
                        var castName = $"(({receiverType})obj).{symbolName}";

                        if (!accessor.IsWriteOnly && !(accessor.GetMethod is null) && accessor.GetMethod.DeclaredAccessibility is Accessibility.Public && !accessor.GetMethod.IsInitOnly)
                        {
                            getValue = symbol.IsStatic ? $"() => {name}" : $"obj => {castName}";
                        }

                        if (!accessor.IsReadOnly && !(accessor.SetMethod is null) && accessor.SetMethod.DeclaredAccessibility is Accessibility.Public && !accessor.SetMethod.IsInitOnly)
                        {
                            var value = TypeKind.TypeParameter == type.TypeKind ? $"({typeFullName})value" : type.IsValueType ? $"({typeFullName})value" : $"value as {typeFullName.TrimEnd('?')}";

                            if (symbol.IsStatic)
                            {
                                setValue = $"value => {name} = {value}";
                            }
                            else
                            {
                                setValue = $"(ref {globalMeta}InstanceMeta obj, {globalObject} value) => {{ var obj2 = ({receiverType})obj.Instance; obj2.{symbolName} = {value}; obj = new {globalMeta}InstanceMeta(obj.TypeMeta, obj2); }}";
                            }
                        }

                        var attrs = accessor.GetAttributes().Where(c => !(c.ApplicationSyntaxReference is null) && c.AttributeClass.DeclaredAccessibility is Accessibility.Public);
                        var sb = new System.Text.StringBuilder($"new {globalMeta}AccessorProperty(");
                        //==================Meta==================//
                        sb.AppendFormat(accessor.IsExtern);
                        sb.AppendFormat(accessor.IsSealed);
                        sb.AppendFormat(accessor.IsAbstract);
                        sb.AppendFormat(accessor.IsOverride);
                        sb.AppendFormat(accessor.IsVirtual);
                        sb.AppendFormat(accessor.IsStatic);
                        sb.AppendFormat($"{globalMeta}Kind.{{0}}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", attrs.Any() ? $"{GetAttributes(attrs, globalMeta, typeClean)}" : "default");
                        //==================IAccessorMember==================//
                        sb.AppendFormat(accessor.IsReadOnly);
                        sb.AppendFormat("{0}, ", type.ToMeta3(opt, typeClean, default, assemblyName));
                        //==================IAccessorProperty==================//
                        sb.AppendFormat($"{globalMeta}RefKind.{{0}}, ", accessor.RefKind.GetName());
                        sb.AppendFormat(accessor.IsWriteOnly);
                        //==================Get, Set==================//
                        sb.AppendFormat("{0}, ", !symbol.IsStatic && !string.IsNullOrEmpty(getValue) ? getValue : "default");
                        sb.AppendFormat("{0}, ", !symbol.IsStatic && !string.IsNullOrEmpty(setValue) ? setValue : "default");
                        sb.AppendFormat("{0}, ", symbol.IsStatic && !string.IsNullOrEmpty(getValue) ? getValue : "default");
                        sb.AppendFormat("{0}", symbol.IsStatic && !string.IsNullOrEmpty(setValue) ? setValue : "default");
                        return sb.Append(")").ToString();
                    }
                case IParameterSymbol accessor:
                    {
                        var attrs = accessor.GetAttributes().Where(c => !(c.ApplicationSyntaxReference is null) && c.AttributeClass.DeclaredAccessibility is Accessibility.Public);

                        var sb = new System.Text.StringBuilder($"new {globalMeta}AccessorParameter(");
                        //==================Meta==================//
                        sb.AppendFormat($"{globalMeta}Kind.{{0}}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", attrs.Any() ? $"{GetAttributes(attrs, globalMeta, typeClean)}" : "default");
                        //==================IAccessorParameter==================//
                        sb.AppendFormat(accessor.IsOptional);
                        sb.AppendFormat(accessor.IsThis);
                        sb.AppendFormat(accessor.IsDiscard);
                        sb.AppendFormat("{0}, ", accessor.Type.ToMeta3(opt, typeClean, default, assemblyName));
                        sb.AppendFormat($"{globalMeta}RefKind.{{0}}, ", accessor.RefKind.GetName());
                        sb.AppendFormat("{0}, ", accessor.Ordinal);
                        sb.AppendFormat(accessor.HasExplicitDefaultValue);
                        sb.AppendFormat("{0}, ", ToDefaultValue(accessor));
                        sb.AppendFormat(ImplicitDefaultValue(accessor));
                        sb.AppendFormat(accessor.IsParams, true);
                        return sb.Append(")").ToString();
                    }
                case IMethodSymbol accessor:
                    {
                        if (accessor.IsObsolete())
                        {
                            return default;
                        }
                        if (accessor.Parameters.Any(c => c.Type.IsRefLikePointerTypedReferenceValueTypeConstraint()))
                        {
                            return default;
                        }
                        if (accessor.ReturnType.IsRefLikePointerTypedReferenceValueTypeConstraint())
                        {
                            return default;
                        }
                        if (accessor.TypeParameters.Any(c => c.HasValueTypeConstraint))
                        {
                            return default;
                        }

                        var getValue = GetValueCode(accessor, receiverType, false, typeClean, opt);
                        var getValueAsync = GetValueCode(accessor, receiverType, true, typeClean, opt);

                        if (getValue?.Contains("*") ?? false)
                        {
                            return default;
                        }

                        var attrs = accessor.GetAttributes().Where(c => !(c.ApplicationSyntaxReference is null) && c.AttributeClass.DeclaredAccessibility is Accessibility.Public);
                        var typeParameters = accessor.TypeParameters.ToDictionary(c => c.GetFullNameStandardFormat(), c => c);

                        #region Members

                        var sb = new System.Text.StringBuilder($"new {globalMeta}AccessorMethod(");
                        //==================Meta==================//
                        sb.AppendFormat(accessor.IsExtern);
                        sb.AppendFormat(accessor.IsSealed);
                        sb.AppendFormat(accessor.IsAbstract);
                        sb.AppendFormat(accessor.IsOverride);
                        sb.AppendFormat(accessor.IsVirtual);
                        sb.AppendFormat(accessor.IsStatic);
                        sb.AppendFormat($"{globalMeta}Kind.{{0}}, ", accessor.Kind.GetName());
                        //==================IAccessorMethod==================//
                        sb.AppendFormat("{0}, ", attrs.Any() ? $"{GetAttributes(attrs, globalMeta, typeClean)}" : "default");
                        sb.AppendFormat(accessor.IsPartialDefinition);
                        sb.AppendFormat("{0}, ", accessor.TypeParameters.Any() ? $"new {globalGeneric}Dictionary<{globalString}, {globalMeta}IAccessorTypeParameter> {{ {string.Join(", ", typeParameters.Select(c => $"{{ \"{c.Key}\", {c.Value.ToMeta3(opt, typeClean, default, assemblyName)} }}"))} }}" : "default");
                        sb.AppendFormat($"{globalMeta}MethodKind.{{0}}, ", accessor.MethodKind.GetName());
                        sb.AppendFormat(accessor.IsGenericMethod);
                        sb.AppendFormat(accessor.IsExtensionMethod);
                        sb.AppendFormat(accessor.IsAsync);
                        sb.AppendFormat(accessor.ReturnsVoid);
                        sb.AppendFormat($"{globalMeta}RefKind.{{0}}, ", accessor.RefKind.GetName());
                        sb.AppendFormat("{0}, ", accessor.ReturnType?.ToMeta3(opt, typeClean, default, assemblyName) ?? "default");
                        //Parameters
                        sb.AppendFormat("{0}, ", accessor.Parameters.Any() ? $"new {globalMeta}IAccessorParameter[] {{ {string.Join(", ", accessor.Parameters.Select(c => c.ToMeta3(opt, typeClean, default, assemblyName)))} }}" : "default");
                        //ParametersRealLength
                        sb.AppendFormat("{0}, ", accessor.Parameters.Length);
                        //ParametersMustLength
                        sb.AppendFormat("{0}, ", accessor.Parameters.Count(c => !c.HasExplicitDefaultValue && !c.IsParams));
                        sb.AppendFormat("{0}, ", getValue);
                        sb.AppendFormat("{0}", getValueAsync);
                        return sb.Append(")").ToString();

                        #endregion
                    }
                case IEventSymbol accessor:
                    {
                        var attrs = accessor.GetAttributes().Where(c => !(c.ApplicationSyntaxReference is null) && c.AttributeClass.DeclaredAccessibility is Accessibility.Public);

                        var sb = new System.Text.StringBuilder($"new {globalMeta}AccessorEvent(");
                        //==================Meta==================//
                        sb.AppendFormat(accessor.IsExtern);
                        sb.AppendFormat(accessor.IsSealed);
                        sb.AppendFormat(accessor.IsAbstract);
                        sb.AppendFormat(accessor.IsOverride);
                        sb.AppendFormat(accessor.IsVirtual);
                        sb.AppendFormat(accessor.IsStatic);
                        sb.AppendFormat($"{globalMeta}Kind.{{0}}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", attrs.Any() ? $"{GetAttributes(attrs, globalMeta, typeClean)}" : "default");
                        //==================IAccessorMember==================//
                        sb.Append("false, ");
                        sb.AppendFormat("{0}, ", accessor.Type.ToMeta3(opt, typeClean, default, assemblyName));
                        //==================IAccessorEvent==================//
                        sb.AppendFormat(accessor.IsWindowsRuntimeEvent);
                        sb.AppendFormat("{0}, ", accessor.AddMethod?.GetEventCode($"{accessor.Name} += ", receiverType, typeClean, opt) ?? "default");
                        sb.AppendFormat("{0}", accessor.RemoveMethod?.GetEventCode($"{accessor.Name} -= ", receiverType, typeClean, opt) ?? "default");
                        return sb.Append(")").ToString();
                    }
                default: return default;
            }
        }

        public static string GetAttributes(IEnumerable<AttributeData> attributes, string globalMeta, Func<string, bool, string> typeClean)
        {
            var attr = attributes.Select(c =>
            {
                var args = c.AttributeConstructor?.Parameters.Select(c2 => GetAttributeArgument(c.ConstructorArguments[c2.Ordinal], c2.Name, globalMeta, typeClean));

                var namedArgs = c.NamedArguments.Select(c2 => GetAttributeArgument(c2.Value, c2.Key, globalMeta, typeClean));

                return $"new {globalMeta}AccessorAttribute(\"{c.AttributeClass.Name}\", typeof({c.AttributeClass.GetFullNameStandardFormat(typeClean: typeClean)}), {((args?.Any() ?? false) ? $"new {globalMeta}TypedConstant[] {{ {string.Join(", ", args)} }}" : "default")}, {((namedArgs?.Any() ?? false) ? $"new {globalMeta}TypedConstant[] {{ {string.Join(", ", namedArgs)} }}" : "default")})";
            });

            return $"new {globalMeta}AccessorAttribute[] {{ {string.Join(", ", attr)} }}";
        }

        public static string GetAttributeArgument(TypedConstant typedConstant, string name, string globalMeta, Func<string, bool, string> typeClean) => $"new {globalMeta}TypedConstant(" +
            $"\"{name}\", " +
            $"typeof({typedConstant.Type.GetFullNameStandardFormat(typeClean: typeClean)}), " +
            $"{(typedConstant.IsNull ? "true" : "default")}, " +
            $"{globalMeta}TypedConstantKind.{typedConstant.Kind.GetName()}, " +
            $"{(typedConstant.Type is IArrayTypeSymbol array ? typedConstant.Values.Any() ? $"new {array.ElementType.GetFullNameStandardFormat(typeClean: typeClean)}[] {typedConstant.ToCSharpString()}" : "default" : typedConstant.ToCSharpString())})";

        public static void SetSummary(this System.Text.StringBuilder sb, string space, string v)
        {
            sb.AppendLine($"{space}/// <summary>");
            sb.AppendLine($"{space}/// {v}");
            sb.AppendLine($"{space}/// </summary>");
        }

        static void AddProperty(this System.Text.StringBuilder sb, string space, string type, string name, string value)
        {
            SetSummary(sb, space, name);
            sb.AppendLine($"{space}public {type} {name} => {value};");
        }
    }
}
