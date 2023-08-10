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
    using Business.SourceGenerator.Meta;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public static partial class Utils
    {
        static Lazy<IGeneratorCode> generatorCode = default;

        /// <summary>
        /// Global IGeneratorCode.
        /// <para>According to the compilation order, the entry assembly will be compiled last.</para>
        /// </summary>
        public static IGeneratorCode GeneratorCode => generatorCode.Value;

        /// <summary>
        /// Specify the global IGeneratorCode.
        /// </summary>
        /// <param name="generatorType"></param>
        public static void SetGeneratorCode(this IGeneratorCode generatorType) => generatorCode = new Lazy<IGeneratorCode>(() => generatorType);

        #region AccessorGet

        /// <summary>
        /// Gets the property or field value of a specified object.
        /// </summary>
        /// <typeparam name="Type">Returns the caller of the specified type.</typeparam>
        /// <param name="instanceMeta">The object whose value will be get.</param>
        /// <param name="name">property or field name.</param>
        /// <returns>Return member value.</returns>
        public static Type AccessorGet<Type>(this InstanceMeta instanceMeta, string name)
        {
            if (!(instanceMeta.TypeMeta.AsGeneratorType(name, out IAccessor accessor) is AccessorException.No) || !(accessor is IAccessorMember member) || member.IsStatic || member.GetValue is null)
            {
                return default;
            }

            return (Type)member.GetValue(instanceMeta.Instance);
        }

        /// <summary>
        /// Gets the property or field value of a specified object.
        /// </summary>
        /// <typeparam name="Type">Returns the caller of the specified type.</typeparam>
        /// <param name="typeMeta">The object whose value will be get.</param>
        /// <param name="name">property or field name.</param>
        /// <returns></returns>
        public static Type AccessorGet<Type>(this TypeMeta typeMeta, string name)
        {
            if (!(typeMeta.AsGeneratorType(name, out IAccessor accessor) is AccessorException.No) || !(accessor is IAccessorMember member) || !member.IsStatic || member.GetValueStatic is null)
            {
                return default;
            }

            return (Type)member.GetValueStatic();
        }

        /// <summary>
        /// Gets the property or field value of a specified object.
        /// </summary>
        /// <typeparam name="Type">Returns the caller of the specified type.</typeparam>
        /// <param name="typeMeta">The object whose value will be get.</param>
        /// <param name="name">property or field name.</param>
        /// <param name="instance">The object whose property or field value will be returned.</param>
        /// <returns></returns>
        public static Type AccessorGet<Type>(this TypeMeta typeMeta, string name, object instance)
        {
            if (instance is null)
            {
                throw AccessorException.InstanceNull.AsException();
            }

            if (!(typeMeta.AsGeneratorType(name, out IAccessor accessor) is AccessorException.No) || !(accessor is IAccessorMember member) || member.IsStatic || member.GetValue is null)
            {
                return default;
            }

            return (Type)member.GetValue(instance);
        }

        #endregion

        #region AccessorSet

        /// <summary>
        /// Sets the property or field value of a specified object.
        /// </summary>
        /// <typeparam name="Type">Returns the caller of the specified type.</typeparam>
        /// <param name="instanceMeta">The object whose value will be set.</param>
        /// <param name="name">property or field name.</param>
        /// <param name="value">The new value.</param>
        /// <returns>Caller of specified type.</returns>
        public static Type AccessorSet<Type>(this InstanceMeta instanceMeta, string name, object value) => (Type)AccessorSet(instanceMeta, name, value).Instance;

        /// <summary>
        /// Sets the property or field value of a specified object.
        /// </summary>
        /// <param name="instanceMeta">The object whose value will be set.</param>
        /// <param name="name">property or field name.</param>
        /// <param name="value">The new value.</param>
        /// <returns>IGeneratorAccessor.</returns>
        public static InstanceMeta AccessorSet(this InstanceMeta instanceMeta, string name, object value)
        {
            if (!(instanceMeta.TypeMeta.AsGeneratorType(name, out IAccessor accessor) is AccessorException.No) || !(accessor is IAccessorMember member) || member.IsStatic || member.SetValue is null)
            {
                return default;
            }

            member.SetValue(ref instanceMeta, value);
            return instanceMeta;
        }

        /// <summary>
        /// Sets the property or field value of a specified object.
        /// </summary>
        /// <param name="typeMeta">The object whose value will be set.</param>
        /// <param name="name">property or field name.</param>
        /// <param name="value">The new value.</param>
        /// <returns>IGeneratorAccessor.</returns>
        public static TypeMeta AccessorSet(this TypeMeta typeMeta, string name, object value)
        {
            if (!(typeMeta.AsGeneratorType(name, out IAccessor accessor) is AccessorException.No) || !(accessor is IAccessorMember member) || !member.IsStatic || member.SetValueStatic is null)
            {
                return default;
            }

            member.SetValueStatic(value);
            return typeMeta;
        }

        /// <summary>
        /// Sets the property or field value of a specified object.
        /// </summary>
        /// <param name="typeMeta">The object whose value will be set.</param>
        /// <param name="name">property or field name.</param>
        /// <param name="value">The new value.</param>
        /// <param name="instance">The object whose property or field value will be set.</param>
        /// <returns>IGeneratorAccessor.</returns>
        public static TypeMeta AccessorSet(this TypeMeta typeMeta, string name, object value, object instance)
        {
            if (instance is null)
            {
                throw AccessorException.InstanceNull.AsException();
            }

            if (!(typeMeta.AsGeneratorType(name, out IAccessor accessor) is AccessorException.No) || !(accessor is IAccessorMember member) || member.IsStatic || member.SetValue is null)
            {
                return default;
            }

            var instanceMeta = new InstanceMeta(typeMeta, instance);
            member.SetValue(ref instanceMeta, value);
            return typeMeta;
        }

        #endregion

        #region AccessorMethod TypeMeta

        /// <summary>
        /// Call the specified method to obtain the return object and out parameters.
        /// </summary>
        /// <typeparam name="ResultType"></typeparam>
        /// <param name="typeMeta">Metadata of the specified type.</param>
        /// <param name="name">Method name.</param>
        /// <param name="result">Return value, obtained as an out declaration.</param>
        /// <param name="instance">The object on which to invoke the method or constructor.</param>
        /// <param name="args">Method parameter array.</param>
        /// <returns></returns>
        public static bool AccessorMethod<ResultType>(this TypeMeta typeMeta, string name, out ResultType result, object instance = default, params object[] args)
        {
            var success = AccessorMethod(new InstanceMeta(typeMeta, instance), name, out object result2, false, args);

            result = success ? (ResultType)result2 : default;

            return success;
        }

        /// <summary>
        /// Call the specified method to obtain the return object and out parameters.
        /// </summary>
        /// <param name="typeMeta">Metadata of the specified type.</param>
        /// <param name="name">Method name.</param>
        /// <param name="result">Return value, obtained as an out declaration.</param>
        /// <param name="instance">The object on which to invoke the method or constructor.</param>
        /// <param name="args">Method parameter array.</param>
        /// <returns></returns>
        public static bool AccessorMethod(this TypeMeta typeMeta, string name, out object result, object instance = default, params object[] args) => AccessorMethod(new InstanceMeta(typeMeta, instance), name, out result, false, args);

        /// <summary>
        /// Call the specified method to obtain the return object and out parameters.
        /// </summary>
        /// <typeparam name="ResultType"></typeparam>
        /// <param name="typeMeta">Metadata of the specified type.</param>
        /// <param name="name">Method name.</param>
        /// <param name="result">Return value, obtained as an out declaration.</param>
        /// <param name="instance">The object on which to invoke the method or constructor.</param>
        /// <param name="args">Method parameter array.</param>
        /// <returns></returns>
        public static bool AccessorMethodSkipGeneric<ResultType>(this TypeMeta typeMeta, string name,  out ResultType result, object instance = default, params object[] args)
        {
            var success = AccessorMethod(new InstanceMeta(typeMeta, instance), name, out object result2, true, args);

            result = success ? (ResultType)result2 : default;

            return success;
        }

        /// <summary>
        /// Call the specified method to obtain the return object and out parameters.
        /// </summary>
        /// <param name="typeMeta">Metadata of the specified type.</param>
        /// <param name="name">Method name.</param>
        /// <param name="result">Return value, obtained as an out declaration.</param>
        /// <param name="instance">The object on which to invoke the method or constructor.</param>
        /// <param name="args">Method parameter array</param>
        /// <returns></returns>
        public static bool AccessorMethodSkipGeneric(this TypeMeta typeMeta, string name,  out object result, object instance = default, params object[] args) => AccessorMethod(new InstanceMeta(typeMeta, instance), name, out result, true, args);

        #endregion

        #region AccessorMethod InstanceMeta

        /// <summary>
        /// Call the specified method to obtain the return object and out parameters.
        /// </summary>
        /// <typeparam name="ResultType"></typeparam>
        /// <param name="instanceMeta">Metadata of the specified type.</param>
        /// <param name="name">Method name.</param>
        /// <param name="result">Return value, obtained as an out declaration.</param>
        /// <param name="args">Method parameter array.</param>
        /// <returns></returns>
        public static bool AccessorMethod<ResultType>(this InstanceMeta instanceMeta, string name, out ResultType result, params object[] args)
        {
            var success = AccessorMethod(instanceMeta, name, out object result2, false, args);

            result = success ? (ResultType)result2 : default;

            return success;
        }

        /// <summary>
        /// Call the specified method to obtain the return object and out parameters.
        /// </summary>
        /// <param name="instanceMeta">Metadata of the specified type.</param>
        /// <param name="name">Method name.</param>
        /// <param name="result">Return value, obtained as an out declaration.</param>
        /// <param name="args">Method parameter array.</param>
        /// <returns></returns>
        public static bool AccessorMethod(this InstanceMeta instanceMeta, string name, out object result, params object[] args) => AccessorMethod(instanceMeta, name, out result, false, args);

        /// <summary>
        /// Call the specified method to obtain the return object and out parameters.
        /// </summary>
        /// <typeparam name="ResultType"></typeparam>
        /// <param name="instanceMeta">Metadata of the specified type.</param>
        /// <param name="name">Method name.</param>
        /// <param name="result">Return value, obtained as an out declaration.</param>
        /// <param name="args">Method parameter array.</param>
        /// <returns></returns>
        public static bool AccessorMethodSkipGeneric<ResultType>(this InstanceMeta instanceMeta, string name, out ResultType result, params object[] args)
        {
            var success = AccessorMethod(instanceMeta, name, out object result2, true, args);

            result = success ? (ResultType)result2 : default;

            return success;
        }

        /// <summary>
        /// Call the specified method to obtain the return object and out parameters.
        /// </summary>
        /// <param name="instanceMeta">Metadata of the specified type.</param>
        /// <param name="name">Method name.</param>
        /// <param name="result">Return value, obtained as an out declaration.</param>
        /// <param name="args">Method parameter array</param>
        /// <returns></returns>
        public static bool AccessorMethodSkipGeneric(this InstanceMeta instanceMeta, string name, out object result, params object[] args) => AccessorMethod(instanceMeta, name, out result, true, args);

        /// <summary>
        /// Call the specified method to obtain the return object and out parameters.
        /// </summary>
        /// <param name="instanceMeta">Metadata of the specified type.</param>
        /// <param name="name">Method name.</param>
        /// <param name="result">Return value, obtained as an out declaration.</param>
        /// <param name="skipGenericTypeCheck">Do you want to skip checking generic type parameters.</param>
        /// <param name="args">Method parameter array.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        static bool AccessorMethod(InstanceMeta instanceMeta, string name, out object result, bool skipGenericTypeCheck, params object[] args)
        {
            var pre = AccessorMethodPrepar(instanceMeta.TypeMeta, name, skipGenericTypeCheck, args);

            if (!(pre.Exception is AccessorException.No))
            {
                throw pre.Exception.AsException(name);
            }

            if (pre.Method.InvokeAsync is null)
            {
                throw AccessorException.InvokeNull.AsException(name);
            }

            if (!pre.Method.IsStatic && instanceMeta.Instance is null)
            {
                throw AccessorException.InstanceNull.AsException();
            }

            result = pre.Method.Invoke(instanceMeta.Instance, pre.Parameters, args);
            return true;
        }

        #endregion

        #region AccessorMethodAsync TypeMeta

        /// <summary>
        /// Gets the method value of a specified object.
        /// </summary>
        /// <typeparam name="ResultType"></typeparam>
        /// <param name="typeMeta">Metadata of the specified type.</param>
        /// <param name="name">Method name.</param>
        /// <param name="instance">The object on which to invoke the method or constructor.</param>
        /// <param name="args">Parameter object of calling method.</param>
        /// <returns></returns>
        public static async Task<ResultType> AccessorMethodAsync<ResultType>(this TypeMeta typeMeta, string name, object instance = default, params object[] args) => (ResultType)await AccessorMethodAsync(new InstanceMeta(typeMeta, instance), false, name, args);

        /// <summary>
        /// Gets the method value of a specified object.
        /// </summary>
        /// <param name="typeMeta">Metadata of the specified type.</param>
        /// <param name="name">Method name.</param>
        /// <param name="instance">The object on which to invoke the method or constructor.</param>
        /// <param name="args">Parameter object of calling method.</param>
        /// <returns></returns>
        public static async Task<object> AccessorMethodAsync(this TypeMeta typeMeta, string name, object instance = default, params object[] args) => await AccessorMethodAsync(new InstanceMeta(typeMeta, instance), false, name, args);

        /// <summary>
        /// Gets the method value of a specified object.
        /// </summary>
        /// <typeparam name="ResultType"></typeparam>
        /// <param name="typeMeta">Metadata of the specified type.</param>
        /// <param name="name">Method name.</param>
        /// <param name="instance">The object on which to invoke the method or constructor.</param>
        /// <param name="args">Parameter object of calling method.</param>
        /// <returns></returns>
        public static async Task<ResultType> AccessorMethodSkipGenericAsync<ResultType>(this TypeMeta typeMeta, string name, object instance = default, params object[] args) => (ResultType)await AccessorMethodAsync(new InstanceMeta(typeMeta, instance), true, name, args);

        /// <summary>
        /// Gets the method value of a specified object.
        /// </summary>
        /// <param name="typeMeta">Metadata of the specified type.</param>
        /// <param name="name">Method name.</param>
        /// <param name="instance">The object on which to invoke the method or constructor.</param>
        /// <param name="args">Parameter object of calling method.</param>
        /// <returns></returns>
        public static async Task<object> AccessorMethodSkipGenericAsync(this TypeMeta typeMeta, string name, object instance = default, params object[] args) => await AccessorMethodAsync(new InstanceMeta(typeMeta, instance), true, name, args);

        #endregion

        #region AccessorMethodAsync InstanceMeta

        /// <summary>
        /// Gets the method value of a specified object.
        /// </summary>
        /// <typeparam name="ResultType"></typeparam>
        /// <param name="instanceMeta">Metadata of the specified type.</param>
        /// <param name="name">Method name.</param>
        /// <param name="args">Parameter object of calling method.</param>
        /// <returns></returns>
        public static async Task<ResultType> AccessorMethodAsync<ResultType>(this InstanceMeta instanceMeta, string name, params object[] args) => (ResultType)await AccessorMethodAsync(instanceMeta, false, name, args);

        /// <summary>
        /// Gets the method value of a specified object.
        /// </summary>
        /// <param name="instanceMeta">Metadata of the specified type.</param>
        /// <param name="name">Method name.</param>
        /// <param name="args">Parameter object of calling method.</param>
        /// <returns></returns>
        public static async Task<object> AccessorMethodAsync(this InstanceMeta instanceMeta, string name, params object[] args) => await AccessorMethodAsync(instanceMeta, false, name, args);

        /// <summary>
        /// Gets the method value of a specified object.
        /// </summary>
        /// <typeparam name="ResultType"></typeparam>
        /// <param name="instanceMeta">Metadata of the specified type.</param>
        /// <param name="name">Method name.</param>
        /// <param name="args">Parameter object of calling method.</param>
        /// <returns></returns>
        public static async Task<ResultType> AccessorMethodSkipGenericAsync<ResultType>(this InstanceMeta instanceMeta, string name, params object[] args) => (ResultType)await AccessorMethodAsync(instanceMeta, true, name, args);

        /// <summary>
        /// Gets the method value of a specified object.
        /// </summary>
        /// <param name="instanceMeta">Metadata of the specified type.</param>
        /// <param name="name">Method name.</param>
        /// <param name="args">Parameter object of calling method.</param>
        /// <returns></returns>
        public static async Task<object> AccessorMethodSkipGenericAsync(this InstanceMeta instanceMeta, string name, params object[] args) => await AccessorMethodAsync(instanceMeta, true, name, args);

        /// <summary>
        /// Gets the method value of a specified object.
        /// </summary>
        /// <param name="instanceMeta">Metadata of the specified type.</param>
        /// <param name="skipGenericTypeCheck">Do you want to skip checking generic type parameters.</param>
        /// <param name="name">Method name.</param>
        /// <param name="args">Parameter object of calling method.</param>
        /// <returns></returns>
        static async Task<object> AccessorMethodAsync(InstanceMeta instanceMeta, bool skipGenericTypeCheck, string name, object[] args)
        {
            var pre = AccessorMethodPrepar(instanceMeta.TypeMeta, name, skipGenericTypeCheck, args);

            if (!(pre.Exception is AccessorException.No))
            {
                return await Task.FromException<object>(pre.Exception.AsException(name));
            }

            if (pre.Method.InvokeAsync is null)
            {
                return await Task.FromException<object>(AccessorException.InvokeNull.AsException(name));
            }

            if (!pre.Method.IsStatic && instanceMeta.Instance is null)
            {
                return await Task.FromException<object>(AccessorException.InstanceNull.AsException());
            }

            return await pre.Method.InvokeAsync(instanceMeta.Instance, pre.Parameters, args);
        }

        #endregion

        /// <summary>
        /// Method inspection results.
        /// </summary>
        public readonly struct AccessorMethodPreparResult
        {
            static readonly AccessorMethodPreparResult defaultValue = default;

            public AccessorMethodPreparResult(IAccessorMethod method, CheckedParameterValue[] parameters)
            {
                Method = method;
                Parameters = parameters;
                Exception = AccessorException.No;
            }

            public AccessorMethodPreparResult(AccessorException exception)
            {
                Method = default;
                Parameters = default;
                Exception = exception;
            }

            public bool IsDefault() => defaultValue.Equals(this);

            /// <summary>
            /// Accessible method metadata.
            /// </summary>
            public readonly IAccessorMethod Method { get; }

            /// <summary>
            /// Full size parameter array.
            /// </summary>
            public readonly CheckedParameterValue[] Parameters { get; }

            /// <summary>
            /// Checked exception.
            /// </summary>
            public readonly AccessorException Exception { get; }
        }

        /// <summary>
        /// Method check exception.
        /// </summary>
        public enum AccessorException
        {
            No,

            TypeNull,
            GenericDefinition,
            GeneratorNull,
            TypeNotExist,

            NameNull,
            AccessorNull,
            MemberNotExist,

            ArgumentOutOfRangeOrTypeError,

            InvokeNull,
            GetNull,
            SetNull,

            /// <summary>
            /// instance
            /// </summary>
            InstanceNull,

            ///// <summary>
            ///// Unable to call instance method.
            ///// </summary>
            //InstanceMethod,
            ///// <summary>
            ///// Unable to call static method.
            ///// </summary>
            //StaticMethod,
            //InstanceMember,
            //StaticMember,
            //ArgsNull,
        }

        static Exception AsException(this AccessorException exception, string memberName = default)
        {
            switch (exception)
            {
                case AccessorException.No:
                    return default;
                case AccessorException.TypeNull:
                    return new ArgumentNullException("type");
                case AccessorException.GenericDefinition:
                    return new ArgumentException("This operation is only valid on real generic type.", "type");
                case AccessorException.GeneratorNull:
                    return new NullReferenceException($"{nameof(IGeneratorCode)} cannot be empty.");
                case AccessorException.TypeNotExist:
                    return new System.Collections.Generic.KeyNotFoundException("The current type does not exist.");
                case AccessorException.NameNull:
                    return new ArgumentNullException("name");
                case AccessorException.AccessorNull:
                    return new NullReferenceException($"{nameof(IAccessorType)} cannot be empty.");
                case AccessorException.MemberNotExist:
                    return new System.Collections.Generic.KeyNotFoundException($"The current member \"{memberName}\" does not exist.");
                case AccessorException.ArgumentOutOfRangeOrTypeError:
                    return new ArgumentOutOfRangeException("args", "The number of parameters must be equal to the actual total number of parameters for the method, and the parameter types must be consistent.");
                case AccessorException.InvokeNull:
                    return new NotImplementedException($"This current method \"{memberName}\" is not callable.");
                case AccessorException.GetNull:
                    return new NotImplementedException($"This current member \"{memberName}\" is not read.");
                case AccessorException.SetNull:
                    return new NotImplementedException($"This current member \"{memberName}\" is not modify.");
                case AccessorException.InstanceNull:
                    return new ArgumentNullException("instance");
                //case AccessorException.InstanceMethod:
                //    return new MethodAccessException($"Unable to call instance method \"{memberName}\".");
                //case AccessorException.StaticMethod:
                //    return new MethodAccessException($"Unable to call static method \"{memberName}\".");
                //case AccessorException.InstanceMember:
                //    return new MethodAccessException("Cannot call property or field on static objects.");
                //case AccessorException.StaticMember:
                //    return new MethodAccessException("Cannot call property or field on instance objects.");
                //case AccessorException.ArgsNull:
                //    return new ArgumentNullException("args");
                default: return new Exception($"uncaught exception: {exception.GetName()}.");
            }
        }

        /// <summary>
        /// Check before accessing the method.
        /// </summary>
        /// <param name="typeMeta">Metadata of the specified type.</param>
        /// <param name="name">Method name.</param>
        /// <param name="skipGenericTypeCheck">Do you want to skip checking generic type parameters.</param>
        /// <param name="args">Parameter object of calling method.</param>
        /// <returns>Return inspection results.</returns>
        public static AccessorMethodPreparResult AccessorMethodPrepar(this TypeMeta typeMeta, string name, bool skipGenericTypeCheck = false, params object[] args)
        {
            var result = typeMeta.AsGeneratorType(name, out IAccessor accessor);

            if (!(result is AccessorException.No))
            {
                return new AccessorMethodPreparResult(result);
            }

            if (args is null)
            {
                args = Array.Empty<object>();
            }

            switch (accessor)
            {
                case IAccessorMethod method:
                    {
                        var checkedArgs = CheckedMethod(method, args, skipGenericTypeCheck);

                        if (checkedArgs is null)
                        {
                            return new AccessorMethodPreparResult(AccessorException.ArgumentOutOfRangeOrTypeError);
                        }

                        return new AccessorMethodPreparResult(method, checkedArgs);
                    }
                case IAccessorMethodCollection collection:
                    {
                        foreach (var method in collection)
                        {
                            var checkedArgs = CheckedMethod(method, args, skipGenericTypeCheck);

                            if (checkedArgs is null)
                            {
                                continue;
                            }

                            return new AccessorMethodPreparResult(method, checkedArgs);
                        }

                        return new AccessorMethodPreparResult(AccessorException.ArgumentOutOfRangeOrTypeError);
                    }
                default: return new AccessorMethodPreparResult(AccessorException.MemberNotExist);
            }
        }

        static CheckedParameterValue[] CheckedMethod(IMethod method, object[] args, bool skipGenericTypeCheck)
        {
            if (method.ParametersRealLength < args.Length || method.ParametersMustLength > args.Length)
            {
                return default;
            }

            if (0 == method.ParametersRealLength)
            {
                return Array.Empty<CheckedParameterValue>();
            }

            var argsObject = new CheckedParameterValue[method.ParametersRealLength];

            var skip = false;

            foreach (var parameter in method.Parameters)
            {
                if (parameter.Ordinal < args.Length)
                {
                    var arg = args[parameter.Ordinal];
                    Type argType;

                    //checked ref, out
                    if (parameter.RefKind is RefKind.Ref || parameter.RefKind is RefKind.Out)
                    {
                        if (!(arg is RefArg refValue && parameter.RefKind == refValue.RefKind))
                        {
                            skip = true;
                            break;
                        }

                        arg = refValue.Value;
                        argType = refValue.RuntimeType;
                    }
                    else
                    {
                        argType = arg?.GetType();
                    }

                    if (!typeof(object).Equals(parameter.Type.RuntimeType) && !(parameter.Type is IAccessorNamedType namedType && namedType.HasTypeParameter && skipGenericTypeCheck))
                    {
                        if (!(argType is null))
                        {
                            if (!Equals(argType, parameter.Type.RuntimeType) && !((TypeKind.Class == parameter.Type.TypeKind || TypeKind.Interface == parameter.Type.TypeKind || TypeKind.TypeParameter == parameter.Type.TypeKind) && parameter.Type.RuntimeType.IsAssignableFrom(argType)))
                            {
                                skip = true;
                                break;
                            }
                        }
                        else if (parameter.Type.RuntimeType.IsValueType)
                        {
                            skip = true;
                            break;
                        }
                    }

                    argsObject[parameter.Ordinal] = new CheckedParameterValue(arg, false);
                }
                //ImplicitDefaultValue is params or value = default
                else if (parameter.ImplicitDefaultValue)
                {
                    argsObject[parameter.Ordinal] = new CheckedParameterValue(default, parameter.ImplicitDefaultValue);
                }
                else if (parameter.HasExplicitDefaultValue)
                {
                    argsObject[parameter.Ordinal] = new CheckedParameterValue(parameter.ExplicitDefaultValue, default);
                }
                else if (parameter.IsParams)
                {
                    argsObject[parameter.Ordinal] = new CheckedParameterValue(parameter.Type.DefaultValue(true), default);
                }
                else
                {
                    skip = true;
                    break;
                    //throw new ArgumentOutOfRangeException($"{nameof(args)}[{i}]", "The number of parameters must be less than or equal to the actual total number of parameters for the method.");
                }
            }

            if (!skip)
            {
                return argsObject;
            }

            return default;
        }

        static string GetName(this Enum value) => null == value ? null : Enum.GetName(value.GetType(), value);

        #region AsGeneratorType

        /// <summary>
        /// Obtain the GeneratorTypeMeta object for the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="generatorCode"></param>
        /// <param name="typeMeta"></param>
        /// <returns></returns>
        static AccessorException AsGeneratorType(Type type, IGeneratorCode generatorCode, out TypeMeta typeMeta)
        {
            typeMeta = default;

            if (type is null)
            {
                return AccessorException.TypeNull;
            }

            if (type.IsGenericType && type.IsGenericTypeDefinition)
            {
                return AccessorException.GenericDefinition;
            }

            generatorCode = generatorCode ?? GeneratorCode;

            if (generatorCode is null)
            {
                return AccessorException.GeneratorNull;
            }

            if (!generatorCode.GeneratorType.TryGetValue(type, out typeMeta))
            {
                return AccessorException.TypeNotExist;
            }

            return AccessorException.No;
        }

        /// <summary>
        /// Obtain the AccessorException object for the specified GeneratorTypeMeta.
        /// </summary>
        /// <param name="typeMeta"></param>
        /// <param name="name"></param>
        /// <param name="accessor"></param>
        /// <returns></returns>
        public static AccessorException AsGeneratorType(this TypeMeta typeMeta, string name, out IAccessor accessor)
        {
            accessor = default;

            if (name is null)
            {
                return AccessorException.NameNull;
            }

            if (typeMeta.AccessorType is null)
            {
                return AccessorException.AccessorNull;
            }

            if (!typeMeta.AccessorType.Members.TryGetValue(name, out accessor))
            {
                return AccessorException.MemberNotExist;
            }

            return AccessorException.No;
        }

        /// <summary>
        /// Obtain the InstanceMeta object for the specified instance.
        /// </summary>
        /// <typeparam name="Instance"></typeparam>
        /// <param name="instance"></param>
        /// <param name="generatorCode"></param>
        /// <returns></returns>
        public static InstanceMeta AsGeneratorType<Instance>(this Instance instance, IGeneratorCode generatorCode = default)
        {
            if (instance is null)
            {
                throw AccessorException.InstanceNull.AsException();
            }

            var result = AsGeneratorType(typeof(Instance), generatorCode, out TypeMeta typeMeta);

            if (!(result is AccessorException.No))
            {
                throw result.AsException();
            }

            return new InstanceMeta(typeMeta, instance);
        }

        /// <summary>
        /// Obtain the GeneratorTypeMeta object for the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="generatorCode"></param>
        /// <returns></returns>
        public static TypeMeta AsGeneratorType(this Type type, IGeneratorCode generatorCode = default)
        {
            var result = AsGeneratorType(type, generatorCode, out TypeMeta typeMeta);

            if (!(result is AccessorException.No))
            {
                throw result.AsException();
            }

            return typeMeta;
        }

        /*
        static IAccessor AsGeneratorType(this GeneratorTypeMeta typeMeta, string name)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (typeMeta.AccessorType is null)
            {
                throw new NullReferenceException("AccessorType");
            }

            if (!typeMeta.AccessorType.Members.TryGetValue(name, out IAccessor accessor))
            {
                throw new System.Collections.Generic.KeyNotFoundException($"The current member \"{name}\" does not exist.");
            }

            return accessor;
        }
        */

        /// <summary>
        /// Substitutes the elements of an type for the type parameters of the current generic type definition and returns a GeneratorTypeMeta object representing the resulting constructed type.
        /// </summary>
        /// <typeparam name="Type">An type parameter to be substituted for the type parameters of the current generic type.</typeparam>
        /// <param name="type">Current generic type.</param>
        /// <param name="generatorType">Target IGeneratorCode.</param>
        /// <returns>A System.Type object representing the resulting constructed type.</returns>
        public static TypeMeta AsGeneratorType<Type>(this System.Type type, IGeneratorCode generatorType = default) => AsGeneratorType(type, typeof(Type), generatorType);

        /// <summary>
        /// Substitutes the elements of an type for the type parameters of the current generic type definition and returns a GeneratorTypeMeta object representing the resulting constructed type.
        /// </summary>
        /// <param name="type">Current generic type.</param>
        /// <param name="typeArgument">An types to be substituted for the type parameters of the current generic type.</param>
        /// <param name="generatorType">Target IGeneratorCode.</param>
        /// <returns>A System.Type object representing the resulting constructed type.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException"></exception>
        public static TypeMeta AsGeneratorType(this Type type, Type typeArgument, IGeneratorCode generatorType = default)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!type.IsGenericType)
            {
                throw new ArgumentNullException(nameof(type), "This operation is only valid on generic type.");
            }

            if (typeArgument is null)
            {
                throw new ArgumentNullException(nameof(typeArgument));
            }

            var typeMeta = AsGeneratorType(typeArgument, generatorType);

            if (typeMeta.MakeGenerics is null || !typeMeta.MakeGenerics.TryGetValue(type, out Type makeType))
            {
                throw new System.Collections.Generic.KeyNotFoundException($"{nameof(typeArgument)}<{nameof(typeArgument)}>");
            }

            return makeType.AsGeneratorType();
        }

        #endregion

        #region CreateInstance

        /// <summary>
        /// Create objects of pre built type.
        /// </summary>
        /// <typeparam name="Type">Returned target type.</typeparam>
        /// <param name="type">Target type metadata.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If args is an empty array or null, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static Type CreateInstance<Type>(this System.Type type, params object[] args) => type.AsGeneratorType().CreateInstance<Type>(args);

        /// <summary>
        /// Create objects of pre built type.
        /// </summary>
        /// <param name="type">Target type metadata.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If args is an empty array or null, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static InstanceMeta CreateInstance(this Type type, params object[] args) => type.AsGeneratorType().CreateInstance(args);

        /// <summary>
        /// Create objects of pre built type.
        /// </summary>
        /// <param name="type">Target type metadata.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If args is an empty array or null, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static Type CreateInstanceSkipGeneric<Type>(this System.Type type, params object[] args) => type.AsGeneratorType().CreateInstanceSkipGeneric<Type>(args);

        /// <summary>
        /// Create objects of pre built type.
        /// </summary>
        /// <param name="type">Target type metadata.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If args is an empty array or null, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static InstanceMeta CreateInstanceSkipGeneric(this Type type, params object[] args) => type.AsGeneratorType().CreateInstanceSkipGeneric(args);

        /// <summary>
        /// Create objects of pre built type.
        /// </summary>
        /// <typeparam name="Type">Returned target type.</typeparam>
        /// <param name="typeMeta">Target type metadata.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If args is an empty array or null, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static Type CreateInstance<Type>(this TypeMeta typeMeta, params object[] args) => (Type)CreateInstance(typeMeta, false, args);

        /// <summary>
        /// Create objects of pre built type.
        /// </summary>
        /// <param name="typeMeta">Target type metadata.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If args is an empty array or null, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static InstanceMeta CreateInstance(this TypeMeta typeMeta, params object[] args) => new InstanceMeta(typeMeta, CreateInstance(typeMeta, false, args));

        /// <summary>
        /// Create objects of pre built type.
        /// </summary>
        /// <param name="typeMeta">Target type metadata.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If args is an empty array or null, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static Type CreateInstanceSkipGeneric<Type>(this TypeMeta typeMeta, params object[] args) => (Type)CreateInstance(typeMeta, true, args);

        /// <summary>
        /// Create objects of pre built type.
        /// </summary>
        /// <param name="typeMeta">Target type metadata.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If args is an empty array or null, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static InstanceMeta CreateInstanceSkipGeneric(this TypeMeta typeMeta, params object[] args) => new InstanceMeta(typeMeta, CreateInstance(typeMeta, true, args));

        /// <summary>
        /// Create objects of pre built type.
        /// </summary>
        /// <param name="typeMeta">Target type metadata.</param>
        /// <param name="skipGenericTypeCheck">Do you want to skip checking generic type parameters.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If args is an empty array or null, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        static object CreateInstance(TypeMeta typeMeta, bool skipGenericTypeCheck, params object[] args)
        {
            if (args is null)
            {
                args = Array.Empty<object>();
            }

            foreach (var method in typeMeta.Constructors)
            {
                var checkedArgs = CheckedMethod(method, args, skipGenericTypeCheck);

                if (checkedArgs is null)
                {
                    continue;
                }

                if (method.Invoke is null)
                {
                    throw new NotImplementedException("This method is not callable.");
                }

                return method.Invoke(default, checkedArgs, args);
            }

            throw new ArgumentOutOfRangeException(nameof(args), "The number of parameters must be equal to the actual total number of parameters for the method, and the parameter types must be consistent.");
        }

        #endregion

        #region Attribute

        /// <summary>
        /// Retrieve custom attribute.
        /// </summary>
        /// <param name="accessorAttribute">Proporciona acceso a los datos de atributos personalizados para los ensamblados.</param>
        /// <returns></returns>
        public static Attribute GetAttribute(this AccessorAttribute accessorAttribute)
        {
            var typeMeta = accessorAttribute.RuntimeType.AsGeneratorType();

            var args = accessorAttribute.ConstructorArguments?.Select(c => c.Value);

            var attr = typeMeta.CreateInstance(args?.ToArray());

            if (!(typeMeta.AccessorType is null) && (accessorAttribute.NamedArguments?.Any() ?? false))
            {
                foreach (var item in accessorAttribute.NamedArguments)
                {
                    if (!(typeMeta.AsGeneratorType(item.Name, out IAccessor accessor) is AccessorException.No) || !(accessor is IAccessorMember member) || member.SetValue is null)
                    {
                        continue;
                    }

                    member.SetValue(ref attr, item.Value);
                }
            }

            return attr.Instance as Attribute;
        }

        /// <summary>
        /// Retrieve custom attribute.
        /// </summary>
        /// <typeparam name="Attribute">Proporciona acceso a los datos de atributos personalizados para los ensamblados.</typeparam>
        /// <param name="accessorAttribute"></param>
        /// <returns></returns>
        public static Attribute GetAttribute<Attribute>(this AccessorAttribute accessorAttribute)
            where Attribute : System.Attribute
            => GetAttribute(accessorAttribute) as Attribute;

        #endregion

        /*
        public static bool AccessorGet(object obj, string name, out object value, params object[] args)
        {
            if (!(obj is IGeneratorAccessor accessor))
            {
                throw new ArgumentException($"{nameof(obj)} is not {nameof(IGeneratorAccessor)}.", nameof(obj));
            }

            return accessor.AccessorGet(name, out value, args);
        }

        public static Task<object> AccessorGetAsync(object obj, string name, params object[] args)
        {
            if (!(obj is IGeneratorAccessor accessor))
            {
                throw new ArgumentException($"{nameof(obj)} is not {nameof(IGeneratorAccessor)}.", nameof(obj));
            }

            return accessor.AccessorGetAsync(name, args);
        }

        public static bool AccessorSet(object obj, string name, object value)
        {
            if (!(obj is IGeneratorAccessor accessor))
            {
                throw new ArgumentException($"{nameof(obj)} is not {nameof(IGeneratorAccessor)}.", nameof(obj));
            }

            return accessor.AccessorSet(name, value);
        }
        */
    }
}
