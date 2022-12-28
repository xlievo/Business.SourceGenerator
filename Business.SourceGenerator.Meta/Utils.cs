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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;

    public static partial class Utils
    {
        /// <summary>
        /// "BusinessSourceGenerator"
        /// </summary>
        const string GeneratorCodeName = "BusinessSourceGenerator";

        static readonly Lazy<IGeneratorType> generatorCode = new Lazy<IGeneratorType>(() => GetGenericType(System.Reflection.Assembly.GetEntryAssembly()));

        /// <summary>
        /// IGeneratorCode in Assembly.GetEntryAssembly().
        /// <para>According to the compilation order, the entry assembly will be compiled last.</para>
        /// </summary>
        public static IGeneratorType GeneratorCode { get => generatorCode.Value; }

        //public static bool SetGeneratorCode(System.Reflection.Assembly assembly)
        //{
        //    if (assembly is null)
        //    {
        //        throw new ArgumentNullException(nameof(assembly));
        //    }

        //    var type = assembly.GetType($"{assembly?.GetName().Name}.{GeneratorCodeName}", false);

        //    if (null != type)
        //    {
        //        GeneratorCode = type.GetProperty("Generator").GetValue(null) as IGeneratorType;

        //        return true;
        //    }

        //    return false;
        //}

        /// <summary>
        /// Gets the generator type of the specified assembly.
        /// </summary>
        /// <param name="assembly">Target assembly.</param>
        /// <returns>A IGeneratorType or null</returns>
        public static IGeneratorType GetGenericType(System.Reflection.Assembly assembly)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var type = assembly.GetType($"{assembly?.GetName().Name}.{GeneratorCodeName}", false);

            if (null != type)
            {
                return type.GetProperty("Generator").GetValue(null) as IGeneratorType;
            }

            return default;
        }

        public static bool AccessorGet(this IGeneratorAccessor accessor, string name, out object value, params object[] args)
        {
            if (accessor is null)
            {
                throw new ArgumentNullException(nameof(accessor));
            }

            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (accessor.AccessorType().Members.TryGetValue(name, out IAccessorMeta meta))
            {
                switch (meta)
                {
                    case IAccessorMember member:
                        if (member.GetValue is null)
                        {
                            break;
                        }
                        value = member.GetValue(accessor);
                        return true;
                    case IAccessorMethod member:
                        if (member.GetValue is null)
                        {
                            break;
                        }
                        value = member.GetValue(accessor, args);
                        return true;
                    default: break;
                }
            }

            value = default;

            return default;
        }

        public static Task<object> AccessorGetAsync(this IGeneratorAccessor accessor, string name, params object[] args)
        {
            if (accessor is null)
            {
                throw new ArgumentNullException(nameof(accessor));
            }

            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (!accessor.AccessorType().Members.TryGetValue(name, out IAccessorMeta meta) || !(meta is IAccessorMethod member) || member.GetValueAsync is null)
            {
                return default;
            }

            return member.GetValueAsync(accessor, args);
        }

        /*
        public static bool AccessorSet(this IGeneratorAccessor accessor, string name, object value)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (accessor.AccessorType().Members.TryGetValue(name, out IAccessorMeta meta) && meta is IAccessorMember member)
            {
                if (member.SetValue is null)
                {
                    return default;
                }

                member.SetValue(ref accessor, value);

                return true;
            }

            return default;
        }
        */
        #region IGeneratorType

        static string GetGenericType(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return TypeNameFormatter.TypeName.GetFormattedName(type, TypeNameFormatter.TypeNameFormatOptions.Namespaces | TypeNameFormatter.TypeNameFormatOptions.NoKeywords | TypeNameFormatter.TypeNameFormatOptions.NoTuple | TypeNameFormatter.TypeNameFormatOptions.NoAnonymousTypes);
        }

        /// <summary>
        /// searches for the specified constructor whose parameters match the specified argument types.
        /// </summary>
        /// <param name="constructors"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        static (int sign, object[] args) GetConstructor(IEnumerable<Constructor> constructors, params object[] args)
        {
            foreach (var item in constructors)
            {
                //if (!string.Equals(item.key, typeName))
                //{
                //    continue;
                //}

                if (item.parameters.Length < args.Length || item.length > args.Length)
                {
                    continue;
                }

                if (0 == item.parameters.Length)
                {
                    return (item.sign, args);
                }

                var parameters = item.parameters;

                var argsObject = new object[parameters.Length];

                var skip = false;

                for (int i = 0; i < argsObject.Length; i++)
                {
                    var parameter = parameters[i];

                    if (i < args.Length)
                    {
                        var arg = args[i];

                        if (!typeof(object).Equals(parameter.type))
                        {
                            if (!Equals(null, arg))
                            {
                                var argType = arg.GetType();

                                if (!Equals(argType, parameter.type) && !((TypeKind.Class == parameter.kind || TypeKind.Interface == parameter.kind) && parameter.type.IsAssignableFrom(argType)))
                                {
                                    skip = true;
                                    break;
                                }
                            }
                            else if (parameter.isValueType)
                            {
                                skip = true;
                                break;
                            }
                        }

                        argsObject[i] = arg;
                    }
                    else if (parameter.hasDefaultValue)
                    {
                        argsObject[i] = parameter.defaultValue;
                    }
                }

                if (!skip)
                {
                    return (item.sign, argsObject);
                }
            }

            return default;
        }

        static Func<GeneratorTypeArg, GeneratorTypeOpt, object> GetType(Type type, IGeneratorType generatorType)
        {
            if (!generatorType.GeneratorType.TryGetValue(GetGenericType(type), out Func<GeneratorTypeArg, GeneratorTypeOpt, object> value))
            {
                if (type.IsGenericType)
                {
                    var typeArgument = type.GetGenericArguments();

                    var typeArgumentKey = 1 < typeArgument.Length ? $"<{string.Join(", ", typeArgument.Select(c => GetGenericType(c)))}>" : GetGenericType(typeArgument[0]);

                    if (!generatorType.GeneratorType.TryGetValue(typeArgumentKey, out value))
                    {
                        return default;
                    }
                }
                else
                {
                    return default;
                }
            }

            return value;
        }

        /// <summary>
        /// Substitutes the generic type parameter for the type parameters of the  current generic type definition and returns a System.Type object representing the resulting constructed type.
        /// </summary>
        /// <typeparam name="Type">An type parameter to be substituted for the type parameters of the current generic type.</typeparam>
        /// <param name="type">Current generic type.</param>
        /// <returns>A System.Type object representing the resulting constructed type.</returns>
        public static System.Type MakeGenericType<Type>(this System.Type type) => MakeGenericTypes(GeneratorCode, type, typeof(Type));

        /// <summary>
        /// Substitutes the generic type parameter for the type parameters of the  current generic type definition and returns a System.Type object representing the resulting constructed type.
        /// </summary>
        /// <typeparam name="Type">An type parameter to be substituted for the type parameters of the current generic type.</typeparam>
        /// <param name="generatorType">Target IGeneratorType.</param>
        /// <param name="type">Current generic type.</param>
        /// <returns>A System.Type object representing the resulting constructed type.</returns>
        public static System.Type MakeGenericType<Type>(this IGeneratorType generatorType, System.Type type) => MakeGenericTypes(generatorType, type, typeof(Type));

        /// <summary>
        /// Substitutes the elements of an array of types for the type parameters of the current generic type definition and returns a System.Type object representing the resulting constructed type.
        /// </summary>
        /// <param name="type">Current generic type.</param>
        /// <param name="typeArgument">An array of types to be substituted for the type parameters of the current generic type.</param>
        /// <returns>A System.Type object representing the resulting constructed type.</returns>
        public static Type MakeGenericTypes(this Type type, params Type[] typeArgument) => MakeGenericTypes(GeneratorCode, type, typeArgument);

        /// <summary>
        /// Substitutes the elements of an array of types for the type parameters of the current generic type definition and returns a System.Type object representing the resulting constructed type.
        /// </summary>
        /// <param name="generatorType">Target IGeneratorType.</param>
        /// <param name="type">Current generic type.</param>
        /// <param name="typeArgument">An array of types to be substituted for the type parameters of the current generic type.</param>
        /// <returns></returns>
        public static Type MakeGenericTypes(this IGeneratorType generatorType, Type type, params Type[] typeArgument)
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
                throw new ArgumentNullException(nameof(type), "This operation is only valid on generic types.");
            }

            if (typeArgument is null)
            {
                throw new ArgumentNullException(nameof(typeArgument));
            }

            if (0 == typeArgument.Length)
            {
                return type;
            }

            var key = 1 < typeArgument.Length ? $"<{string.Join(", ", typeArgument.Select(c => GetGenericType(c)))}>" : GetGenericType(typeArgument[0]);

            if (!generatorType.GeneratorType.TryGetValue(key, out Func<GeneratorTypeArg, GeneratorTypeOpt, object> value))
            {
                return default;
            }

            return value(new GeneratorTypeArg(GetGenericType(type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition())), GeneratorTypeOpt.MakeGenericType) as Type;
        }

        /// <summary>
        /// Create objects of pre built type.
        /// </summary>
        /// <param name="type">Target type</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If args is an empty array or null, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static object CreateInstance(this Type type, params object[] args) => CreateInstance(GeneratorCode, type, args);

        /// <summary>
        /// Create objects of pre built type.
        /// </summary>
        /// <param name="generatorType">Target IGeneratorType.</param>
        /// <param name="type">Target type</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If args is an empty array or null, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <returns></returns>
        public static object CreateInstance(this IGeneratorType generatorType, Type type, params object[] args)
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

            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var value = GetType(type, generatorType);

            if (default == value)
            {
                return default;
            }

            if (!(value(new GeneratorTypeArg(GetGenericType(type.IsGenericType ? type.GetGenericTypeDefinition() : type)), GeneratorTypeOpt.Constructors) is IEnumerable<Constructor> constructors))
            {
                return default;
            }

            var constructor = GetConstructor(constructors, args);

            if (default == constructor)
            {
                return default;
            }

            return value(new GeneratorTypeArg(constructor.sign, constructor.args), GeneratorTypeOpt.CreateGenericType);
        }

        /// <summary>
        /// Whether to customize the object.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool ContainsType(this Type type) => ContainsType(GeneratorCode, type);

        /// <summary>
        /// Whether to customize the object.
        /// </summary>
        /// <param name="generatorType">Target IGeneratorType.</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool ContainsType(this IGeneratorType generatorType, Type type)
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

            var value = GetType(type, generatorType);

            if (default == value)
            {
                return default;
            }

            return (bool)value(default, GeneratorTypeOpt.ContainsType);
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
