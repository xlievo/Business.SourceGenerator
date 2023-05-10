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
    using System.Collections.Specialized;
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

        const int accessorDepth = 4;

        static string ToDefaultValue(ISymbol symbol)
        {
            SpecialType specialType = SpecialType.None;
            object value = default;

            switch (symbol)
            {
                case IFieldSymbol fieldSymbol:
                    if (!fieldSymbol.HasConstantValue)
                    {
                        return "default";
                    }

                    specialType = fieldSymbol.Type.SpecialType;
                    value = fieldSymbol.ConstantValue;
                    break;
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

        public static string GetConstructor(IMethodSymbol constructorMethod, IDictionary<string, ITypeSymbol> typeArguments, Func<string, bool, string> typeClean, ToCodeOpt opt)
        {
            var globalSystem = opt.GetGlobalName(GlobalName.System);
            var globalMeta = opt.GetGlobalName(GlobalName.Business_SourceGenerator_Meta);
            var globalObject = opt.GetGlobalName(GlobalName.System_Object);

            var mustLength = 0;

            var parameterList = constructorMethod.Parameters.Select(c =>
            {
                if (!c.HasExplicitDefaultValue && !c.IsParams)
                {
                    mustLength++;
                }

                if (c.Type.SpecialType is SpecialType.System_Object || c.Type.TypeKind is TypeKind.Dynamic)
                {
                    return ToMetaParameter(globalMeta, c.Name, globalObject, TypeKind.Unknown, c.RefKind, c.Type.IsValueType, c.Ordinal, c.HasExplicitDefaultValue, ToDefaultValue(c), ImplicitDefaultValue(c));
                }

                var type = (typeArguments?.Any() ?? false) && typeArguments.TryGetValue(c.Type.GetFullName(), out ITypeSymbol typeSymbol) ? typeSymbol : c.Type;

                var typeName = $"{type.GetFullNameRealStandardFormat(typeClean: typeClean)}";

                var typeKind = type.SpecialType is SpecialType.None ? type.TypeKind : TypeKind.Unknown;

                return ToMetaParameter(globalMeta, c.Name, typeName, typeKind, c.RefKind, type.IsValueType, c.Ordinal, c.HasExplicitDefaultValue, ToDefaultValue(c), ImplicitDefaultValue(c), c.Type.HasGenericType());
            }).ToArray();

            const string methodParameterRefVarName = "args";
            const string methodParameterVarName = "m{0}.v";

            var sign = constructorMethod.GetFullNameMethodRealStandardFormat(GetFullNameOpt.Create(captureStyle: CaptureStyle.NoName, methodParameterVarName: methodParameterVarName, methodParameterRefVarName: methodParameterRefVarName, methodParameterVarNameCallback: MethodParameterVarNameCallback, typeArguments: typeArguments, global: opt.Global), typeClean: typeClean);

            var refs = constructorMethod.Parameters.GetMethodRef(methodParameterVarName, methodParameterRefVarName, typeClean, opt);

            return $"new {globalMeta}Constructor({constructorMethod.Parameters.Length}, {mustLength}, {(parameterList.Any() ? $"new {globalMeta}IParameterMeta[] {{ {string.Join(", ", parameterList)} }}" : "default")}, (obj, m, {methodParameterRefVarName}) => {{ {refs.refs}var result = new {sign}; {refs.refs2}return result; }})";
        }

        static bool ImplicitDefaultValue(IParameterSymbol parameterSymbol) => (parameterSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is ParameterSyntax parameter && parameter.Default?.Value is LiteralExpressionSyntax literalExpression && literalExpression.Token.IsKind(SyntaxKind.DefaultKeyword)) || parameterSymbol.IsParams;

        static string MethodParameterVarNameCallback(string typeName, int ordinal) => $"!m[{ordinal}].c ? {typeName} : default";

        static string GetValueCode(IMethodSymbol method, string receiverType, bool asTask, Func<string, bool, string> typeClean, ToCodeOpt opt)
        {
            var globalTasks = opt.GetGlobalName(GlobalName.System_Threading_Tasks);
            var globalObject = opt.GetGlobalName(GlobalName.System_Object);
            var globalSourceGenerator = opt.GetGlobalName(GlobalName.Business_SourceGenerator);
            var globalMeta = opt.GetGlobalName(GlobalName.Business_SourceGenerator_Meta);
            var globalSystem = opt.GetGlobalName(GlobalName.System);

            const string methodParameterRefVarName = "args";
            //!m[3].c ? global::Sl::System.ValueTuple<global::System.Int32?, global::System.String>)m[1].v; ystem.Convert.ToSingle(m[3].v) : default
            const string methodParameterVarName = "m{0}.v";
            const string methodParameterDeclaration = "(obj, m, args)";
            //var m{0}0 = m[0].v as global::System.String; var m{0}1 = (global::System.ValueTuple<global::System.Int32?, global::System.String>)m[1].v; 
            var sign = method.GetFullNameMethodRealStandardFormat(GetFullNameOpt.Create(captureStyle: CaptureStyle.NoPrefix, methodParameterVarName: methodParameterVarName, methodParameterRefVarName: methodParameterRefVarName, methodParameterVarNameCallback: MethodParameterVarNameCallback, global: opt.Global), typeClean: typeClean);

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
                    result = $"async {methodParameterDeclaration} => {{ {refs.refs}{result}; {refs.refs2}return default; }}";
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
                            result = $"async {methodParameterDeclaration} => {{ {refs.refs}var result = {result}; {refs.refs2}return result; }}";
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
                        //case AsyncType.Other: break;
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

        public static string ToMetaParameter(string globalMeta, string name, string type, TypeKind typeKind = TypeKind.Unknown, RefKind refKind = RefKind.None, bool isValueType = default, int ordinal = default, bool hasExplicitDefaultValue = default, string explicitDefaultValue = "default", bool implicitDefaultValue = default, bool hasGenericType = default) => $"new {globalMeta}Parameter(\"{name}\", typeof({type}), {globalMeta}TypeKind.{typeKind.GetName()},{globalMeta}RefKind.{refKind.GetName()}, {(isValueType ? "true" : "default")}, {ordinal}, {(hasExplicitDefaultValue ? "true" : "default")}, {explicitDefaultValue}, {(implicitDefaultValue ? "true" : "default")}, {(hasGenericType ? "true" : "default")})";

        public static string ToMeta(this ISymbol symbol, ToCodeOpt opt, int depth = default, string receiverType = default, StringCollection types = default, Func<string, bool, string> typeClean = default)
        {
            var globalMeta = opt.GetGlobalName(GlobalName.Business_SourceGenerator_Meta);
            var globalSystem = opt.GetGlobalName(GlobalName.System);
            var globalGeneric = opt.GetGlobalName(GlobalName.System_Collections_Generic);
            var globalString = opt.GetGlobalName(GlobalName.System_String);
            var globalInt32 = opt.GetGlobalName(GlobalName.System_Int32);
            var globalObject = opt.GetGlobalName(GlobalName.System_Object);
            var globalType = opt.GetGlobalName(GlobalName.System_Type);

            switch (symbol)
            {
                case INamedTypeSymbol accessor:
                    {
                        depth++;
                        var isDeclaringSyntaxReferences = symbol.DeclaringSyntaxReferences.Any();
                        var skip = accessorDepth < depth || SpecialType.None != accessor.SpecialType;
                        var skip2 = skip || !isDeclaringSyntaxReferences;
                        var fullName = symbol.GetFullNameStandardFormat(typeClean: typeClean);
                        var attrs = accessor.GetAttributes().Where(c => !(c.ApplicationSyntaxReference is null));

                        if (null == types)
                        {
                            types = new StringCollection { fullName };
                        }

                        #region members

                        var members = skip2 ? default : new Dictionary<string, string>();
                        var methods = skip2 ? default : new Dictionary<string, IList<IMethodSymbol>>();

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
                                            if (member.Parameters.Any(c => c.Type.IsPointerType()))
                                            {
                                                continue;
                                            }
                                            if (!methods.TryGetValue(member.Name, out IList<IMethodSymbol> m))
                                            {
                                                methods.Add(member.Name, new List<IMethodSymbol> { member });
                                            }
                                            else
                                            {
                                                m.Add(member);
                                            }
                                            //if (!members.ContainsKey(member.Name))
                                            //{
                                            //    members.Add(member.Name, member.ToMeta(opt, depth, fullName, typeClean: typeClean));
                                            //}
                                            //else
                                            //{
                                            //    members.Add(member.GetFullNameStandardFormat(), member.ToMeta(opt, depth, fullName, typeClean: typeClean));
                                            //}
                                        }
                                        break;
                                    case IFieldSymbol member:
                                        if (member.IsImplicitlyDeclared) { continue; }
                                        if (member.Type.IsPointerType())
                                        {
                                            continue;
                                        }
                                        members.Add(member.Name, member.ToMeta(opt, depth, fullName, types, typeClean)); break;
                                    case IPropertySymbol member:
                                        if (member.IsImplicitlyDeclared) { continue; }
                                        if (member.Type.IsPointerType())
                                        {
                                            continue;
                                        }
                                        members.Add(member.Name, member.ToMeta(opt, depth, fullName, types, typeClean)); break;
                                    case IEventSymbol member:
                                        if (member.Type.IsPointerType()) { continue; }
                                        members.Add(member.Name, member.ToMeta(opt, depth, fullName, types, typeClean)); break;
                                    default: break;
                                }
                            }

                            foreach (var item in methods)
                            {
                                if (1 < item.Value.Count)
                                {
                                    members.Add(item.Key, item.Value.ToMeta(opt, depth, fullName, typeClean));
                                }
                                else
                                {
                                    members.Add(item.Key, item.Value.First().ToMeta(opt, depth, fullName, typeClean: typeClean));
                                }
                            }
                        }

                        #endregion

                        var sb = new System.Text.StringBuilder($"new {globalMeta}AccessorNamedType(");
                        //==================Meta==================//
                        sb.AppendFormat($"{globalMeta}Accessibility.{{0}}, ", accessor.DeclaredAccessibility.GetName());
                        //sb.AppendFormat("{0}, ", accessor.CanBeReferencedByName ? "true" : "default");
                        //sb.AppendFormat("{0}, ", accessor.IsImplicitlyDeclared ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsExtern ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsSealed ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsAbstract ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsOverride ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsVirtual ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsStatic ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsDefinition ? "true" : "default");
                        sb.AppendFormat("\"{0}\", ", accessor.Name);
                        sb.AppendFormat("\"{0}\", ", symbol.GetFullNameStandardFormat());
                        sb.AppendFormat($"{globalMeta}Kind.{{0}}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", isDeclaringSyntaxReferences ? "true" : "default");
                        sb.AppendFormat("{0}, ", attrs.Any() ? $"{GetAttributes(attrs, globalMeta, typeClean)}" : "default");
                        //==================IAccessorType==================//
                        //sb.AppendFormat("{0}, ", accessor.IsNamespace ? "true" : "default");
                        //sb.AppendFormat("{0}, ", accessor.IsType ? "true" : "default");

                        /* Members */
                        sb.AppendFormat("{0}, ", !(members?.Any() ?? false) ? "default" : $"new {globalGeneric}Dictionary<{globalString}, {globalMeta}IAccessor> {{ {string.Join(", ", members.Select(c => $"{{ \"{c.Key}\", {c.Value} }}"))} }}");
                        /* IsReferenceType
                        sb.AppendFormat("{0}, ", accessor.IsReferenceType ? "true" : "default"); */
                        /* IsReadOnly */
                        sb.AppendFormat("{0}, ", accessor.IsReadOnly ? "true" : "default");
                        /* IsUnmanagedType */
                        sb.AppendFormat("{0}, ", accessor.IsUnmanagedType ? "true" : "default");
                        /* IsRefLikeType
                        sb.AppendFormat("{0}, ", accessor.IsRefLikeType ? "true" : "default"); */
                        /* SpecialType */
                        sb.AppendFormat($"{globalMeta}SpecialType.{{0}}, ", accessor.SpecialType.GetName());
                        /* IsNativeIntegerType
                        sb.AppendFormat("{0}, ", accessor.IsNativeIntegerType ? "true" : "default"); */
                        /* IsTupleType */
                        sb.AppendFormat("{0}, ", accessor.IsTupleType ? "true" : "default");
                        /* IsAnonymousType */
                        sb.AppendFormat("{0}, ", accessor.IsAnonymousType ? "true" : "default");
                        /* IsValueType
                        sb.AppendFormat("{0}, ", accessor.IsValueType ? "true" : "default"); */
                        /* NullableAnnotation */
                        sb.AppendFormat($"{globalMeta}NullableAnnotation.{{0}}, ", accessor.NullableAnnotation.GetName());
                        ///* AllInterfaces */
                        //sb.AppendFormat("{0}, ", (skip2 || !accessor.AllInterfaces.Any()) ? "default" : $"new {globalType}[] {{ {string.Join(", ", accessor.AllInterfaces.Where(c => c.DeclaredAccessibility is Accessibility.Public).Select(c => $"typeof({(c.TypeChecked(t => t is ITypeParameterSymbol typeParameter && typeParameter.TypeParameterKind is TypeParameterKind.Method) ? c.GetFullNameRealStandardFormat(typeClean: typeClean) : c.GetFullNameStandardFormat(typeClean: typeClean))})"))} }}");
                        ///* BaseType */
                        //sb.AppendFormat("{0}, ", accessor.BaseType is null ? "default" : $"typeof({accessor.BaseType.GetFullNameStandardFormat(typeClean: typeClean)})");
                        /* TypeKind */
                        sb.AppendFormat($"{globalMeta}TypeKind.{{0}}, ", accessor.TypeKind.GetName());
                        /* IsRecord */
                        sb.AppendFormat("{0}, ", accessor.IsRecord ? "true" : "default");
                        /* AsyncType */
                        sb.AppendFormat($"{globalMeta}AsyncType.{{0}}, ", GetAsyncType(accessor).GetName());
                        //==================IAccessorNamedType==================//
                        sb.AppendFormat("{0}, ", (skip2 || !accessor.TypeArgumentNullableAnnotations.Any()) ? "default" : $"new {globalMeta}NullableAnnotation[] {{ {string.Join(", ", accessor.TypeArgumentNullableAnnotations.Select(c => $"{globalMeta}NullableAnnotation.{c.GetName()}"))} }}");
                        sb.AppendFormat("{0}, ", (skip2 || null == accessor.TupleElements || !accessor.TupleElements.Any()) ? "default" : $"new {globalMeta}IAccessorField[] {{ {string.Join(", ", accessor.TupleElements.Select(c => c.ToMeta(opt, depth, accessor.Name, typeClean: typeClean)))} }}");
                        sb.AppendFormat("{0}, ", accessor.MightContainExtensionMethods ? "true" : "default");
                        sb.AppendFormat("{0}, ", (skip2 || !accessor.Constructors.Any()) ? "default" : $"new {globalMeta}IAccessorMethod[] {{ {string.Join(", ", accessor.Constructors.Select(c => c.ToMeta(opt, depth, typeClean: typeClean)))} }}");
                        sb.AppendFormat("{0}, ", (skip2 || accessor.EnumUnderlyingType is null) ? "default" : accessor.EnumUnderlyingType.ToMeta(opt, depth, typeClean: typeClean));
                        sb.AppendFormat("{0}, ", (skip2 || accessor.DelegateInvokeMethod is null) ? "default" : accessor.DelegateInvokeMethod.ToMeta(opt, depth, typeClean: typeClean));
                        sb.AppendFormat("{0}, ", accessor.IsSerializable ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsPartial() ? "true" : "default");
                        sb.AppendFormat("{0}, ", (skip || !accessor.TypeParameters.Any()) ? "default" : $"new {globalGeneric}Dictionary<{globalString}, {globalMeta}IAccessorTypeParameter> {{ {string.Join(", ", accessor.TypeParameters.Select(c => $"{{ \"{c.GetFullNameStandardFormat()}\", {c.ToMeta(opt, depth, typeClean: typeClean)} }}"))} }}");
                        //sb.Append((MemberNames?.Any() ?? false) ? $"new string[] {{ {string.Join(", ", MemberNames.Select(c => $"\"{c}\""))} }}, " : "default, ");
                        //sb.AppendFormat("{0}, ", accessor.IsUnboundGenericType ? "true" : "default");
                        //sb.AppendFormat("{0}, ", accessor.IsGenericType ? "true" : "default");
                        sb.AppendFormat("{0}", (skip || !accessor.TypeArguments.Any()) ? "default" : $"new {globalMeta}IAccessorType[] {{ {string.Join(", ", accessor.TypeArguments.Select(c => c.ToMeta(opt, depth, typeClean: typeClean)))} }}");
                        return sb.Append(")").ToString();
                    }
                case ITypeParameterSymbol accessor:
                    {
                        depth++;
                        var isDeclaringSyntaxReferences = symbol.DeclaringSyntaxReferences.Any();
                        var skip = accessorDepth < depth || SpecialType.None != accessor.SpecialType;
                        var skip2 = skip || !isDeclaringSyntaxReferences;
                        var fullName = symbol.GetFullName(GetFullNameOpt.Create(true, typeParameterStyle: TypeParameterStyle.FullName));
                        var attrs = accessor.GetAttributes().Where(c => !(c.ApplicationSyntaxReference is null));

                        var sb = new System.Text.StringBuilder($"new {globalMeta}AccessorTypeParameter(");
                        //==================Meta==================//
                        sb.AppendFormat($"{globalMeta}Accessibility.{{0}}, ", accessor.DeclaredAccessibility.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsExtern ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsSealed ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsAbstract ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsOverride ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsVirtual ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsStatic ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsDefinition ? "true" : "default");
                        sb.AppendFormat("\"{0}\", ", accessor.GetFullNameStandardFormat());
                        sb.AppendFormat("\"{0}\", ", fullName);
                        sb.AppendFormat($"{globalMeta}Kind.{{0}}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", isDeclaringSyntaxReferences ? "true" : "default");
                        sb.AppendFormat("{0}, ", attrs.Any() ? $"{GetAttributes(attrs, globalMeta, typeClean)}" : "default");
                        //==================IAccessorType==================//
                        //sb.AppendFormat("{0}, ", accessor.IsNamespace ? "true" : "default");
                        //sb.AppendFormat("{0}, ", accessor.IsType ? "true" : "default");
                        sb.Append("default, ");
                        /* sb.AppendFormat("{0}, ", accessor.IsReferenceType ? "true" : "default"); */
                        sb.AppendFormat("{0}, ", accessor.IsReadOnly ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsUnmanagedType ? "true" : "default");
                        /* sb.AppendFormat("{0}, ", accessor.IsRefLikeType ? "true" : "default"); */
                        sb.AppendFormat($"{globalMeta}SpecialType.{{0}}, ", accessor.SpecialType.GetName());
                        /* sb.AppendFormat("{0}, ", accessor.IsNativeIntegerType ? "true" : "default"); */
                        sb.AppendFormat("{0}, ", accessor.IsTupleType ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsAnonymousType ? "true" : "default");
                        /* sb.AppendFormat("{0}, ", accessor.IsValueType ? "true" : "default"); */
                        sb.AppendFormat($"{globalMeta}NullableAnnotation.{{0}}, ", accessor.NullableAnnotation.GetName());
                        //sb.AppendFormat("{0}, ", (skip2 || !accessor.AllInterfaces.Any()) ? "default" : $"new {globalType}[] {{ {string.Join(", ", accessor.AllInterfaces.Where(c => c.DeclaredAccessibility is Accessibility.Public).Select(c => $"typeof({(c.TypeChecked(t => t is ITypeParameterSymbol typeParameter && typeParameter.TypeParameterKind is TypeParameterKind.Method) ? c.GetFullNameRealStandardFormat(typeClean: typeClean) : c.GetFullNameStandardFormat(typeClean: typeClean))})"))} }}");
                        //sb.AppendFormat("{0}, ", accessor.BaseType is null ? "default" : $"typeof({accessor.BaseType.GetFullNameStandardFormat(typeClean: typeClean)})");
                        sb.AppendFormat($"{globalMeta}TypeKind.{{0}}, ", accessor.TypeKind.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsRecord ? "true" : "default");
                        sb.AppendFormat($"{globalMeta}AsyncType.{{0}}, ", GetAsyncType(accessor).GetName());
                        //==================IAccessorTypeParameter==================//
                        sb.AppendFormat("{0}, ", accessor.Ordinal);
                        sb.AppendFormat($"{globalMeta}VarianceKind.{{0}}, ", accessor.Variance.GetName());
                        sb.AppendFormat($"{globalMeta}TypeParameterKind.{{0}}, ", accessor.TypeParameterKind.GetName());
                        sb.AppendFormat("{0}, ", accessor.HasReferenceTypeConstraint ? "true" : "default");
                        sb.AppendFormat($"{globalMeta}NullableAnnotation.{{0}}, ", accessor.ReferenceTypeConstraintNullableAnnotation.GetName());
                        sb.AppendFormat("{0}, ", accessor.HasValueTypeConstraint ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.HasUnmanagedTypeConstraint ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.HasNotNullConstraint ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.HasConstructorConstraint ? "true" : "default");
                        sb.Append((skip2 || !accessor.ConstraintTypes.Any()) ? "default" : $"new {globalMeta}IAccessorType[] {{ {string.Join(", ", accessor.ConstraintTypes.Select(c => c.ToMeta(opt, depth, typeClean: typeClean)))} }}");
                        return sb.Append(")").ToString();
                    }
                case ITypeSymbol accessor:
                    {
                        switch (symbol)
                        {
                            case INamedTypeSymbol typeSymbol: return ToMeta(typeSymbol, opt, depth, typeClean: typeClean);
                            case ITypeParameterSymbol typeSymbol: return ToMeta(typeSymbol, opt, depth, typeClean: typeClean);
                            default: break;
                        }

                        depth++;
                        var isDeclaringSyntaxReferences = symbol.DeclaringSyntaxReferences.Any();
                        var skip = accessorDepth < depth || SpecialType.None != accessor.SpecialType;
                        var skip2 = skip || !isDeclaringSyntaxReferences;
                        var fullName = symbol.GetFullNameStandardFormat();
                        var attrs = accessor.GetAttributes().Where(c => !(c.ApplicationSyntaxReference is null));

                        if (null == types)
                        {
                            types = new StringCollection { fullName };
                        }

                        #region members

                        var members = skip2 ? default : new Dictionary<string, string>();
                        var methods = skip2 ? default : new Dictionary<string, IList<IMethodSymbol>>();

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
                                            if (member.Parameters.Any(c => c.Type.IsPointerType()))
                                            {
                                                continue;
                                            }
                                            if (!methods.TryGetValue(member.Name, out IList<IMethodSymbol> m))
                                            {
                                                methods.Add(member.Name, new List<IMethodSymbol> { member });
                                            }
                                            else
                                            {
                                                m.Add(member);
                                            }
                                            //if (!members.ContainsKey(member.Name))
                                            //{
                                            //    members.Add(member.Name, member.ToMeta(opt, depth, fullName, typeClean: typeClean));
                                            //}
                                            //else
                                            //{
                                            //    members.Add(member.GetFullNameStandardFormat(), member.ToMeta(opt, depth, fullName, typeClean: typeClean));
                                            //}
                                        }
                                        break;
                                    case IFieldSymbol member:
                                        if (member.IsImplicitlyDeclared) { continue; }
                                        if (member.Type.IsPointerType())
                                        {
                                            continue;
                                        }
                                        members.Add(member.Name, member.ToMeta(opt, depth, fullName, types, typeClean)); break;
                                    case IPropertySymbol member:
                                        if (member.IsImplicitlyDeclared) { continue; }
                                        if (member.Type.IsPointerType())
                                        {
                                            continue;
                                        }
                                        members.Add(member.Name, member.ToMeta(opt, depth, fullName, types, typeClean)); break;
                                    case IEventSymbol member:
                                        if (member.Type.IsPointerType()) { continue; }
                                        members.Add(member.Name, member.ToMeta(opt, depth, fullName, types, typeClean: typeClean)); break;
                                    default: break;
                                }
                            }

                            foreach (var item in methods)
                            {
                                if (1 < item.Value.Count)
                                {
                                    members.Add(item.Key, item.Value.ToMeta(opt, depth, fullName, typeClean));
                                }
                                else
                                {
                                    members.Add(item.Key, item.Value.First().ToMeta(opt, depth, fullName, types, typeClean: typeClean));
                                }
                            }
                        }

                        #endregion

                        var sb = new System.Text.StringBuilder($"new {globalMeta}AccessorType(");
                        //==================Meta==================//
                        sb.AppendFormat($"{globalMeta}Accessibility.{{0}}, ", accessor.DeclaredAccessibility.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsExtern ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsSealed ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsAbstract ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsOverride ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsVirtual ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsStatic ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsDefinition ? "true" : "default");
                        sb.AppendFormat("\"{0}\", ", accessor.Name);
                        sb.AppendFormat("\"{0}\", ", fullName);
                        sb.AppendFormat($"{globalMeta}Kind.{{0}}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", isDeclaringSyntaxReferences ? "true" : "default");
                        sb.AppendFormat("{0}, ", attrs.Any() ? $"{GetAttributes(attrs, globalMeta, typeClean)}" : "default");
                        //==================IAccessorType==================//
                        //sb.AppendFormat("{0}, ", accessor.IsNamespace ? "true" : "default");
                        //sb.AppendFormat("{0}, ", accessor.IsType ? "true" : "default");
                        sb.AppendFormat("{0}, ", !(members?.Any() ?? false) ? "default" : $"new {globalGeneric}Dictionary<{globalString}, {globalMeta}IAccessor> {{ {string.Join(", ", members.Select(c => $"{{ \"{c.Key}\", {c.Value} }}"))} }}");
                        /* sb.AppendFormat("{0}, ", accessor.IsReferenceType ? "true" : "default"); */
                        sb.AppendFormat("{0}, ", accessor.IsReadOnly ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsUnmanagedType ? "true" : "default");
                        /* sb.AppendFormat("{0}, ", accessor.IsRefLikeType ? "true" : "default"); */
                        sb.AppendFormat($"{globalMeta}SpecialType.{{0}}, ", accessor.SpecialType.GetName());
                        /* sb.AppendFormat("{0}, ", accessor.IsNativeIntegerType ? "true" : "default"); */
                        sb.AppendFormat("{0}, ", accessor.IsTupleType ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsAnonymousType ? "true" : "default");
                        /* sb.AppendFormat("{0}, ", accessor.IsValueType ? "true" : "default"); */
                        sb.AppendFormat($"{globalMeta}NullableAnnotation.{{0}}, ", accessor.NullableAnnotation.GetName());
                        //sb.AppendFormat("{0}, ", (skip2 || !accessor.AllInterfaces.Any()) ? "default" : $"new {globalType}[] {{ {string.Join(", ", accessor.AllInterfaces.Where(c => c.DeclaredAccessibility is Accessibility.Public).Select(c => $"typeof({(c.TypeChecked(t => t is ITypeParameterSymbol typeParameter && typeParameter.TypeParameterKind is TypeParameterKind.Method) ? c.GetFullNameRealStandardFormat(typeClean: typeClean) : c.GetFullNameStandardFormat(typeClean: typeClean))})"))} }}");
                        //sb.AppendFormat("{0}, ", accessor.BaseType is null ? "default" : $"typeof({accessor.BaseType.GetFullNameStandardFormat(typeClean: typeClean)})");
                        sb.AppendFormat($"{globalMeta}TypeKind.{{0}}, ", accessor.TypeKind.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsRecord ? "true" : "default");
                        sb.AppendFormat($"{globalMeta}AsyncType.{{0}}", GetAsyncType(accessor).GetName());
                        return sb.Append(")").ToString();
                    }
                case IParameterSymbol accessor:
                    {
                        depth++;
                        var isDeclaringSyntaxReferences = symbol.DeclaringSyntaxReferences.Any();
                        var skip = accessorDepth < depth;
                        var fullName = symbol.GetFullNameStandardFormat();
                        var attrs = accessor.GetAttributes().Where(c => !(c.ApplicationSyntaxReference is null));

                        var sb = new System.Text.StringBuilder($"new {globalMeta}AccessorParameter(");
                        //==================Meta==================//
                        sb.AppendFormat($"{globalMeta}Accessibility.{{0}}, ", accessor.DeclaredAccessibility.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsExtern ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsSealed ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsAbstract ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsOverride ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsVirtual ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsStatic ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsDefinition ? "true" : "default");
                        sb.AppendFormat("\"{0}\", ", accessor.Name);
                        sb.AppendFormat("\"{0}\", ", fullName);
                        sb.AppendFormat($"{globalMeta}Kind.{{0}}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", isDeclaringSyntaxReferences ? "true" : "default");
                        sb.AppendFormat("{0}, ", attrs.Any() ? $"{GetAttributes(attrs, globalMeta, typeClean)}" : "default");
                        //==================IAccessorParameter==================//
                        sb.AppendFormat($"{globalMeta}RefKind.{{0}}, ", accessor.RefKind.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsParams ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsOptional ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsThis ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsDiscard ? "true" : "default");
                        sb.AppendFormat("{0}, ", skip ? "default" : accessor.Type.ToMeta(opt, depth, typeClean: typeClean));
                        sb.AppendFormat($"{globalMeta}NullableAnnotation.{{0}}, ", accessor.NullableAnnotation.GetName());
                        sb.AppendFormat("{0}, ", accessor.Ordinal);
                        sb.AppendFormat("{0}, ", accessor.HasExplicitDefaultValue ? "true" : "default");
                        sb.AppendFormat("{0}, ", ToDefaultValue(accessor));
                        sb.AppendFormat("{0}, ", ImplicitDefaultValue(accessor) ? "true" : "default");

                        sb.AppendFormat("{0}, ", $"typeof({accessor.Type.GetFullNameRealStandardFormat(typeClean: typeClean)})");
                        sb.AppendFormat($"{globalMeta}TypeKind.{{0}}, ", accessor.Type.TypeKind.GetName());
                        sb.AppendFormat("{0}, ", accessor.Type.IsValueType ? "true" : "default");
                        sb.Append(accessor.Type.HasGenericType() ? "true" : "default");
                        return sb.Append(")").ToString();
                    }
                case IMethodSymbol accessor:
                    {
                        depth++;
                        var isDeclaringSyntaxReferences = symbol.DeclaringSyntaxReferences.Any();
                        var skip = accessorDepth < depth;
                        var fullName = symbol.GetFullNameStandardFormat();
                        var attrs = accessor.GetAttributes().Where(c => !(c.ApplicationSyntaxReference is null));

                        var typeParameters = skip ? default : accessor.TypeParameters.ToDictionary(c => c.GetFullNameStandardFormat(), c => c);

                        var skip2 = string.IsNullOrEmpty(receiverType) || 2 < depth;
                        var getValue = !skip2 ? GetValueCode(accessor, $"{receiverType}", false, typeClean, opt) : default;
                        var getValueAsync = !skip2 ? GetValueCode(accessor, $"{receiverType}", true, typeClean, opt) : default;

                        var sb = new System.Text.StringBuilder($"new {globalMeta}AccessorMethod(");
                        //==================Meta==================//
                        sb.AppendFormat($"{globalMeta}Accessibility.{{0}}, ", accessor.DeclaredAccessibility.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsExtern ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsSealed ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsAbstract ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsOverride ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsVirtual ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsStatic ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsDefinition ? "true" : "default");
                        sb.AppendFormat("\"{0}\", ", accessor.Name);
                        sb.AppendFormat("\"{0}\", ", fullName);
                        sb.AppendFormat($"{globalMeta}Kind.{{0}}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", isDeclaringSyntaxReferences ? "true" : "default");
                        sb.AppendFormat("{0}, ", attrs.Any() ? $"{GetAttributes(attrs, globalMeta, typeClean)}" : "default");
                        //==================IAccessorMethod==================//
                        sb.AppendFormat("{0}, ", accessor.IsReadOnly ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsInitOnly ? "true" : "default");
                        //Parameters
                        sb.AppendFormat("{0}, ", (skip || !accessor.Parameters.Any()) ? "default" : $"new {globalMeta}IAccessorParameter[] {{ {string.Join(", ", accessor.Parameters.Select(c => c.ToMeta(opt, depth, typeClean: typeClean)))} }}");
                        sb.AppendFormat("{0}, ", accessor.IsPartialDefinition ? "true" : "default");
                        sb.AppendFormat("{0}, ", (skip || !accessor.TypeParameters.Any()) ? "default" : $"new {globalGeneric}Dictionary<{globalString}, {globalMeta}IAccessorTypeParameter> {{ {string.Join(", ", typeParameters.Select(c => $"{{ \"{c.Key}\", {c.Value.ToMeta(opt, depth, typeClean: typeClean)} }}"))} }}");
                        sb.AppendFormat("{0}, ", accessor.IsConditional ? "true" : "default");
                        sb.AppendFormat($"{globalMeta}MethodKind.{{0}}, ", accessor.MethodKind.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsGenericMethod ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsExtensionMethod ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsAsync ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.ReturnsVoid ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.ReturnsByRef ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.ReturnsByRefReadonly ? "true" : "default");
                        sb.AppendFormat($"{globalMeta}RefKind.{{0}}, ", accessor.RefKind.GetName());
                        sb.AppendFormat("{0}, ", skip ? "default" : accessor.ReturnType?.ToMeta(opt, depth, typeClean: typeClean));
                        sb.AppendFormat("{0}, ", accessor.HidesBaseMethodsByName ? "true" : "default");
                        //sb.AppendFormat("{0}, ", isClone ? "true" : "default");
                        sb.AppendFormat("{0}, ", skip2 ? "default" : getValue);
                        sb.AppendFormat("{0}, ", skip2 ? "default" : getValueAsync);

                        //ParametersRealLength
                        sb.AppendFormat("{0}, ", accessor.Parameters.Length);
                        //ParametersMustLength
                        sb.Append(accessor.Parameters.Count(c => !c.HasExplicitDefaultValue && !c.IsParams));

                        /*
                        #region refOrdinal

                        var refOrdinal = 0;
                        var refOrdinalList = new List<int>();
                        foreach (var item in accessor.Parameters)
                        {
                            switch (item.RefKind)
                            {
                                case RefKind.Ref: refOrdinalList.Add(refOrdinal++); break;
                                case RefKind.Out: break;
                                default: refOrdinal++; break;
                            }
                        }
                        //ParametersRefOrdinal
                        sb.Append(!refOrdinalList.Any() ? "default" : $"new {globalInt32}[] {{ {string.Join(", ", refOrdinalList)} }}");

                        #endregion
                        */
                        return sb.Append(")").ToString();
                    }
                case IFieldSymbol accessor:
                    {
                        depth++;
                        var isDeclaringSyntaxReferences = symbol.DeclaringSyntaxReferences.Any();
                        var skip = accessorDepth < depth;
                        var fullName = symbol.GetFullNameStandardFormat();
                        var attrs = accessor.GetAttributes().Where(c => !(c.ApplicationSyntaxReference is null));

                        var isRepeat = types?.Contains(fullName) ?? false;
                        if (!isRepeat && null != types)
                        {
                            types.Add(fullName);
                        }

                        var type = accessor.Type;
                        var typeFullName = type.GetFullNameStandardFormat();

                        string getValue = default;
                        string setValue = default;
                        if (!string.IsNullOrEmpty(receiverType) && 2 >= depth)
                        {
                            var name = $"{receiverType}.{symbol.Name}";
                            var castName = $"(({receiverType})obj).{symbol.Name}";

                            getValue = symbol.IsStatic ? $"obj => {name}" : $"obj => {castName}";
                            var value = TypeKind.TypeParameter == type.TypeKind ? $"({type.Name})value" : type.IsValueType ? $"({typeClean(typeFullName, false)})value" : TypeKind.Dynamic == type.TypeKind ? $"value as {typeClean(typeFullName.TrimEnd('?'), false)}" : $"value as {typeClean(typeFullName.TrimEnd('?'), false)}";

                            if (symbol.IsStatic)
                            {
                                setValue = $"(ref {globalMeta}IGeneratorAccessor obj, {globalObject} value) => {name} = {value}";
                            }
                            else
                            {
                                setValue = $"(ref {globalMeta}IGeneratorAccessor obj, {globalObject} value) => {{ var obj2 = ({receiverType})obj; obj2.{symbol.Name} = {value}; obj = obj2; }}";
                            }
                        }

                        var sb = new System.Text.StringBuilder($"new {globalMeta}AccessorField(");
                        //==================Meta==================//
                        sb.AppendFormat($"{globalMeta}Accessibility.{{0}}, ", accessor.DeclaredAccessibility.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsExtern ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsSealed ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsAbstract ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsOverride ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsVirtual ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsStatic ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsDefinition ? "true" : "default");
                        sb.AppendFormat("\"{0}\", ", accessor.Name);
                        sb.AppendFormat("\"{0}\", ", fullName);
                        sb.AppendFormat($"{globalMeta}Kind.{{0}}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", isDeclaringSyntaxReferences ? "true" : "default");
                        sb.AppendFormat("{0}, ", attrs.Any() ? $"{GetAttributes(attrs, globalMeta, typeClean)}" : "default");
                        //==================IAccessorMember==================//
                        sb.AppendFormat("{0}, ", accessor.IsReadOnly ? "true" : "default");
                        sb.AppendFormat("{0}, ", skip ? "default" : type.ToMeta(opt, depth, types: types, typeClean: typeClean));
                        sb.AppendFormat("\"{0}\", ", typeFullName);
                        //==================IAccessorField==================//
                        sb.AppendFormat("{0}, ", accessor.IsConst ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsVolatile ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.HasConstantValue ? "true" : "default");
                        //sb.AppendObject(accessor.ConstantValue, ", ");
                        sb.AppendFormat("{0}, ", ToDefaultValue(accessor));
                        sb.AppendFormat("{0}, ", accessor.IsExplicitlyNamedTupleElement ? "true" : "default");

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
                        var attrs = accessor.GetAttributes().Where(c => !(c.ApplicationSyntaxReference is null));

                        var isRepeat = types?.Contains(fullName) ?? false;
                        if (!isRepeat && null != types)
                        {
                            types.Add(fullName);
                        }

                        var type = accessor.Type;
                        var typeFullName = type.GetFullNameStandardFormat();

                        string getValue = default;
                        string setValue = default;
                        if (!string.IsNullOrEmpty(receiverType) && 2 >= depth && !accessor.IsReadOnly)
                        {
                            var name = $"{receiverType}.{symbol.Name}";
                            var castName = $"(({receiverType})obj).{symbol.Name}";
                            getValue = symbol.IsStatic ? $"obj => {name}" : $"obj => {castName}";
                            var value = TypeKind.TypeParameter == type.TypeKind ? $"({type.Name})value" : type.IsValueType ? $"({typeClean(typeFullName, false)})value" : TypeKind.Dynamic == type.TypeKind ? $"value as {typeClean(typeFullName.TrimEnd('?'), false)}" : $"value as {typeClean(typeFullName.TrimEnd('?'), false)}";

                            if (symbol.IsStatic)
                            {
                                setValue = $"(ref {globalMeta}IGeneratorAccessor obj, {globalObject} value) => {name} = {value}";
                            }
                            else
                            {
                                setValue = $"(ref {globalMeta}IGeneratorAccessor obj, {globalObject} value) => {{ var obj2 = ({receiverType})obj; obj2.{symbol.Name} = {value}; obj = obj2; }}";
                            }
                        }

                        var sb = new System.Text.StringBuilder($"new {globalMeta}AccessorProperty(");
                        //==================Meta==================//
                        sb.AppendFormat($"{globalMeta}Accessibility.{{0}}, ", accessor.DeclaredAccessibility.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsExtern ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsSealed ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsAbstract ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsOverride ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsVirtual ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsStatic ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsDefinition ? "true" : "default");
                        sb.AppendFormat("\"{0}\", ", accessor.Name);
                        sb.AppendFormat("\"{0}\", ", fullName);
                        sb.AppendFormat($"{globalMeta}Kind.{{0}}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", isDeclaringSyntaxReferences ? "true" : "default");
                        sb.AppendFormat("{0}, ", attrs.Any() ? $"{GetAttributes(attrs, globalMeta, typeClean)}" : "default");
                        //==================IAccessorMember==================//
                        sb.AppendFormat("{0}, ", accessor.IsReadOnly ? "true" : "default");
                        sb.AppendFormat("{0}, ", skip ? "default" : type.ToMeta(opt, depth, types: types, typeClean: typeClean));
                        sb.AppendFormat("\"{0}\", ", typeFullName);
                        //==================IAccessorProperty==================//
                        sb.AppendFormat($"{globalMeta}RefKind.{{0}}, ", accessor.RefKind.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsWriteOnly ? "true" : "default");

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
                        var attrs = accessor.GetAttributes().Where(c => !(c.ApplicationSyntaxReference is null));

                        var isRepeat = types?.Contains(fullName) ?? false;
                        if (!isRepeat && null != types)
                        {
                            types.Add(fullName);
                        }

                        var type = accessor.Type;
                        var typeFullName = type.GetFullNameStandardFormat();

                        string getValue = default;
                        string setValue = default;
                        if (!string.IsNullOrEmpty(receiverType) && 2 >= depth)
                        {
                            var name = $"{receiverType}.{symbol.Name}";
                            var castName = $"(({receiverType})obj).{symbol.Name}";
                            getValue = symbol.IsStatic ? $"obj => {name}" : $"obj => {castName}";
                        }

                        var sb = new System.Text.StringBuilder($"new {globalMeta}AccessorEvent(");
                        //==================Meta==================//
                        sb.AppendFormat($"{globalMeta}Accessibility.{{0}}, ", accessor.DeclaredAccessibility.GetName());
                        sb.AppendFormat("{0}, ", accessor.IsExtern ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsSealed ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsAbstract ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsOverride ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsVirtual ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsStatic ? "true" : "default");
                        sb.AppendFormat("{0}, ", accessor.IsDefinition ? "true" : "default");
                        sb.AppendFormat("\"{0}\", ", accessor.Name);
                        sb.AppendFormat("\"{0}\", ", fullName);
                        sb.AppendFormat($"{globalMeta}Kind.{{0}}, ", accessor.Kind.GetName());
                        sb.AppendFormat("{0}, ", isDeclaringSyntaxReferences ? "true" : "default");
                        sb.AppendFormat("{0}, ", attrs.Any() ? $"{GetAttributes(attrs, globalMeta, typeClean)}" : "default");
                        //==================IAccessorMember==================//
                        sb.Append("false, ");
                        sb.AppendFormat("{0}, ", skip ? "default" : type.ToMeta(opt, depth, types: types, typeClean: typeClean));
                        sb.AppendFormat("\"{0}\", ", typeFullName);
                        //==================IAccessorEvent==================//
                        sb.AppendFormat("{0}, ", accessor.IsWindowsRuntimeEvent ? "true" : "default");
                        sb.AppendFormat("{0}, ", skip ? "default" : accessor.AddMethod?.ToMeta(opt, typeClean: typeClean) ?? "default");
                        sb.AppendFormat("{0}, ", skip ? "default" : accessor.RemoveMethod?.ToMeta(opt, typeClean: typeClean) ?? "default");

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

        public static string ToMeta(this IEnumerable<IMethodSymbol> methodSymbols, ToCodeOpt opt, int depth, string receiverType, Func<string, bool, string> typeClean)
        {
            var globalMeta = opt.GetGlobalName(GlobalName.Business_SourceGenerator_Meta);

            var collection = string.Join(", ", methodSymbols.Select(c => c.ToMeta(opt, depth, receiverType, typeClean: typeClean)));

            return $"new {globalMeta}AccessorMethodCollection(new {globalMeta}IAccessorMethod[] {{ {collection} }})";
        }

        public static string GetAttributes(IEnumerable<AttributeData> attributes, string globalMeta, Func<string, bool, string> typeClean)
        {
            var attr = attributes.Select(c =>
            {
                var args = GetAttributeArguments(c, globalMeta, typeClean);
                return $"new {globalMeta}AccessorAttribute(\"{c.AttributeClass.Name}\", {((args?.Any() ?? false) ? $"new {globalMeta}TypedConstant[] {{ {string.Join(", ", args)} }}" : "default")})";
            });

            return $"new {globalMeta}AccessorAttribute[] {{ {string.Join(", ", attr)} }}";
        }

        public static IEnumerable<string> GetAttributeArguments(AttributeData attribute, string globalMeta, Func<string, bool, string> typeClean) => attribute.AttributeConstructor?.Parameters.Select(c =>
        {
            var v = attribute.ConstructorArguments[c.Ordinal];

            return $"new {globalMeta}TypedConstant(" +
            $"\"{c.Name}\", " +
            $"typeof({c.Type.GetFullNameStandardFormat(typeClean: typeClean)}), " +
            $"{(v.IsNull ? "true" : "default")}, " +
            $"{globalMeta}TypedConstantKind.{v.Kind.GetName()}, " +
            $"{(c.Type is IArrayTypeSymbol array ? v.Values.Any() ? $"new {array.ElementType.GetFullNameStandardFormat(typeClean: typeClean)}[] {v.ToCSharpString()}" : "default" : v.ToCSharpString())})";
        });
    }
}
