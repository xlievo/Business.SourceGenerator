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
    }
}
