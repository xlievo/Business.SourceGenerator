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
    using Business.SourceGenerator.Analysis;
    using System;

    public static partial class Utils
    {
        /// <summary>
        /// "BusinessSourceGenerator"
        /// </summary>
        public const string GeneratorCodeName = "BusinessSourceGenerator";

        static Utils()
        {
            SetGeneratorCode(System.Reflection.Assembly.GetEntryAssembly());
        }

        /// <summary>
        /// Find IGeneratorCode in Assembly.GetEntryAssembly().
        /// </summary>
        public static IGeneratorCode GeneratorCode { get; set; }

        public static bool SetGeneratorCode(System.Reflection.Assembly assembly)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var type = assembly.GetType($"{assembly?.GetName().Name}.{GeneratorCodeName}", false);

            if (null != type)
            {
                GeneratorCode = type.GetProperty("Generator").GetValue(null) as IGeneratorCode;

                return true;
            }

            return false;
        }

        public static bool TryGetGeneratorAccessor(object instance, string name, out object value)
        {
            value = default;

            if (instance is null || name is null || !(instance is IGeneratorAccessor accessor))
            {
                return false;
            }

            try
            {
                return accessor.AccessorGet(name, out value);
            }
            catch
            {
                return false;
            }
        }

        public static bool TrySetGeneratorAccessor(object instance, string name, object value)
        {
            if (instance is null || name is null || !(instance is IGeneratorAccessor accessor))
            {
                return false;
            }

            try
            {
                return accessor.AccessorSet(name, value);
            }
            catch
            {
                return false;
            }
        }

        /*
        /// <summary>
        /// Replace System.Type.MakeGenericType(params Type[] typeArguments).
        /// <para>Find IGeneratorCode in Assembly.GetEntryAssembly().</para>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="typeArguments"></param>
        /// <returns></returns>
        public static Type MakeGenericType(Type type, params Type[] typeArguments) => generatorCode?.MakeGenericType(type, typeArguments);

        /// <summary>
        /// Replace System.Type.MakeGenericType(params Type[] typeArguments).
        /// </summary>
        /// <param name="generatorCode">IGeneratorCode</param>
        /// <param name="type">type</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic type.</param>
        /// <returns></returns>
        public static Type MakeGenericType(this IGeneratorCode generatorCode, Type type, params Type[] typeArguments) => generatorCode?.GetGenericType(MakeGenericTypeName(type, typeArguments));

        public static string MakeGenericTypeName(Type type, params Type[] typeArguments)
        {
            if (type is null) { throw new ArgumentNullException(nameof(type)); }
            if (typeArguments is null) { throw new ArgumentNullException(nameof(typeArguments)); }

            var typeName = TypeNameFormatter.TypeName.GetFormattedName(type, TypeNameFormatter.TypeNameFormatOptions.Namespaces | TypeNameFormatter.TypeNameFormatOptions.NoKeywords | TypeNameFormatter.TypeNameFormatOptions.NoAnonymousTypes | TypeNameFormatter.TypeNameFormatOptions.NoGeneric);

            var args = string.Join(", ", typeArguments.Select(c => TypeNameFormatter.TypeName.GetFormattedName(c, TypeNameFormatter.TypeNameFormatOptions.Namespaces | TypeNameFormatter.TypeNameFormatOptions.NoKeywords | TypeNameFormatter.TypeNameFormatOptions.NoAnonymousTypes)));

            if (string.IsNullOrEmpty(args))
            {
                return typeName;
            }

            return $"{typeName}<{args}>";
        }
        */
    }
}
