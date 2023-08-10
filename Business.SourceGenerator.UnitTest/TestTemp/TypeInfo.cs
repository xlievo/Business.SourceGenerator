



using Business.SourceGenerator.Meta;
using Business.SourceGenerator;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks.Sources;
using System.Threading.Tasks;
using System.Threading;
using System.Linq.Expressions;
using System.Linq;
using System.Globalization;
using System.IO;
using System;

using Business.SourceGenerator.Meta;
using Business.SourceGenerator;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks.Sources;
using System.Threading.Tasks;
using System.Threading;
using System.Linq.Expressions;
using System.Linq;
using System.Globalization;
using System.IO;
using System;



















//namespace TypeInfoAssembly
//{
//    public partial class BusinessSourceGenerator : IGeneratorType
//    {
//        static readonly Lazy<BusinessSourceGenerator> generator = new Lazy<BusinessSourceGenerator>(() => new BusinessSourceGenerator());

//        public static IGeneratorType Generator { get => generator.Value; }

//        static readonly Lazy<IReadOnlyDictionary<Type, GeneratorTypeMeta>> generatorType = new Lazy<IReadOnlyDictionary<Type, GeneratorTypeMeta>>(() => new ReadOnlyDictionary<Type, GeneratorTypeMeta>(new Dictionary<Type, GeneratorTypeMeta>
//        {
//            #region NotImplementedException
//            [typeof(NotImplementedException)] = generatorTypeMeta.Value
//            #endregion

//        }));

//        public IReadOnlyDictionary<Type, GeneratorTypeMeta> GeneratorType { get => generatorType.Value; }
//    }
//}


//namespace TypeInfoAssembly
//{
//    public partial class BusinessSourceGenerator
//    {
//        static readonly Lazy<IAccessorType> System_NotImplementedException__NamedType_Class_None = new Lazy<IAccessorType>(() => new AccessorNamedType(Accessibility.Public, default, default, default, default, default, default, true, "NotImplementedException", "System.NotImplementedException", Kind.NamedType, default, default, default, default, default, SpecialType.None, default, default, NullableAnnotation.None, TypeKind.Class, default, AsyncType.None, default, default, default, default, default, default, true, default, default));

//        static readonly Lazy<GeneratorTypeMeta> generatorTypeMeta = new Lazy<GeneratorTypeMeta>(() => new GeneratorTypeMeta(new Dictionary<Type, Type> { [typeof(Business.SourceGenerator.Test.ResultObject<>)] = typeof(Business.SourceGenerator.Test.ResultObject<NotImplementedException>), [typeof(Business.SourceGenerator.Test.ResultObject3<>)] = typeof(Business.SourceGenerator.Test.ResultObject3<NotImplementedException>), [typeof(Business.SourceGenerator.Test.ResultObject2<>)] = typeof(Business.SourceGenerator.Test.ResultObject2<NotImplementedException>), [typeof(MyCode.MyStruct<>)] = typeof(MyCode.MyStruct<NotImplementedException>) }, new IMethodMeta[] { new Constructor(0, 0, default, (obj, m, args) => { var result = new NotImplementedException(); return result; }), new Constructor(1, 1, new IParameterMeta[] { new Parameter("message", typeof(string), TypeKind.Unknown, RefKind.None, default, 0, default, default, default, default) }, (obj, m, args) => { var result = new NotImplementedException(!m[0].c ? m[0].v as string : default); return result; }), new Constructor(2, 2, new IParameterMeta[] { new Parameter("message", typeof(string), TypeKind.Unknown, RefKind.None, default, 0, default, default, default, default), new Parameter("inner", typeof(Exception), TypeKind.Class, RefKind.None, default, 1, default, default, default, default) }, (obj, m, args) => { var result = new NotImplementedException(!m[0].c ? m[0].v as string : default, !m[1].c ? m[1].v as Exception : default); return result; }) }, default, true, TypeKind.Class, System_NotImplementedException__NamedType_Class_None.Value));
//    }
//}





//namespace TypeInfoAssembly
//{
//    public partial class BusinessSourceGenerator
//    {
//        static readonly Lazy<IAccessorType> System_String____ArrayType_Array_None = new Lazy<IAccessorType>(() => new AccessorType(Accessibility.NotApplicable, default, default, default, default, default, default, true, "", "System.String[]", Kind.ArrayType, default, default, default, default, default, SpecialType.None, default, default, NullableAnnotation.None, TypeKind.Array, default, AsyncType.None));
//    }
//}


