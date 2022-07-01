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
    using System.Linq;

    public static partial class Utils
    {
        static readonly System.Reflection.Assembly entryAssembly = System.Reflection.Assembly.GetEntryAssembly();

        static readonly IGeneratorCode generatorCode;

        /// <summary>
        /// BusinessSourceGenerator
        /// </summary>
        public const string Name = "BusinessSourceGenerator";

        static Utils()
        {
            var type = GetGeneratorCodeType(entryAssembly);

            if (null != type)
            {
                generatorCode = Activator.CreateInstance(type) as IGeneratorCode;
            }
        }

        /// <summary>
        /// Find IGeneratorCode in Assembly.GetEntryAssembly().
        /// </summary>
        public static IGeneratorCode GeneratorCode { get => generatorCode; }

        public static Type GetGeneratorCodeType(this System.Reflection.Assembly assembly) => assembly.GetType($"{entryAssembly?.GetName().Name}.{Name}", false);

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
        public static Type MakeGenericType(this IGeneratorCode generatorCode, Type type, params Type[] typeArguments) => generatorCode?.MakeGenericType(MakeGenericTypeName(type, typeArguments));

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
    }
}
