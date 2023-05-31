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
    using System.Threading.Tasks;

    public static partial class Utils
    {
        static Lazy<IGeneratorType> generatorCode = default;

        /// <summary>
        /// Global IGeneratorType.
        /// <para>According to the compilation order, the entry assembly will be compiled last.</para>
        /// </summary>
        public static IGeneratorType GeneratorCode { get => generatorCode.Value; }

        /// <summary>
        /// Specify the global IGeneratorType.
        /// </summary>
        /// <param name="generatorType"></param>
        public static void SetGeneratorCode(this IGeneratorType generatorType) => generatorCode = new Lazy<IGeneratorType>(() => generatorType);

        #region Member

        /// <summary>
        /// Gets the property or field value of a specified object.
        /// </summary>
        /// <typeparam name="Type">Returns the caller of the specified type.</typeparam>
        /// <param name="accessor">The object whose value will be get.</param>
        /// <param name="name">property or field name.</param>
        /// <returns>Return member value.</returns>
        public static Type AccessorGet<Type>(this IGeneratorAccessor accessor, string name)
        {
            var success = AccessorGet(accessor, name, out object result2);

            return success ? (Type)result2 : default;
        }

        /// <summary>
        /// Gets the property or field value of a specified object.
        /// </summary>
        /// <typeparam name="Type">Returns the caller of the specified type.</typeparam>
        /// <param name="accessor">The object whose value will be get.</param>
        /// <param name="name">property or field name.</param>
        /// <param name="result">The property or field value of the specified object.</param>
        /// <returns>Return to true successfully.</returns>
        public static bool AccessorGet<Type>(this IGeneratorAccessor accessor, string name, out Type result)
        {
            var success = AccessorGet(accessor, name, out object result2);

            result = success ? (Type)result2 : default;

            return success;
        }

        /// <summary>
        /// Gets the property or field value of a specified object.
        /// </summary>
        /// <param name="accessor">The object whose value will be get.</param>
        /// <param name="name">property or field name.</param>
        /// <param name="result">The property or field value of the specified object.</param>
        /// <returns>Return to true successfully.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool AccessorGet(this IGeneratorAccessor accessor, string name, out object result)
        {
            if (accessor is null)
            {
                throw new ArgumentNullException(nameof(accessor));
            }

            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (!accessor.AccessorType().Members.TryGetValue(name, out IAccessor meta) || !(meta is IAccessorMember member) || member.GetValue is null)
            {
                result = default;
                return default;
            }

            result = member.GetValue(accessor);
            return true;
        }

        /// <summary>
        /// Sets the property or field value of a specified object.
        /// </summary>
        /// <param name="accessor">The object whose value will be set.</param>
        /// <param name="name">property or field name.</param>
        /// <param name="value">The new value.</param>
        /// <returns></returns>
        public static IGeneratorAccessor AccessorSet(this IGeneratorAccessor accessor, string name, object value)
        {
            if (accessor.AccessorSet(name, value))
            {
                return accessor;
            }

            return default;
        }

        /// <summary>
        /// Sets the property or field value of a specified object.
        /// </summary>
        /// <typeparam name="Type">Returns the caller of the specified type.</typeparam>
        /// <param name="accessor">The object whose value will be set.</param>
        /// <param name="name">property or field name.</param>
        /// <param name="value">The new value.</param>
        /// <returns></returns>
        public static Type AccessorSet<Type>(this IGeneratorAccessor accessor, string name, object value)
        {
            if (accessor.AccessorSet(name, value))
            {
                return (Type)accessor;
            }

            return default;
        }

        #endregion

        #region Method

        /// <summary>
        /// Call the specified method to obtain the return object and out parameters.
        /// </summary>
        /// <typeparam name="ResultType"></typeparam>
        /// <param name="accessor">Own an instance of this method.</param>
        /// <param name="name">Method name.</param>
        /// <param name="result">Return value, obtained as an out declaration.</param>
        /// <param name="args">Method parameter array.</param>
        /// <returns></returns>
        public static bool AccessorMethod<ResultType>(this IGeneratorAccessor accessor, string name, out ResultType result, params object[] args)
        {
            var success = AccessorMethod(accessor, name, out object result2, false, args);

            result = success ? (ResultType)result2 : default;

            return success;
        }

        /// <summary>
        /// Call the specified method to obtain the return object and out parameters.
        /// </summary>
        /// <param name="accessor">Own an instance of this method.</param>
        /// <param name="name">Method name.</param>
        /// <param name="result">Return value, obtained as an out declaration.</param>
        /// <param name="args">Method parameter array.</param>
        /// <returns></returns>
        public static bool AccessorMethod(this IGeneratorAccessor accessor, string name, out object result, params object[] args) => AccessorMethod(accessor, name, out result, false, args);

        /// <summary>
        /// Call the specified method to obtain the return object and out parameters.
        /// </summary>
        /// <typeparam name="ResultType"></typeparam>
        /// <param name="accessor">Own an instance of this method.</param>
        /// <param name="name">Method name.</param>
        /// <param name="result">Return value, obtained as an out declaration.</param>
        /// <param name="args">Method parameter array.</param>
        /// <returns></returns>
        public static bool AccessorMethodSkipGeneric<ResultType>(this IGeneratorAccessor accessor, string name, out ResultType result, params object[] args)
        {
            var success = AccessorMethod(accessor, name, out object result2, true, args);

            result = success ? (ResultType)result2 : default;

            return success;
        }

        /// <summary>
        /// Call the specified method to obtain the return object and out parameters.
        /// </summary>
        /// <param name="accessor">Own an instance of this method.</param>
        /// <param name="name">Method name.</param>
        /// <param name="result">Return value, obtained as an out declaration.</param>
        /// <param name="skipGenericTypeCheck"></param>
        /// <param name="args">Method parameter array</param>
        /// <returns></returns>
        public static bool AccessorMethodSkipGeneric(this IGeneratorAccessor accessor, string name, out object result, params object[] args) => AccessorMethod(accessor, name, out result, true, args);

        /// <summary>
        /// Call the specified method to obtain the return object and out parameters.
        /// </summary>
        /// <param name="accessor">Own an instance of this method.</param>
        /// <param name="name">Method name.</param>
        /// <param name="result">Return value, obtained as an out declaration.</param>
        /// <param name="skipGenericTypeCheck">Do you want to skip checking generic type parameters.</param>
        /// <param name="args">Method parameter array.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        static bool AccessorMethod(IGeneratorAccessor accessor, string name, out object result, bool skipGenericTypeCheck, params object[] args)
        {
            var checkedResult = AccessorMethodPrepar(accessor, name, skipGenericTypeCheck, args);

            switch (checkedResult.Exception)
            {
                case AccessorMethodPreparException.AccessorNull:
                    throw new ArgumentNullException(nameof(accessor));
                case AccessorMethodPreparException.NameNull:
                    throw new ArgumentNullException(nameof(name));
                case AccessorMethodPreparException.ArgsNull:
                    throw new ArgumentNullException(nameof(args));
                case AccessorMethodPreparException.MethodNotExist:
                case AccessorMethodPreparException.ArgumentOutOfRangeOrTypeError:
                    result = default;
                    return default;
                default:
                    if (checkedResult.Method.Invoke is null)
                    {
                        result = default;
                        return default;
                    }

                    result = checkedResult.Method.Invoke(accessor, checkedResult.Parameters, args);
                    return true;
            }
        }

        /// <summary>
        /// Gets the method value of a specified object.
        /// </summary>
        /// <typeparam name="ResultType"></typeparam>
        /// <param name="accessor">Own an instance of this method.</param>
        /// <param name="name">method name.</param>
        /// <param name="args">Parameter object of calling method.</param>
        /// <returns></returns>
        public static async Task<ResultType> AccessorMethodAsync<ResultType>(this IGeneratorAccessor accessor, string name, params object[] args) => (ResultType)await AccessorMethodAsync(accessor, name, false, args);

        /// <summary>
        /// Gets the method value of a specified object.
        /// </summary>
        /// <param name="accessor">Own an instance of this method.</param>
        /// <param name="name">method name.</param>
        /// <param name="args">Parameter object of calling method.</param>
        /// <returns></returns>
        public static async Task<object> AccessorMethodAsync(this IGeneratorAccessor accessor, string name, params object[] args) => await AccessorMethodAsync(accessor, name, false, args);

        /// <summary>
        /// Gets the method value of a specified object.
        /// </summary>
        /// <typeparam name="ResultType"></typeparam>
        /// <param name="accessor">Own an instance of this method.</param>
        /// <param name="name">method name.</param>
        /// <param name="args">Parameter object of calling method.</param>
        /// <returns></returns>
        public static async Task<ResultType> AccessorMethodSkipGenericAsync<ResultType>(this IGeneratorAccessor accessor, string name, params object[] args) => (ResultType)await AccessorMethodAsync(accessor, name, true, args);

        /// <summary>
        /// Gets the method value of a specified object.
        /// </summary>
        /// <param name="accessor">Own an instance of this method.</param>
        /// <param name="name">method name.</param>
        /// <param name="args">Parameter object of calling method.</param>
        /// <returns></returns>
        public static async Task<object> AccessorMethodSkipGenericAsync(this IGeneratorAccessor accessor, string name, params object[] args) => await AccessorMethodAsync(accessor, name, true, args);

        /// <summary>
        /// Gets the method value of a specified object.
        /// </summary>
        /// <param name="accessor">Own an instance of this method.</param>
        /// <param name="name">method name.</param>
        /// <param name="skipGenericTypeCheck">Do you want to skip checking generic type parameters.</param>
        /// <param name="args">Parameter object of calling method.</param>
        /// <returns></returns>
        static async Task<object> AccessorMethodAsync(IGeneratorAccessor accessor, string name, bool skipGenericTypeCheck, params object[] args)
        {
            var checkedResult = AccessorMethodPrepar(accessor, name, skipGenericTypeCheck, args);

            switch (checkedResult.Exception)
            {
                case AccessorMethodPreparException.AccessorNull:
                    return await Task.FromException<object>(new ArgumentNullException(nameof(accessor)));
                case AccessorMethodPreparException.NameNull:
                    return await Task.FromException<object>(new ArgumentNullException(nameof(name)));
                case AccessorMethodPreparException.ArgsNull:
                    return await Task.FromException<object>(new ArgumentNullException(nameof(args)));
                case AccessorMethodPreparException.MethodNotExist:
                    return await Task.FromException<object>(new MethodAccessException($"The current method \"{name}\" does not exist."));
                case AccessorMethodPreparException.ArgumentOutOfRangeOrTypeError:
                    return await Task.FromException<object>(new ArgumentOutOfRangeException(nameof(args), "The number of parameters must be equal to the actual total number of parameters for the method, and the parameter types must be consistent."));
                default:
                    if (checkedResult.Method.InvokeAsync is null)
                    {
                        return await Task.FromException<object>(new NotImplementedException("This method is not callable."));
                    }

                    return await checkedResult.Method.InvokeAsync(accessor, checkedResult.Parameters, args);
            }
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
                Exception = AccessorMethodPreparException.No;
            }

            public AccessorMethodPreparResult(AccessorMethodPreparException exception)
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
            public readonly AccessorMethodPreparException Exception { get; }
        }

        /// <summary>
        /// Method check exception.
        /// </summary>
        public enum AccessorMethodPreparException
        {
            No,
            AccessorNull,
            NameNull,
            ArgsNull,
            MethodNotExist,
            ArgumentOutOfRangeOrTypeError,
        }

        /// <summary>
        /// Check before accessing the method.
        /// </summary>
        /// <param name="accessor">Own an instance of this method.</param>
        /// <param name="name">method name.</param>
        /// <param name="skipGenericTypeCheck">Do you want to skip checking generic type parameters.</param>
        /// <param name="args">Parameter object of calling method.</param>
        /// <returns>Return inspection results.</returns>
        public static AccessorMethodPreparResult AccessorMethodPrepar(IGeneratorAccessor accessor, string name, bool skipGenericTypeCheck = false, params object[] args)
        {
            if (accessor is null)
            {
                return new AccessorMethodPreparResult(AccessorMethodPreparException.AccessorNull);
            }

            if (name is null)
            {
                return new AccessorMethodPreparResult(AccessorMethodPreparException.NameNull);
            }

            if (args is null)
            {
                args = Array.Empty<object>();
            }

            if (!accessor.AccessorType().Members.TryGetValue(name, out IAccessor meta))
            {
                return new AccessorMethodPreparResult(AccessorMethodPreparException.MethodNotExist);
            }

            switch (meta)
            {
                case IAccessorMethod method:
                    {
                        var checkedArgs = CheckedMethod(method, args, skipGenericTypeCheck);

                        if (checkedArgs is null)
                        {
                            return new AccessorMethodPreparResult(AccessorMethodPreparException.ArgumentOutOfRangeOrTypeError);
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

                        return new AccessorMethodPreparResult(AccessorMethodPreparException.ArgumentOutOfRangeOrTypeError);
                    }
                default: return new AccessorMethodPreparResult(AccessorMethodPreparException.MethodNotExist);
            }
        }

        static CheckedParameterValue[] CheckedMethod(IMethodMeta method, object[] args, bool skipGenericTypeCheck)
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

            foreach (var parameter in method.ParametersMeta)
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
                        argType = refValue.Type;
                    }
                    else
                    {
                        argType = arg?.GetType();
                    }

                    if (!typeof(object).Equals(parameter.RuntimeType) && !(parameter.HasGenericType && skipGenericTypeCheck))
                    {
                        if (!(argType is null))
                        {
                            if (!Equals(argType, parameter.RuntimeType) && !((TypeKind.Class == parameter.TypeKind || TypeKind.Interface == parameter.TypeKind || TypeKind.TypeParameter == parameter.TypeKind) && parameter.RuntimeType.IsAssignableFrom(argType)))
                            {
                                skip = true;
                                break;
                            }
                        }
                        else if (parameter.IsValueType)
                        {
                            skip = true;
                            break;
                        }
                    }

                    argsObject[parameter.Ordinal] = new CheckedParameterValue(arg, false);
                }
                //ImplicitDefaultValue is params and value = default
                else if (parameter.HasExplicitDefaultValue || parameter.ImplicitDefaultValue)
                {
                    argsObject[parameter.Ordinal] = new CheckedParameterValue(parameter.ExplicitDefaultValue, parameter.ImplicitDefaultValue);
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

        #region IGeneratorType

        /// <summary>
        /// Substitutes the generic type parameter for the type parameters of the  current generic type definition and returns a System.Type object representing the resulting constructed type.
        /// </summary>
        /// <typeparam name="Type">An type parameter to be substituted for the type parameters of the current generic type.</typeparam>
        /// <param name="type">Current generic type.</param>
        /// <returns>A System.Type object representing the resulting constructed type.</returns>
        public static System.Type GetGenericType<Type>(this System.Type type) => GetGenericType(GeneratorCode, type, typeof(Type));

        /// <summary>
        /// Substitutes the generic type parameter for the type parameters of the  current generic type definition and returns a System.Type object representing the resulting constructed type.
        /// </summary>
        /// <typeparam name="Type">An type parameter to be substituted for the type parameters of the current generic type.</typeparam>
        /// <param name="generatorType">Target IGeneratorType.</param>
        /// <param name="type">Current generic type.</param>
        /// <returns>A System.Type object representing the resulting constructed type.</returns>
        public static System.Type GetGenericType<Type>(this IGeneratorType generatorType, System.Type type) => GetGenericType(generatorType, type, typeof(Type));

        /// <summary>
        /// Substitutes the elements of an type for the type parameters of the current generic type definition and returns a System.Type object representing the resulting constructed type.
        /// </summary>
        /// <param name="type">Current generic type.</param>
        /// <param name="typeArgument">An types to be substituted for the type parameters of the current generic type.</param>
        /// <returns>A System.Type object representing the resulting constructed type.</returns>
        public static Type GetGenericType(this Type type, Type typeArgument) => GetGenericType(GeneratorCode, type, typeArgument);

        /// <summary>
        /// Substitutes the elements of an type for the type parameters of the current generic type definition and returns a System.Type object representing the resulting constructed type.
        /// </summary>
        /// <param name="generatorType">Target IGeneratorType.</param>
        /// <param name="type">Current generic type.</param>
        /// <param name="typeArgument">An types to be substituted for the type parameters of the current generic type.</param>
        /// <returns>A System.Type object representing the resulting constructed type.</returns>
        public static Type GetGenericType(this IGeneratorType generatorType, Type type, Type typeArgument)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (generatorType is null)
            {
                throw new ArgumentNullException(nameof(generatorType));
            }

            if (!type.IsGenericType)
            {
                throw new ArgumentNullException(nameof(type), "This operation is only valid on generic type.");
            }

            if (typeArgument is null)
            {
                throw new ArgumentNullException(nameof(typeArgument));
            }

            if (!generatorType.GeneratorType.TryGetValue(typeArgument, out GeneratorTypeMeta meta))
            {
                return default;
            }

            if (meta.MakeGenerics is null || !meta.MakeGenerics.TryGetValue(type, out Type makeType))
            {
                return default;
            }

            return makeType;
        }

        /// <summary>
        /// Create objects of pre built type.
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="type">Target type.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If args is an empty array or null, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static Type CreateInstance<Type>(this System.Type type, params object[] args) => (Type)CreateInstance(GeneratorCode, type, false, args);

        /// <summary>
        /// Create objects of pre built type.
        /// </summary>
        /// <param name="type">Target type.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If args is an empty array or null, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static object CreateInstance(this Type type, params object[] args) => CreateInstance(GeneratorCode, type, false, args);

        /// <summary>
        /// Create objects of pre built type.
        /// </summary>
        /// <param name="generatorType">Target IGeneratorType.</param>
        /// <param name="type">Target type.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If args is an empty array or null, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static object CreateInstance(this IGeneratorType generatorType, Type type, params object[] args) => CreateInstance(generatorType, type, false, args);

        /// <summary>
        /// Create objects of pre built type.
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="type">Target type.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If args is an empty array or null, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static Type CreateInstanceSkipGeneric<Type>(this System.Type type, params object[] args) => (Type)CreateInstance(GeneratorCode, type, true, args);

        /// <summary>
        /// Create objects of pre built type.
        /// </summary>
        /// <param name="type">Target type.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If args is an empty array or null, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static object CreateInstanceSkipGeneric(this Type type, params object[] args) => CreateInstance(GeneratorCode, type, true, args);

        /// <summary>
        /// Create objects of pre built type.
        /// </summary>
        /// <param name="generatorType"></param>
        /// <param name="type">Target type.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If args is an empty array or null, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static object CreateInstanceSkipGeneric(this IGeneratorType generatorType, Type type, params object[] args) => CreateInstance(generatorType, type, true, args);

        /// <summary>
        /// Create objects of pre built type.
        /// </summary>
        /// <param name="generatorType">Target IGeneratorType.</param>
        /// <param name="type">Target type.</param>
        /// <param name="skipGenericTypeCheck">Do you want to skip checking generic type parameters</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If args is an empty array or null, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        static object CreateInstance(IGeneratorType generatorType, Type type, bool skipGenericTypeCheck, params object[] args)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (generatorType is null)
            {
                throw new ArgumentNullException(nameof(generatorType));
            }

            if (type.IsGenericType && type.IsGenericTypeDefinition)
            {
                throw new ArgumentNullException(nameof(type), "This operation is only valid on real generic types.");
            }

            //if (args is null)
            //{
            //    throw new ArgumentNullException(nameof(args));
            //}

            if (args is null)
            {
                args = Array.Empty<object>();
            }

            if (!generatorType.GeneratorType.TryGetValue(type, out GeneratorTypeMeta meta))
            {
                return default;
            }

            foreach (var method in meta.Constructors)
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

        /// <summary>
        /// Whether to customize the object.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsCustom(this Type type) => IsCustom(GeneratorCode, type);

        /// <summary>
        /// Whether to customize the object.
        /// </summary>
        /// <param name="generatorType">Target IGeneratorType.</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsCustom(this IGeneratorType generatorType, Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (generatorType is null)
            {
                throw new ArgumentNullException(nameof(generatorType));
            }

            if (type.IsGenericType && type.IsGenericTypeDefinition)
            {
                throw new ArgumentNullException(nameof(type), "This operation is only valid on generic types.");
            }

            if (!generatorType.GeneratorType.TryGetValue(type, out GeneratorTypeMeta meta))
            {
                return default;
            }

            return meta.IsCustom;
        }

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