//namespace TypeInfoAssembly
//{
//    public partial class BusinessSourceGenerator
//    {
//        static readonly Lazy<IAccessorType> Business_SourceGenerator_Test_GeneratorType2Attribute__NamedType_Class_None = new Lazy<IAccessorType>(() => new AccessorNamedType(Accessibility.Public, default, true, default, default, default, default, true, "GeneratorType2Attribute", "Business.SourceGenerator.Test.GeneratorType2Attribute", Kind.NamedType, true, new AccessorAttribute[] { new AccessorAttribute("AttributeUsageAttribute", typeof(AttributeUsageAttribute), new TypedConstant[] { new TypedConstant("validOn", typeof(AttributeTargets), default, TypedConstantKind.Enum, System.AttributeTargets.All) }, new TypedConstant[] { new TypedConstant("AllowMultiple", typeof(bool), default, TypedConstantKind.Primitive, true) }) }, new Dictionary<string, IAccessor> { { "A", new AccessorProperty(Accessibility.Public, default, default, default, default, default, default, true, "A", "Business.SourceGenerator.Test.GeneratorType2Attribute.A", Kind.Property, true, default, default, new AccessorNamedType(Accessibility.Public, default, true, default, default, default, default, true, "String", "System.String", Kind.NamedType, default, default, default, default, default, SpecialType.System_String, default, default, NullableAnnotation.None, TypeKind.Class, default, AsyncType.None, default, default, default, default, default, default, true, default, default), "System.String", RefKind.None, default, obj => ((Business.SourceGenerator.Test.GeneratorType2Attribute)obj).A, (ref object obj, object value) => { var obj2 = (Business.SourceGenerator.Test.GeneratorType2Attribute)obj; obj2.A = value as string; obj = obj2; }) } }, default, default, SpecialType.None, default, default, NullableAnnotation.None, TypeKind.Class, default, AsyncType.None, default, default, default, new IAccessorMethod[] { new AccessorMethod(Accessibility.Public, default, default, default, default, default, default, true, ".ctor", "Business.SourceGenerator.Test.GeneratorType2Attribute.GeneratorType2Attribute()", Kind.Method, true, default, default, default, default, MethodKind.Constructor, default, default, default, true, RefKind.None, new AccessorNamedType(Accessibility.Public, default, true, default, default, default, default, true, "Void", "System.Void", Kind.NamedType, default, default, default, default, true, SpecialType.System_Void, default, default, NullableAnnotation.NotAnnotated, TypeKind.Struct, default, AsyncType.None, default, default, default, default, default, default, default, default, default), default, default, 0, 0), new AccessorMethod(Accessibility.Public, default, default, default, default, default, default, true, ".ctor", "Business.SourceGenerator.Test.GeneratorType2Attribute.GeneratorType2Attribute(System.Type, System.String, System.Int32)", Kind.Method, true, default, new IAccessorParameter[] { new AccessorParameter(Accessibility.NotApplicable, default, default, default, default, default, default, true, "type", "Business.SourceGenerator.Test.GeneratorType2Attribute.GeneratorType2Attribute.type", Kind.Parameter, true, default, RefKind.None, default, default, default, default, new AccessorNamedType(Accessibility.Public, default, default, true, default, default, default, true, "Type", "System.Type", Kind.NamedType, default, default, default, default, default, SpecialType.None, default, default, NullableAnnotation.None, TypeKind.Class, default, AsyncType.None, default, default, default, default, default, default, default, default, default), NullableAnnotation.None, 0, default, default, default, typeof(Type), TypeKind.Class, default, default), new AccessorParameter(Accessibility.NotApplicable, default, default, default, default, default, default, true, "a", "Business.SourceGenerator.Test.GeneratorType2Attribute.GeneratorType2Attribute.a", Kind.Parameter, true, default, RefKind.None, default, true, default, default, new AccessorNamedType(Accessibility.Public, default, true, default, default, default, default, true, "String", "System.String", Kind.NamedType, default, default, default, default, default, SpecialType.System_String, default, default, NullableAnnotation.None, TypeKind.Class, default, AsyncType.None, default, default, default, default, default, default, true, default, default), NullableAnnotation.None, 1, true, "", true, typeof(string), TypeKind.Class, default, default), new AccessorParameter(Accessibility.NotApplicable, default, default, default, default, default, default, true, "b", "Business.SourceGenerator.Test.GeneratorType2Attribute.GeneratorType2Attribute.b", Kind.Parameter, true, default, RefKind.None, default, true, default, default, new AccessorNamedType(Accessibility.Public, default, true, default, default, default, default, true, "Int32", "System.Int32", Kind.NamedType, default, default, default, true, true, SpecialType.System_Int32, default, default, NullableAnnotation.NotAnnotated, TypeKind.Struct, default, AsyncType.None, default, default, default, default, default, default, true, default, default), NullableAnnotation.NotAnnotated, 2, true, 0, true, typeof(int), TypeKind.Struct, true, default) }, default, default, MethodKind.Constructor, default, default, default, true, RefKind.None, new AccessorNamedType(Accessibility.Public, default, true, default, default, default, default, true, "Void", "System.Void", Kind.NamedType, default, default, default, default, true, SpecialType.System_Void, default, default, NullableAnnotation.NotAnnotated, TypeKind.Struct, default, AsyncType.None, default, default, default, default, default, default, default, default, default), default, default, 3, 1) }, default, default, default, default, default));
//    }
//}



















namespace MyCode
{
    public partial struct TypeInfo<Type>
    {
        public System.Threading.Tasks.ValueTask<T> Test2<T>(T? a, System.DateTimeOffset? c, Type d, dynamic e)
        {
            var t = new System.Collections.Generic.Dictionary<T, Type>();

            var type = t.GetType();

            return System.Threading.Tasks.ValueTask.FromResult<T>(default);
        }
    }

    [Business.SourceGenerator.Meta.GeneratorType]
    public partial struct MyStruct<T>
    //where T : class
    {
        public string A { get; set; }

        public T B { get; set; }

        public MyStruct(string a)
        {
            _ = typeof(List<string>);
            this.A = a ?? throw new System.ArgumentNullException(nameof(a));
        }

        public System.Threading.Tasks.ValueTask<T> StructMethod<T2>(string? a, ref T b, out (int? c1, string? c2) c)
        //where T2 : class, System.Collections.Generic.IList<int>
        {
            //b.c1 = (T)888;
            c = (333, "xxx");
            return System.Threading.Tasks.ValueTask.FromResult(b);
        }
    }
}
