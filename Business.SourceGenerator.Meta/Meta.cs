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

namespace Business.SourceGenerator.Meta
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public readonly struct Global
    {
        /// <summary>
        /// A string containing "\r\n" non-Unix platforms.
        /// </summary>
        public const string EnvironmentNewLine = "\r\n";

        /// <summary>
        /// "BusinessSourceGenerator"
        /// </summary>
        public const string GeneratorCodeName = "BusinessSourceGenerator";

        /// <summary>
        /// GeneratorTypeAttribute
        /// </summary>
        public const string GeneratorTypeKey = "Business.SourceGenerator.Meta.GeneratorTypeAttribute";

        /// <summary>
        /// Business.SourceGenerator.Meta.IGeneratorAccessor
        /// </summary>
        public const string AccessorKey = "Business.SourceGenerator.Meta.IGeneratorAccessor";

        /// <summary>
        /// Business.SourceGenerator.Meta
        /// </summary>
        public const string BusinessSourceGeneratorMeta = "Business.SourceGenerator.Meta";
    }

    #region IGeneratorCode, IGeneratorAccessor

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class GeneratorTypeAttribute : Attribute { }

    public interface IGeneratorType
    {
        /// <summary>
        /// IGeneratorType
        /// </summary>
        public IReadOnlyDictionary<Type, GeneratorTypeMeta> GeneratorType { get; }
    }

    /// <summary>
    /// IGeneratorAccessor.
    /// </summary>
    public interface IGeneratorAccessor
    {
        /// <summary>
        /// Metadata objects.
        /// </summary>
        /// <returns></returns>
        public IAccessorNamedType AccessorType();

        /// <summary>
        /// Sets the property or field value of a specified object.
        /// </summary>
        /// <param name="name">property or field name.</param>
        /// <param name="value">The new value.</param>
        /// <returns></returns>
        public bool AccessorSet(string name, object value);
    }

    /// <summary>
    /// GeneratorTypeMeta.
    /// </summary>
    public readonly struct GeneratorTypeMeta
    {
        /// <summary>
        /// GeneratorTypeMeta.
        /// </summary>
        /// <param name="makeGenerics"></param>
        /// <param name="constructors"></param>
        /// <param name="isDefinition"></param>
        /// <param name="noParameterConstructor"></param>
        /// <param name="typeKind"></param>
        public GeneratorTypeMeta(IReadOnlyDictionary<Type, Type> makeGenerics, IEnumerable<IMethodMeta> constructors, bool isCustom, bool noParameterConstructor, TypeKind typeKind)
        {
            MakeGenerics = makeGenerics;
            Constructors = constructors;
            IsCustom = isCustom;
            NoParameterConstructor = noParameterConstructor;
            TypeKind = typeKind;
        }

        /// <summary>
        /// MakeGenerics.
        /// </summary>
        public readonly IReadOnlyDictionary<Type, Type> MakeGenerics { get; }

        /// <summary>
        /// Constructors.
        /// </summary>
        public readonly IEnumerable<IMethodMeta> Constructors { get; }

        /// <summary>
        /// Whether to customize the object.
        /// </summary>
        public readonly bool IsCustom { get; }

        /// <summary>
        /// Is there a parameterless constructor.
        /// </summary>
        public readonly bool NoParameterConstructor { get; }

        /// <summary>
        /// An enumerated value that identifies whether this type is an array, pointer, enum,
        /// and so on.
        /// </summary>
        public readonly TypeKind TypeKind { get; }
    }

    public readonly struct Constructor : IMethodMeta
    {
        /// <summary>
        /// Skip the Out type to obtain the parameter count for this method. If the condition is not met, it returns 0.
        /// </summary>
        public readonly int ParametersRealLength { get; }

        /// <summary>
        /// Skip the Out type and default value and Params to obtain the parameter count for this method. If the condition is not met, it returns 0.
        /// </summary>
        public readonly int ParametersMustLength { get; }

        ///// <summary>
        ///// The position of the ref parameter in the complete parameter array.
        ///// </summary>
        //public int[] ParametersRefOrdinal { get; }

        public readonly IParameterMeta[] ParametersMeta { get; }

        /// <summary>
        /// Call this method.
        /// </summary>
        public readonly Func<object, CheckedParameterValue[], object[], object> Invoke { get; }

        /// <summary>
        /// Invoke this method asynchronously.
        /// </summary>
        public readonly Func<object, CheckedParameterValue[], object[], Task<object>> InvokeAsync { get; }

        public Constructor(int parametersRealLength, int parametersMustLength, IParameterMeta[] parameters, Func<object, CheckedParameterValue[], object[], object> invoke)
        {
            this.ParametersRealLength = parametersRealLength;
            this.ParametersMustLength = parametersMustLength;
            this.ParametersMeta = parameters;
            this.Invoke = invoke;
            this.InvokeAsync = default;
        }
    }

    public readonly struct Parameter : IParameterMeta
    {
        public Parameter(string name, Type runtimeType, TypeKind typeKind, RefKind refKind, bool isValueType, int ordinal, bool hasExplicitDefaultValue, object explicitDefaultValue, bool implicitDefaultValue, bool hasGenericType)
        {
            Name = name;
            RuntimeType = runtimeType;
            TypeKind = typeKind;

            RefKind = refKind;
            IsValueType = isValueType;
            Ordinal = ordinal;
            HasExplicitDefaultValue = hasExplicitDefaultValue;
            ExplicitDefaultValue = explicitDefaultValue;
            ImplicitDefaultValue = implicitDefaultValue;
            HasGenericType = hasGenericType;
        }

        /// <summary>
        /// Gets the symbol name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string Name { get; }

        /// <summary>
        /// Gets the runtime type of the parameter.
        /// </summary>
        public readonly Type RuntimeType { get; }

        /// <summary>
        /// An enumerated value that identifies whether this type is an array, pointer, enum,
        /// and so on.
        /// </summary>
        public readonly TypeKind TypeKind { get; }

        /// <summary>
        /// Whether the parameter passed by value or by reference.
        /// </summary>
        public readonly RefKind RefKind { get; }

        /// <summary>
        /// True if this type is known to be a value type.
        /// </summary>
        public readonly bool IsValueType { get; }

        /// <summary>
        /// Gets the ordinal position of the parameter. The first parameter has ordinal zero.
        /// The 'this' parameter ('Me' in Visual Basic) has ordinal -1.
        /// </summary>
        public int Ordinal { get; }

        /// <summary>
        /// Returns true if the parameter specifies a default value to be passed when no
        /// value is provided as an argument to a call. The default value can be obtained
        /// with the Microsoft.CodeAnalysis.IParameterSymbol.ExplicitDefaultValue property.
        /// </summary>
        public readonly bool HasExplicitDefaultValue { get; }

        /// <summary>
        /// Returns the default value of the parameter.
        /// </summary>
        /// <remarks>
        /// Returns null if the parameter type is a struct and the default value of the parameter
        /// is the default value of the struct type.
        /// </remarks>
        public readonly object ExplicitDefaultValue { get; }

        /// <summary>
        /// Whether the default parameter value.
        /// </summary>
        public readonly bool ImplicitDefaultValue { get; }

        /// <summary>
        /// Whether contain generic types.
        /// </summary>
        public readonly bool HasGenericType { get; }
    }

    public interface IMethodMeta
    {
        /// <summary>
        /// Skip the Out type to obtain the parameter count for this method. If the condition is not met, it returns 0.
        /// </summary>
        public int ParametersRealLength { get; }

        /// <summary>
        /// Skip the Out type and default value and Params to obtain the parameter count for this method. If the condition is not met, it returns 0.
        /// </summary>
        public int ParametersMustLength { get; }

        ///// <summary>
        ///// The position of the ref parameter in the complete parameter array.
        ///// </summary>
        //public int[] ParametersRefOrdinal { get; }

        /// <summary>
        /// Gets the parameters of this method. If this method has no parameters, returns
        /// an empty list.
        /// </summary>
        public IParameterMeta[] ParametersMeta { get; }

        /// <summary>
        /// Call this method.
        /// </summary>
        public Func<object, CheckedParameterValue[], object[], object> Invoke { get; }

        /// <summary>
        /// Invoke this method asynchronously.
        /// </summary>
        public Func<object, CheckedParameterValue[], object[], Task<object>> InvokeAsync { get; }
    }

    public interface IParameterMeta
    {
        /// <summary>
        /// Gets the symbol name. Returns the empty string if unnamed.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the runtime type of the parameter.
        /// </summary>
        public Type RuntimeType { get; }

        /// <summary>
        /// An enumerated value that identifies whether this type is an array, pointer, enum,
        /// and so on.
        /// </summary>
        public TypeKind TypeKind { get; }

        /// <summary>
        /// Whether the parameter passed by value or by reference.
        /// </summary>
        public RefKind RefKind { get; }

        /// <summary>
        /// True if this type is known to be a value type.
        /// </summary>
        public bool IsValueType { get; }

        /// <summary>
        /// Gets the ordinal position of the parameter. The first parameter has ordinal zero.
        /// The 'this' parameter ('Me' in Visual Basic) has ordinal -1.
        /// </summary>
        public int Ordinal { get; }

        /// <summary>
        /// Returns true if the parameter specifies a default value to be passed when no
        /// value is provided as an argument to a call. The default value can be obtained
        /// with the Microsoft.CodeAnalysis.IParameterSymbol.ExplicitDefaultValue property.
        /// </summary>
        public bool HasExplicitDefaultValue { get; }

        /// <summary>
        /// Returns the default value of the parameter.
        /// </summary>
        /// <remarks>
        /// Returns null if the parameter type is a struct and the default value of the parameter
        /// is the default value of the struct type.
        /// </remarks>
        public object ExplicitDefaultValue { get; }

        /// <summary>
        /// Whether the default parameter value.
        /// </summary>
        public bool ImplicitDefaultValue { get; }

        /// <summary>
        /// Whether contain generic types.
        /// </summary>
        public bool HasGenericType { get; }
    }

    /// <summary>
    /// Packing a ref or out parameter value.
    /// </summary>
    public readonly struct RefArg
    {
        /// <summary>
        /// Packing a ref parameter value.
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RefArg Ref<Type>(Type value = default) => new RefArg(value, typeof(Type), RefKind.Ref);

        /// <summary>
        /// Packing a ref parameter value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RefArg Ref(object value = default) => new RefArg(value, typeof(object), RefKind.Ref);

        /// <summary>
        /// Packing a out parameter value.
        /// </summary>
        /// <returns></returns>
        public static RefArg Out<Type>() => new RefArg(default, typeof(Type), RefKind.Out);

        /// <summary>
        /// Set value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public RefArg Set(object value = default) => new RefArg(value, Type, RefKind);

        /// <summary>
        /// Original parameter object.
        /// </summary>
        public readonly object Value { get; }

        public readonly Type Type { get; }

        /// <summary>
        /// RefKind.
        /// </summary>
        public readonly RefKind RefKind { get; }

        public RefArg(object value, Type type, RefKind refKind)
        {
            this.Value = value;
            this.Type = type;
            this.RefKind = refKind;
        }
    }

    #endregion

    public readonly struct CheckedParameterValue
    {
        /// <summary>
        /// Default value.
        /// </summary>
        public readonly object v;

        /// <summary>
        /// Whether the default parameter value.
        /// </summary>
        public readonly bool c;

        public CheckedParameterValue(object v, bool c)
        {
            this.v = v;
            this.c = c;
        }
    }

    /// <summary>
    /// Represents the nullability of values that can be assigned to an expression used
    /// as an lvalue.
    /// </summary>
    public enum NullableAnnotation : byte
    {
        /// <summary>
        /// The expression has not been analyzed, or the syntax is not an expression (such
        /// as a statement).
        /// </summary>
        /// <remarks>
        /// There are a few different reasons the expression could have not been analyzed:
        /// 1) The symbol producing the expression comes from a method that has not been
        /// annotated, such as invoking a C# 7.3 or earlier method, or a method in this compilation
        /// that is in a disabled context. 2) Nullable is completely disabled in this compilation.
        /// </remarks>
        None = 0,

        /// <summary>
        /// The expression is not annotated (does not have a ?).
        /// </summary>
        NotAnnotated = 1,

        /// <summary>
        /// The expression is annotated (does have a ?).
        /// </summary>
        Annotated = 2
    }

    /// <summary>
    /// Specifies the possible kinds of symbols.
    /// </summary>
    public enum Kind
    {
        /// <summary>
        /// Symbol is an alias.
        /// </summary>
        Alias = 0,

        /// <summary>
        /// Symbol is an array type.
        /// </summary>
        ArrayType = 1,

        /// <summary>
        /// Symbol is an assembly.
        /// </summary>
        Assembly = 2,

        /// <summary>
        /// Symbol is a dynamic type.
        /// </summary>
        DynamicType = 3,

        /// <summary>
        /// Symbol that represents an error
        /// </summary>
        ErrorType = 4,

        /// <summary>
        /// Symbol is an Event.
        /// </summary>
        Event = 5,

        /// <summary>
        /// Symbol is a field.
        /// </summary>
        Field = 6,

        /// <summary>
        /// Symbol is a label.
        /// </summary>
        Label = 7,

        /// <summary>
        /// Symbol is a local.
        /// </summary>
        Local = 8,

        /// <summary>
        /// Symbol is a method.
        /// </summary>
        Method = 9,

        /// <summary>
        /// Symbol is a netmodule.
        /// </summary>
        NetModule = 10,

        /// <summary>
        /// Symbol is a named type (e.g. class).
        /// </summary>
        NamedType = 11,

        /// <summary>
        /// Symbol is a namespace.
        /// </summary>
        Namespace = 12,

        /// <summary>
        /// Symbol is a parameter.
        /// </summary>
        Parameter = 13,

        /// <summary>
        /// Symbol is a pointer type.
        /// </summary>
        PointerType = 14,

        /// <summary>
        /// Symbol is a property.
        /// </summary>
        Property = 15,

        /// <summary>
        /// Symbol is a range variable of a query expression.
        /// </summary>
        RangeVariable = 16,

        /// <summary>
        /// Symbol is a type parameter.
        /// </summary>
        TypeParameter = 17,

        /// <summary>
        /// Symbol is a preprocessing/conditional compilation constant.
        /// </summary>
        Preprocessing = 18,

        /// <summary>
        /// Symbol represents a value that is discarded, e.g. in M(out _)
        /// </summary>
        Discard = 19,

        /// <summary>
        /// Symbol represents a function pointer type
        /// </summary>
        FunctionPointerType = 20
    }

    /// <summary>
    /// Enumeration for possible kinds of type symbols.
    /// </summary>
    public enum TypeKind : byte
    {
        /// <summary>
        /// Type's kind is undefined.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Type is an array type.
        /// </summary>
        Array = 1,

        /// <summary>
        /// Type is a class.
        /// </summary>
        Class = 2,

        /// <summary>
        /// Type is a delegate.
        /// </summary>
        Delegate = 3,

        /// <summary>
        /// Type is dynamic.
        /// </summary>
        Dynamic = 4,

        /// <summary>
        /// Type is an enumeration.
        /// </summary>
        Enum = 5,

        /// <summary>
        /// Type is an error type.
        /// </summary>
        Error = 6,

        /// <summary>
        /// Type is an interface.
        /// </summary>
        Interface = 7,

        /// <summary>
        /// Type is a module.
        /// </summary>
        Module = 8,

        /// <summary>
        /// Type is a pointer.
        /// </summary>
        Pointer = 9,

        /// <summary>
        /// Type is a C# struct or VB Structure
        /// </summary>
        Struct = 10,

        /// <summary>
        /// Type is a C# struct or VB Structure
        /// </summary>
        Structure = 10,

        /// <summary>
        /// Type is a type parameter.
        /// </summary>
        TypeParameter = 11,

        /// <summary>
        /// Type is an interactive submission.
        /// </summary>
        Submission = 12,

        /// <summary>
        /// Type is a function pointer.
        /// </summary>
        FunctionPointer = 13
    }

    /// <summary>
    /// Denotes the kind of reference.
    /// </summary>
    public enum RefKind : byte
    {
        /// <summary>
        /// Indicates a "value" parameter or return type.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates a "ref" parameter or return type.
        /// </summary>
        Ref = 1,

        /// <summary>
        /// Indicates an "out" parameter.
        /// </summary>
        Out = 2,

        /// <summary>
        /// Indicates an "in" parameter.
        /// </summary>
        In = 3,

        /// <summary>
        /// Indicates a "ref readonly" return type.
        /// </summary>
        RefReadOnly = 3
    }

    /// <summary>
    /// An enumeration declaring the kinds of variance supported for generic type parameters.
    /// </summary>
    public enum VarianceKind : short
    {
        /// <summary>
        /// Invariant.
        /// </summary>
        None = 0,

        /// <summary>
        /// Covariant (out).
        /// </summary>
        Out = 1,

        /// <summary>
        /// Contravariant (in).
        /// </summary>
        In = 2
    }

    /// <summary>
    /// Enumeration for possible kinds of method symbols.
    /// </summary>
    public enum MethodKind
    {
        /// <summary>
        /// An anonymous method or lambda expression
        /// </summary>
        AnonymousFunction = 0,

        LambdaMethod = 0,

        /// <summary>
        /// Method is a constructor.
        /// </summary>
        Constructor = 1,

        /// <summary>
        /// Method is a conversion.
        /// </summary>
        Conversion = 2,

        /// <summary>
        /// Method is a delegate invoke.
        /// </summary>
        DelegateInvoke = 3,

        /// <summary>
        /// Method is a destructor.
        /// </summary>
        Destructor = 4,

        /// <summary>
        /// Method is an event add.
        /// </summary>
        EventAdd = 5,

        /// <summary>
        /// Method is an event raise.
        /// </summary>
        EventRaise = 6,

        /// <summary>
        /// Method is an event remove.
        /// </summary>
        EventRemove = 7,

        /// <summary>
        /// Method is an explicit interface implementation.
        /// </summary>
        ExplicitInterfaceImplementation = 8,

        /// <summary>
        /// Method is an operator.
        /// </summary>
        UserDefinedOperator = 9,

        /// <summary>
        /// Method is an ordinary method.
        /// </summary>
        Ordinary = 10,

        /// <summary>
        /// Method is a property get.
        /// </summary>
        PropertyGet = 11,

        /// <summary>
        /// Method is a property set.
        /// </summary>
        PropertySet = 12,

        /// <summary>
        /// An extension method with the "this" parameter removed.
        /// </summary>
        ReducedExtension = 13,

        /// <summary>
        /// Method is a static constructor.
        /// </summary>
        StaticConstructor = 14,

        SharedConstructor = 14,

        /// <summary>
        /// A built-in operator.
        /// </summary>
        BuiltinOperator = 15,

        /// <summary>
        /// Declare Sub or Function.
        /// </summary>
        DeclareMethod = 16,

        /// <summary>
        /// Method is declared inside of another method.
        /// </summary>
        LocalFunction = 17,

        /// <summary>
        /// Method represents the signature of a function pointer type.
        /// </summary>
        FunctionPointerSignature = 18
    }

    /// <summary>
    /// Represents the different kinds of type parameters.
    /// </summary>
    public enum TypeParameterKind
    {
        /// <summary>
        /// Type parameter of a named type. For example: T in List<T>.
        /// </summary>
        Type = 0,

        /// <summary>
        /// Type parameter of a method. For example: T in void M<T>().
        /// </summary>
        Method = 1,

        /// <summary>
        /// Type parameter in a cref attribute in XML documentation comments. For example:
        /// T in <see cref="List{T}"/>.
        /// </summary>
        Cref = 2
    }

    /// <summary>
    /// Specifies the Ids of special runtime types.
    /// </summary>
    /// <remarks>
    /// Only types explicitly mentioned in "Co-located core types" spec (https://github.com/dotnet/roslyn/blob/main/docs/compilers/Co-located%20core%20types.md)
    /// can be in this enum. The following things should be in sync: 1) SpecialType enum
    /// 2) names in SpecialTypes.EmittedNames array.
    /// </remarks>
    public enum SpecialType : sbyte
    {
        /// <summary>
        /// Indicates a non-special type (default value).
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the type is System.Object.
        /// </summary>
        System_Object = 1,

        /// <summary>
        /// Indicates that the type is System.Enum.
        /// </summary>
        System_Enum = 2,

        /// <summary>
        /// Indicates that the type is System.MulticastDelegate.
        /// </summary>
        System_MulticastDelegate = 3,

        /// <summary>
        /// Indicates that the type is System.Delegate.
        /// </summary>
        System_Delegate = 4,

        /// <summary>
        /// Indicates that the type is System.ValueType.
        /// </summary>
        System_ValueType = 5,

        /// <summary>
        /// Indicates that the type is System.Void.
        /// </summary>
        System_Void = 6,

        /// <summary>
        /// Indicates that the type is System.Boolean.
        /// </summary>
        System_Boolean = 7,

        /// <summary>
        /// Indicates that the type is System.Char.
        /// </summary>
        System_Char = 8,

        /// <summary>
        /// Indicates that the type is System.SByte.
        /// </summary>
        System_SByte = 9,

        /// <summary>
        /// Indicates that the type is System.Byte.
        /// </summary>
        System_Byte = 10,

        /// <summary>
        /// Indicates that the type is System.Int16.
        /// </summary>
        System_Int16 = 11,

        /// <summary>
        /// Indicates that the type is System.UInt16.
        /// </summary>
        System_UInt16 = 12,

        /// <summary>
        /// Indicates that the type is System.Int32.
        /// </summary>
        System_Int32 = 13,

        /// <summary>
        /// Indicates that the type is System.UInt32.
        /// </summary>
        System_UInt32 = 14,

        /// <summary>
        /// Indicates that the type is System.Int64.
        /// </summary>
        System_Int64 = 15,

        /// <summary>
        /// Indicates that the type is System.UInt64.
        /// </summary>
        System_UInt64 = 16,

        /// <summary>
        /// Indicates that the type is System.Decimal.
        /// </summary>
        System_Decimal = 17,

        /// <summary>
        /// Indicates that the type is System.Single.
        /// </summary>
        System_Single = 18,

        /// <summary>
        /// Indicates that the type is System.Double.
        /// </summary>
        System_Double = 19,

        /// <summary>
        /// Indicates that the type is System.String.
        /// </summary>
        System_String = 20,

        /// <summary>
        /// Indicates that the type is System.IntPtr.
        /// </summary>
        System_IntPtr = 21,

        /// <summary>
        /// Indicates that the type is System.UIntPtr.
        /// </summary>
        System_UIntPtr = 22,

        /// <summary>
        /// Indicates that the type is System.Array.
        /// </summary>
        System_Array = 23,

        /// <summary>
        /// Indicates that the type is System.Collections.IEnumerable.
        /// </summary>
        System_Collections_IEnumerable = 24,

        /// <summary>
        /// Indicates that the type is System.Collections.Generic.IEnumerable`1.
        /// </summary>
        System_Collections_Generic_IEnumerable_T = 25,

        /// <summary>
        /// Indicates that the type is System.Collections.Generic.IList`1.
        /// </summary>
        System_Collections_Generic_IList_T = 26,

        /// <summary>
        /// Indicates that the type is System.Collections.Generic.ICollection`1.
        /// </summary>
        System_Collections_Generic_ICollection_T = 27,

        /// <summary>
        /// Indicates that the type is System.Collections.IEnumerator.
        /// </summary>
        System_Collections_IEnumerator = 28,

        /// <summary>
        /// Indicates that the type is System.Collections.Generic.IEnumerator`1.
        /// </summary>
        System_Collections_Generic_IEnumerator_T = 29,

        /// <summary>
        /// Indicates that the type is System.Collections.Generic.IReadOnlyList`1.
        /// </summary>
        System_Collections_Generic_IReadOnlyList_T = 30,

        /// <summary>
        /// Indicates that the type is System.Collections.Generic.IReadOnlyCollection`1.
        /// </summary>
        System_Collections_Generic_IReadOnlyCollection_T = 31,

        /// <summary>
        /// Indicates that the type is System.Nullable`1.
        /// </summary>
        System_Nullable_T = 32,

        /// <summary>
        /// Indicates that the type is System.DateTime.
        /// </summary>
        System_DateTime = 33,

        /// <summary>
        /// Indicates that the type is System.Runtime.CompilerServices.IsVolatile.
        /// </summary>
        System_Runtime_CompilerServices_IsVolatile = 34,

        /// <summary>
        /// Indicates that the type is System.IDisposable.
        /// </summary>
        System_IDisposable = 35,

        /// <summary>
        /// Indicates that the type is System.TypedReference.
        /// </summary>
        System_TypedReference = 36,

        /// <summary>
        /// Indicates that the type is System.ArgIterator.
        /// </summary>
        System_ArgIterator = 37,

        /// <summary>
        /// Indicates that the type is System.RuntimeArgumentHandle.
        /// </summary>
        System_RuntimeArgumentHandle = 38,

        /// <summary>
        /// Indicates that the type is System.RuntimeFieldHandle.
        /// </summary>
        System_RuntimeFieldHandle = 39,

        /// <summary>
        /// Indicates that the type is System.RuntimeMethodHandle.
        /// </summary>
        System_RuntimeMethodHandle = 40,

        /// <summary>
        /// Indicates that the type is System.RuntimeTypeHandle.
        /// </summary>
        System_RuntimeTypeHandle = 41,

        /// <summary>
        /// Indicates that the type is System.IAsyncResult.
        /// </summary>
        System_IAsyncResult = 42,

        /// <summary>
        /// Indicates that the type is System.AsyncCallback.
        /// </summary>
        System_AsyncCallback = 43,

        /// <summary>
        /// Indicates that the type is System.Runtime.CompilerServices.RuntimeFeature.
        /// </summary>
        System_Runtime_CompilerServices_RuntimeFeature = 44,

        /// <summary>
        /// An attribute that is placed on each method with a 'methodimpl" aka ".override"
        /// in metadata.
        /// </summary>
        System_Runtime_CompilerServices_PreserveBaseOverridesAttribute = 45,

        /// <summary>
        /// Count of special types. This is not a count of enum members.
        /// </summary>
        Count = 45
    }

    /// <summary>
    /// Enumeration for common accessibility combinations.
    /// </summary>
    public enum Accessibility
    {
        /// <summary>
        /// No accessibility specified.
        /// </summary>
        NotApplicable = 0,

        /// <summary>
        /// Only accessible where both protected and internal members are accessible (more
        /// restrictive than Microsoft.CodeAnalysis.Accessibility.Protected, Microsoft.CodeAnalysis.Accessibility.Internal
        /// and Microsoft.CodeAnalysis.Accessibility.ProtectedOrInternal).
        /// </summary>
        Private = 1,

        /// <summary>
        /// Only accessible where both protected and friend members are accessible (more
        /// restrictive than Microsoft.CodeAnalysis.Accessibility.Protected, Microsoft.CodeAnalysis.Accessibility.Friend
        /// and Microsoft.CodeAnalysis.Accessibility.ProtectedOrFriend).
        /// </summary>
        ProtectedAndInternal = 2,

        ProtectedAndFriend = 2,

        Protected = 3,

        Internal = 4,

        Friend = 4,

        /// <summary>
        /// Accessible wherever either protected or internal members are accessible (less
        /// restrictive than Microsoft.CodeAnalysis.Accessibility.Protected, Microsoft.CodeAnalysis.Accessibility.Internal
        /// and Microsoft.CodeAnalysis.Accessibility.ProtectedAndInternal).
        /// </summary>
        ProtectedOrInternal = 5,

        /// <summary>
        /// Accessible wherever either protected or friend members are accessible (less restrictive
        /// than Microsoft.CodeAnalysis.Accessibility.Protected, Microsoft.CodeAnalysis.Accessibility.Friend
        /// and Microsoft.CodeAnalysis.Accessibility.ProtectedAndFriend).
        /// </summary>
        ProtectedOrFriend = 5,

        Public = 6
    }

    /// <summary>
    /// Represents the kind of a TypedConstant.
    /// </summary>
    public enum TypedConstantKind
    {
        Error = 0, // error should be the default so that default(TypedConstant) is internally consistent
        Primitive = 1,
        Enum = 2,
        Type = 3,
        Array = 4
    }

    /// <summary>
    /// AsyncType
    /// </summary>
    public enum AsyncType
    {
        /// <summary>
        /// No
        /// </summary>
        None,

        /// <summary>
        /// Task
        /// </summary>
        Task,

        /// <summary>
        /// Task<>
        /// </summary>
        TaskGeneric,

        /// <summary>
        /// ValueTask
        /// </summary>
        ValueTask,

        /// <summary>
        /// ValueTask<>
        /// </summary>
        ValueTaskGeneric,

        /// <summary>
        /// Other
        /// </summary>
        Other,
    }

    public interface IAccessor { }

    /// <summary>
    /// Represents a symbol (namespace, class, method, parameter, etc.) exposed by the
    /// compiler.
    /// </summary>
    /// <remarks>
    /// This interface is reserved for implementation by its associated APIs. We reserve
    /// the right to change it in the future.
    /// </remarks>
    public interface IAccessorMeta : IAccessor
    {
        //Microsoft.CodeAnalysis.IAssemblySymbol ContainingAssembly { get; }

        /// <summary>
        /// Gets a Microsoft.CodeAnalysis.Accessibility indicating the declared accessibility
        /// for the symbol. Returns NotApplicable if no accessibility is declared.
        /// </summary>
        public Accessibility DeclaredAccessibility { get; }

        /*
        /// <summary>
        /// Returns true if this symbol can be referenced by its name in code.
        /// </summary>
        public bool CanBeReferencedByName { get; }

        /// <summary>
        /// Returns true if this symbol was automatically created by the compiler, and does
        /// not have an explicit corresponding source code declaration.
        /// </summary>
        /// <remarks>
        /// This is intended for symbols that are ordinary symbols in the language sense,
        /// and may be used by code, but that are simply declared implicitly rather than
        /// with explicit language syntax.
        /// Examples include (this list is not exhaustive):
        /// • The default constructor for a class or struct that is created if one is not
        /// provided.
        /// • The BeginInvoke/Invoke/EndInvoke methods for a delegate.
        /// • The generated backing field for an auto property or a field-like event.
        /// • The "this" parameter for non-static methods.
        /// • The "value" parameter for a property setter.
        /// • The parameters on indexer accessor methods (not on the indexer itself).
        /// • Methods in anonymous types.
        /// The class and entry point method for top-level statements are not considered
        /// as implicitly declared.
        /// </remarks>
        public bool IsImplicitlyDeclared { get; }
        */

        /// <summary>
        /// Gets a value indicating whether the symbol is defined externally.
        /// </summary>
        public bool IsExtern { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is sealed.
        /// </summary>
        public bool IsSealed { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is abstract.
        /// </summary>
        public bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is an override of a base class symbol.
        /// </summary>
        public bool IsOverride { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is virtual.
        /// </summary>
        public bool IsVirtual { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is static.
        /// </summary>
        public bool IsStatic { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is the original definition. Returns
        /// false if the symbol is derived from another symbol, by type substitution for
        /// instance.
        /// </summary>
        public bool IsDefinition { get; }

        /// <summary>
        /// Gets the object name. Returns the empty string if unnamed.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the object full name. Returns the empty string if unnamed.
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// Gets the Microsoft.CodeAnalysis.SymbolKind indicating what kind of symbol it is.
        /// </summary>
        public Kind Kind { get; }

        /// <summary>
        /// Whether the syntax node(s) where this symbol was declared in source.
        /// </summary>
        public bool IsDeclaringSyntaxReferences { get; }

        /// <summary>
        /// Returns the list of custom attributes, if any, associated with the returned value. 
        /// </summary>
        public AccessorAttribute[] Attributes { get; }
    }

    /// <summary>
    /// Represents a type.
    /// </summary>
    /// <remarks>
    /// This interface is reserved for implementation by its associated APIs. We reserve
    /// the right to change it in the future.
    /// </remarks>
    public interface IAccessorType : IAccessorMeta
    {
        /*
        /// <summary>
        /// Returns true if this symbol is a namespace. If it is not a namespace, it must
        /// be a type.
        /// </summary>
        public bool IsNamespace { get; }

        /// <summary>
        /// Returns true if this symbols is a type. If it is not a type, it must be a namespace.
        /// </summary>
        public bool IsType { get; }
        */

        /// <summary>
        /// Get all the members of this symbol.
        /// </summary>
        /// <remarks>
        /// An ImmutableArray containing all the members of this symbol. If this symbol has
        /// no members, returns an empty ImmutableArray. Never returns Null.
        /// </remarks>
        public IDictionary<string, IAccessor> Members { get; }

        ///// <summary>
        ///// True if this type is known to be a reference type. It is never the case that
        ///// Microsoft.CodeAnalysis.ITypeSymbol.IsReferenceType and Microsoft.CodeAnalysis.ITypeSymbol.IsValueType
        ///// both return true. However, for an unconstrained type parameter, Microsoft.CodeAnalysis.ITypeSymbol.IsReferenceType
        ///// and Microsoft.CodeAnalysis.ITypeSymbol.IsValueType will both return false.
        ///// </summary>
        //public bool IsReferenceType { get; }

        /// <summary>
        /// True if the type is readonly.
        /// </summary>
        public bool IsReadOnly { get; }

        /// <summary>
        /// True if the type is unmanaged according to language rules. False if managed or
        /// if the language has no concept of unmanaged types.
        /// </summary>
        public bool IsUnmanagedType { get; }

        ///// <summary>
        ///// True if the type is ref-like, meaning it follows rules similar to CLR by-ref
        ///// variables. False if the type is not ref-like or if the language has no concept
        ///// of ref-like types.
        ///// </summary>
        ///// <remarks>
        ///// System.Span`1 is a commonly used ref-like type.
        ///// </remarks>
        //public bool IsRefLikeType { get; }

        /// <summary>
        /// An enumerated value that identifies certain 'special' types such as System.Object.
        /// Returns Microsoft.CodeAnalysis.SpecialType.None if the type is not special.
        /// </summary>
        public SpecialType SpecialType { get; }

        //IAccessorType OriginalDefinition { get; }

        ///// <summary>
        ///// True if the type represents a native integer. In C#, the types represented by
        ///// language keywords 'nint' and 'nuint'.
        ///// </summary>
        //public bool IsNativeIntegerType { get; }

        /// <summary>
        /// Is this a symbol for a tuple .
        /// </summary>
        public bool IsTupleType { get; }

        /// <summary>
        /// Is this a symbol for an anonymous type (including anonymous VB delegate).
        /// </summary>
        public bool IsAnonymousType { get; }

        ///// <summary>
        ///// True if this type is known to be a value type. It is never the case that Microsoft.CodeAnalysis.ITypeSymbol.IsReferenceType
        ///// and Microsoft.CodeAnalysis.ITypeSymbol.IsValueType both return true. However,
        ///// for an unconstrained type parameter, Microsoft.CodeAnalysis.ITypeSymbol.IsReferenceType
        ///// and Microsoft.CodeAnalysis.ITypeSymbol.IsValueType will both return false.
        ///// </summary>
        //public bool IsValueType { get; }

        /// <summary>
        /// Nullable annotation associated with the type, or Microsoft.CodeAnalysis.NullableAnnotation.None
        /// if there are none.
        /// </summary>
        public NullableAnnotation NullableAnnotation { get; }

        ///// <summary>
        ///// The list of all interfaces of which this type is a declared subtype, excluding
        ///// this type itself. This includes all declared base interfaces, all declared base
        ///// interfaces of base types, and all declared base interfaces of those results (recursively).
        ///// This also is the effective interface set of a type parameter. Each result appears
        ///// exactly once in the list. This list is topologically sorted by the inheritance
        ///// relationship: if interface type A extends interface type B, then A precedes B
        ///// in the list. This is not quite the same as "all interfaces of which this type
        ///// is a proper subtype" because it does not take into account variance: AllInterfaces
        ///// for IEnumerable<string> will not include IEnumerable<object>.
        ///// </summary>
        //public IEnumerable<Type> AllInterfaces { get; }

        ///// <summary>
        ///// The declared base type of this type, or null. The object type, interface types,
        ///// and pointer types do not have a base type. The base type of a type parameter
        ///// is its effective base class.
        ///// </summary>
        //public Type BaseType { get; }

        /// <summary>
        /// An enumerated value that identifies whether this type is an array, pointer, enum,
        /// and so on.
        /// </summary>
        public TypeKind TypeKind { get; }

        /// <summary>
        /// True if the type is a record.
        /// </summary>
        public bool IsRecord { get; }

        ///// <summary>
        ///// Returns true if this method is an async method
        ///// </summary>
        //public bool IsAsync { get; }

        /// <summary>
        /// Returns asynchronous type if this method is an async method.
        /// </summary>
        public AsyncType AsyncType { get; }
    }

    /// <summary>
    /// Represents a type other than an array, a pointer, a type parameter.
    /// </summary>
    /// <remarks>
    /// This interface is reserved for implementation by its associated APIs. We reserve
    /// the right to change it in the future.
    /// </remarks>
    public interface IAccessorNamedType : IAccessorType
    {
        /// <summary>
        /// Returns the top-level nullability of the type arguments that have been substituted
        /// for the type parameters. If nothing has been substituted for a given type parameter,
        /// then Microsoft.CodeAnalysis.NullableAnnotation.None is returned for that type
        /// argument.
        /// </summary>
        public IEnumerable<NullableAnnotation> TypeArgumentNullableAnnotations { get; }

        /// <summary>
        /// Returns fields that represent tuple elements for types that are tuples. If this
        /// type is not a tuple, then returns default.
        /// </summary>
        public IEnumerable<IAccessorField> TupleElements { get; }

        ///// <summary>
        ///// If this is a tuple type with element names, returns the symbol for the tuple
        ///// type without names. Otherwise, returns null. The type argument corresponding
        ///// to the type of the extension field (VT[8].Rest), which is at the 8th (one based)
        ///// position is always a symbol for another tuple, rather than its underlying type.
        ///// </summary>
        //public IAccessorNamedType TupleUnderlyingType { get; }

        /// <summary>
        /// Determines if the symbol might contain extension methods. If false, the symbol
        /// does not contain extension methods.
        /// </summary>
        public bool MightContainExtensionMethods { get; }

        /// <summary>
        /// Get the both instance and static constructors for this type.
        /// </summary>
        public IEnumerable<IAccessorMethod> Constructors { get; }

        ///// <summary>
        ///// Get the static constructors for this type.
        ///// </summary>
        //public IEnumerable<IAccessorMethod> StaticConstructors { get; }

        ///// <summary>
        ///// Get the instance constructors for this type.
        ///// </summary>
        //public IEnumerable<IAccessorMethod> InstanceConstructors { get; }

        ///// <summary>
        ///// Returns the type symbol that this type was constructed from. This type symbol
        ///// has the same containing type (if any), but has type arguments that are the same
        ///// as the type parameters (although its containing type might not).
        ///// </summary>
        //public IAccessorNamedType ConstructedFrom { get; }

        /// <summary>
        /// For enum types, gets the underlying type. Returns null on all other kinds of
        /// types.
        /// </summary>
        public IAccessorNamedType EnumUnderlyingType { get; }

        /// <summary>
        /// For delegate types, gets the delegate's invoke method. Returns null on all other
        /// kinds of types. Note that it is possible to have an ill-formed delegate type
        /// imported from metadata which does not have an Invoke method. Such a type will
        /// be classified as a delegate but its DelegateInvokeMethod would be null.
        /// </summary>
        public IAccessorMethod DelegateInvokeMethod { get; }

        ///// <summary>
        ///// Get the original definition of this type symbol. If this symbol is derived from
        ///// another symbol by (say) type substitution, this gets the original symbol, as
        ///// it was defined in source or metadata.
        ///// </summary>
        //public IAccessorNamedType OriginalDefinition { get; }

        /// <summary>
        /// True if the type is serializable (has Serializable metadata flag).
        /// </summary>
        public bool IsSerializable { get; }

        /// <summary>
        /// Is the partial keyword declared
        /// </summary>
        public bool IsPartial { get; }

        ///// <summary>
        ///// If this is a native integer, returns the symbol for the underlying type, either
        ///// System.IntPtr or System.UIntPtr. Otherwise, returns null.
        ///// </summary>
        //public IAccessorNamedType NativeIntegerUnderlyingType { get; }

        ///// <summary>
        ///// Returns the type parameters that this type has. If this is a non-generic type,
        ///// returns an empty ImmutableArray.
        ///// </summary>
        //public IDictionary<string, IAccessorTypeParameter> TypeParameters { get; }

        ///// <summary>
        ///// Returns collection of names of members declared within this type.
        ///// </summary>
        //public IEnumerable<string> MemberNames { get; }

        /*
        /// <summary>
        /// Specifies that the class or interface is imported from another module. See System.Reflection.TypeAttributes.Import
        /// and System.Runtime.InteropServices.ComImportAttribute
        /// </summary>
        public bool IsComImport { get; }

        /// <summary>
        /// Returns true if the type is the implicit class that holds onto invalid global
        /// members (like methods or statements in a non script file).
        /// </summary>
        public bool IsImplicitClass { get; }

        /// <summary>
        /// Returns true if the type is a Script class. It might be an interactive submission
        /// class or a Script class in a csx file.
        /// </summary>
        public bool IsScriptClass { get; }
        */

        /*
        /// <summary>
        /// True if this is a reference to an unbound generic type. A generic type is considered
        /// unbound if all of the type argument lists in its fully qualified name are empty.
        /// Note that the type arguments of an unbound generic type will be returned as error
        /// types because they do not really have type arguments. An unbound generic type
        /// yields null for its BaseType and an empty result for its Interfaces.
        /// </summary>
        public bool IsUnboundGenericType { get; }

        /// <summary>
        /// True if this type or some containing type has type parameters.
        /// </summary>
        public bool IsGeneric { get; }
        */
        ///// <summary>
        ///// Returns the arity of this type, or the number of type parameters it takes. A
        ///// non-generic type has zero arity.
        ///// </summary>
        //public int Arity { get; }

        /// <summary>
        /// Returns the type arguments that have been substituted for the type parameters.
        /// If nothing has been substituted for a given type parameter, then the type parameter
        /// itself is considered the type argument.
        /// </summary>
        public IAccessorType[] TypeArguments { get; }
    }

    /// <summary>
    /// Represents a type parameter in a generic type or generic method.
    /// </summary>
    /// <remarks>
    /// This interface is reserved for implementation by its associated APIs. We reserve
    /// the right to change it in the future.
    /// </remarks>
    public interface IAccessorTypeParameter : IAccessorType
    {
        /// <summary>
        /// The ordinal position of the type parameter in the parameter list which declares
        /// it. The first type parameter has ordinal zero.
        /// </summary>
        public int Ordinal { get; }

        /// <summary>
        /// The variance annotation, if any, of the type parameter declaration. Type parameters
        /// may be declared as covariant (out), contravariant (in), or neither.
        /// </summary>
        public VarianceKind Variance { get; }

        /// <summary>
        /// The type parameter kind of this type parameter.
        /// </summary>
        public TypeParameterKind TypeParameterKind { get; }

        ///// <summary>
        ///// The method that declares the type parameter, or null.
        ///// </summary>
        //public IAccessorMethod DeclaringMethod { get; }

        ///// <summary>
        ///// The type that declares the type parameter, or null.
        ///// </summary>
        //public IAccessorNamedType DeclaringType { get; }

        /// <summary>
        /// True if the reference type constraint (class) was specified for the type parameter.
        /// </summary>
        public bool HasReferenceTypeConstraint { get; }

        /// <summary>
        /// If Microsoft.CodeAnalysis.ITypeParameterSymbol.HasReferenceTypeConstraint is
        /// true, returns the top-level nullability of the class constraint that was specified
        /// for the type parameter. If there was no class constraint, this returns Microsoft.CodeAnalysis.NullableAnnotation.None.
        /// </summary>
        public NullableAnnotation ReferenceTypeConstraintNullableAnnotation { get; }

        /// <summary>
        /// True if the value type constraint (struct) was specified for the type parameter.
        /// </summary>
        public bool HasValueTypeConstraint { get; }

        /// <summary>
        /// True if the value type constraint (unmanaged) was specified for the type parameter.
        /// </summary>
        public bool HasUnmanagedTypeConstraint { get; }

        /// <summary>
        /// True if the notnull constraint (notnull) was specified for the type parameter.
        /// </summary>
        public bool HasNotNullConstraint { get; }

        /// <summary>
        /// True if the parameterless constructor constraint (new()) was specified for the
        /// type parameter.
        /// </summary>
        public bool HasConstructorConstraint { get; }

        /// <summary>
        /// The types that were directly specified as constraints on the type parameter.
        /// </summary>
        public IEnumerable<IAccessorType> ConstraintTypes { get; }

        ///// <summary>
        ///// The top-level nullabilities that were directly specified as constraints on the
        ///// constraint types.
        ///// </summary>
        //public IEnumerable<NullableAnnotation> ConstraintNullableAnnotations { get; }

        ///// <summary>
        ///// If this is a type parameter of a reduced extension method, gets the type parameter
        ///// definition that this type parameter was reduced from. Otherwise, returns Nothing.
        ///// </summary>
        //public IAccessorTypeParameter ReducedFrom { get; }
    }

    /// <summary>
    /// Represents a parameter of a method or property.
    /// </summary>
    /// <remarks>
    /// This interface is reserved for implementation by its associated APIs. We reserve
    /// the right to change it in the future.
    /// </remarks>
    public interface IAccessorParameter : IAccessorMeta, IParameterMeta//, System.IEquatable<IAccessorMeta>
    {
        /// <summary>
        /// Returns true if the parameter was declared as a parameter array.
        /// </summary>
        public bool IsParams { get; }

        /// <summary>
        /// Returns true if the parameter is optional.
        /// </summary>
        public bool IsOptional { get; }

        /// <summary>
        /// Returns true if the parameter is the hidden 'this' ('Me' in Visual Basic) parameter.
        /// </summary>
        public bool IsThis { get; }

        /// <summary>
        /// Returns true if the parameter is a discard parameter.
        /// </summary>
        public bool IsDiscard { get; }

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        public IAccessorType Type { get; }

        /// <summary>
        /// Gets the top-level nullability of the parameter.
        /// </summary>
        public NullableAnnotation NullableAnnotation { get; }
    }

    public interface IAccessorMethodCollection : IAccessor, IEnumerable<IAccessorMethod> { }

    /// <summary>
    /// Represents a method or method-like symbol (including constructor, destructor,
    /// operator, or property/event accessor).
    /// </summary>
    /// <remarks>
    /// This interface is reserved for implementation by its associated APIs. We reserve
    /// the right to change it in the future.
    /// </remarks>
    public interface IAccessorMethod : IAccessorMeta, IMethodMeta
    {
        /*
        /// <summary>
        /// Indicates whether the method is readonly, i.e. whether the 'this' receiver parameter
        /// is 'ref readonly'. Returns true for readonly instance methods and accessors and
        /// for reduced extension methods with a 'this in' parameter.
        /// </summary>
        public bool IsReadOnly { get; }

        /// <summary>
        /// Returns true for 'init' set accessors, and false otherwise.
        /// </summary>
        public bool IsInitOnly { get; }
        */

        /// <summary>
        /// Gets the parameters of this method. If this method has no parameters, returns
        /// an empty list.
        /// </summary>
        public IAccessorParameter[] Parameters { get; }

        /// <summary>
        /// Return true if this is a partial method definition without a body. If there is
        /// an implementing body, it can be retrieved with Microsoft.CodeAnalysis.IMethodSymbol.PartialImplementationPart.
        /// </summary>
        public bool IsPartial { get; }

        /// <summary>
        /// Get the type parameters on this method. If the method has not generic, returns
        /// an empty list.
        /// </summary>
        public IDictionary<string, IAccessorTypeParameter> TypeParameters { get; }

        ///// <summary>
        ///// Returns the type arguments that have been substituted for the type parameters.
        ///// If nothing has been substituted for a given type parameter, then the type parameter
        ///// itself is consider the type argument.
        ///// </summary>
        //public IEnumerable<IAccessorTypeParameter> TypeArguments { get; }

        ///// <summary>
        ///// Returns a flag indicating whether this symbol has at least one applied/inherited
        ///// conditional attribute.
        ///// </summary>
        //public bool IsConditional { get; }

        /// <summary>
        /// Gets what kind of method this is. There are several different kinds of things
        /// in the C# language that are represented as methods. This property allow distinguishing
        /// those things without having to decode the name of the method.
        /// </summary>
        public MethodKind MethodKind { get; }

        ///// <summary>
        ///// Returns the arity of this method, or the number of type parameters it takes.
        ///// A non-generic method has zero arity.
        ///// </summary>
        //public int Arity { get; }

        /// <summary>
        /// Returns whether this method is generic; i.e., does it have any type parameters?
        /// </summary>
        public bool IsGeneric { get; }

        /// <summary>
        /// Returns true if this method is an extension method.
        /// </summary>
        public bool IsExtension { get; }

        /*
        /// <summary>
        /// Returns whether this method is using CLI VARARG calling convention. This is used
        /// for C-style variable argument lists. This is used extremely rarely in C# code
        /// and is represented using the undocumented "__arglist" keyword. Note that methods
        /// with "params" on the last parameter are indicated with the "IsParams" property
        /// on ParameterSymbol, and are not represented with this property.
        /// </summary>
        public bool IsVararg { get; }

        /// <summary>
        /// Returns whether this built-in operator checks for integer overflow.
        /// </summary>
        public bool IsCheckedBuiltin { get; }
        */

        /// <summary>
        /// Returns true if this method is an async method
        /// </summary>
        public bool IsAsync { get; }

        /// <summary>
        /// Returns true if this method has no return type; i.e., returns "void".
        /// </summary>
        public bool ReturnsVoid { get; }

        /*
        /// <summary>
        /// Returns true if this method returns by reference.
        /// </summary>
        public bool ReturnsByRef { get; }

        /// <summary>
        /// Returns true if this method returns by ref readonly.
        /// </summary>
        public bool ReturnsByRefReadonly { get; }
        */

        /// <summary>
        /// Returns the RefKind of the method.
        /// </summary>
        public RefKind RefKind { get; }

        /// <summary>
        /// Gets the return type of the method.
        /// </summary>
        public IAccessorType ReturnType { get; }

        /*
        /// <summary>
        /// Gets the top-level nullability of the return type of the method.
        /// </summary>
        public NullableAnnotation ReturnNullableAnnotation { get; }

        /// <summary>
        /// Returns true if this method hides base methods by name. This cannot be specified
        /// directly in the C# language, but can be true for methods defined in other languages
        /// imported from metadata. The equivalent of the "hidebyname" flag in metadata.
        /// </summary>
        public bool HidesBaseMethodsByName { get; }

        */
    }

    public delegate void SetValue(ref IGeneratorAccessor obj, object value);

    /// <summary>
    /// IAccessorMember
    /// </summary>
    public interface IAccessorMember : IAccessorMeta
    {
        /// <summary>
        /// Returns true if this field was declared as "readonly".
        /// True if this is a read-only property; that is, a property with no set accessor.
        /// </summary>
        public bool IsReadOnly { get; }

        /// <summary>
        /// Gets the type of this field.
        /// </summary>
        public IAccessorType Type { get; }

        /// <summary>
        /// Gets the type full name. Returns the empty string if unnamed.
        /// </summary>
        public string TypeFullName { get; }

        /*
        /// <summary>
        /// IsRepeat
        /// </summary>
        public bool IsRepeat { get; }
        */

        /// <summary>
        /// Get return value.
        /// </summary>
        public Func<IGeneratorAccessor, object> GetValue { get; }

        /// <summary>
        /// Set value.
        /// </summary>
        public SetValue SetValue { get; }
    }

    /// <summary>
    /// Represents a field in a class, struct or enum.
    /// </summary>
    /// <remarks>
    /// This interface is reserved for implementation by its associated APIs. We reserve
    /// the right to change it in the future.
    /// </remarks>
    public interface IAccessorField : IAccessorMember
    {
        /// <summary>
        /// Returns true if this field was declared as "const" (i.e. is a constant declaration).
        /// Also returns true for an enum member.
        /// </summary>
        public bool IsConst { get; }

        /// <summary>
        /// Returns true if this field was declared as "volatile".
        /// </summary>
        public bool IsVolatile { get; }

        /*
        /// <summary>
        /// Returns true if this field was declared as "fixed". Note that for a fixed-size
        /// buffer declaration, this.Type will be a pointer type, of which the pointed-to
        /// type will be the declared element type of the fixed-size buffer.
        /// </summary>
        public bool IsFixedSizeBuffer { get; }

        /// <summary>
        /// If IsFixedSizeBuffer is true, the value between brackets in the fixed-size-buffer
        /// declaration. If IsFixedSizeBuffer is false or there is an error (such as a bad
        /// constant value in source), FixedSize is 0. Note that for fixed-size buffer declaration,
        /// this.Type will be a pointer type, of which the pointed-to type will be the declared
        /// element type of the fixed-size buffer.
        /// </summary>
        public int FixedSize { get; }

        /// <summary>
        /// Gets the top-level nullability of this field.
        /// </summary>
        public NullableAnnotation NullableAnnotation { get; }
        */

        /// <summary>
        /// Returns false if the field wasn't declared as "const", or constant value was
        /// omitted or erroneous. True otherwise.
        /// </summary>
        public bool HasConstantValue { get; }

        /// <summary>
        /// Gets the constant value of this field
        /// </summary>
        public object ConstantValue { get; }

        /// <summary>
        /// Returns true if this field represents a tuple element which was given an explicit
        /// name.
        /// </summary>
        public bool IsExplicitlyNamedTupleElement { get; }
    }

    /// <summary>
    /// Represents a property or indexer.
    /// </summary>
    /// <remarks>
    /// This interface is reserved for implementation by its associated APIs. We reserve
    /// the right to change it in the future.
    /// </remarks>
    public interface IAccessorProperty : IAccessorMember
    {
        /*
        /// <summary>
        /// Returns whether the property is really an indexer.
        /// </summary>
        public bool IsIndexer { get; }

        ///// <summary>
        ///// The parameters of this property. If this property has no parameters, returns
        ///// an empty list. Parameters are only present on indexers, or on some properties
        ///// imported from a COM interface.
        ///// </summary>
        //public IEnumerable<IAccessorParameter> Parameters { get; }

        /// <summary>
        /// Custom modifiers associated with the ref modifier, or an empty array if there
        /// are none.
        /// </summary>
        public NullableAnnotation NullableAnnotation { get; }
        */

        /// <summary>
        /// Returns the RefKind of the property.
        /// </summary>
        public RefKind RefKind { get; }

        /*
        /// <summary>
        /// Returns true if this property returns by reference a readonly variable.
        /// </summary>
        public bool ReturnsByRefReadonly { get; }

        /// <summary>
        /// Returns true if this property returns by reference.
        /// </summary>
        public bool ReturnsByRef { get; }

        /// <summary>
        /// Returns true if this property is an auto-created WithEvents property that takes
        /// place of a field member when the field is marked as WithEvents.
        /// </summary>
        public bool IsWithEvents { get; }
        */

        /// <summary>
        /// True if this is a write-only property; that is, a property with no get accessor.
        /// </summary>
        public bool IsWriteOnly { get; }
    }

    /// <summary>
    /// Represents an event.
    /// </summary>
    /// <remarks>
    /// This interface is reserved for implementation by its associated APIs. We reserve
    /// the right to change it in the future.
    /// </remarks>
    public interface IAccessorEvent : IAccessorMember
    {
        /*
        /// <summary>
        /// The top-level nullability of the event.
        /// </summary>
        public NullableAnnotation NullableAnnotation { get; }
        */

        /// <summary>
        /// Returns true if the event is a WinRT type event.
        /// </summary>
        public bool IsWindowsRuntimeEvent { get; }

        /// <summary>
        /// The 'add' accessor of the event. Null only in error scenarios.
        /// </summary>
        public IAccessorMethod AddMethod { get; }

        /// <summary>
        /// The 'remove' accessor of the event. Null only in error scenarios.
        /// </summary>
        public IAccessorMethod RemoveMethod { get; }

        ///// <summary>
        ///// The 'raise' accessor of the event. Null if there is no raise method.
        ///// </summary>
        //public IAccessorMethod RaiseMethod { get; }

        ///// <summary>
        ///// Returns the overridden event, or null.
        ///// </summary>
        //public IAccessorEvent OverriddenEvent { get; }

        ///// <summary>
        ///// Returns interface properties explicitly implemented by this event.
        ///// </summary>
        ///// <remarks>
        ///// Properties imported from metadata can explicitly implement more than one event.
        ///// </remarks>
        //public IEnumerable<IAccessorEvent> ExplicitInterfaceImplementations { get; }
    }

    /// <summary>
    /// Represents a type.
    /// </summary>
    /// <remarks>
    /// This interface is reserved for implementation by its associated APIs. We reserve
    /// the right to change it in the future.
    /// </remarks>
    public readonly struct AccessorType : IAccessorType
    {
        public AccessorType(Accessibility declaredAccessibility, bool isExtern, bool isSealed, bool isAbstract, bool isOverride, bool isVirtual, bool isStatic, bool isDefinition, string name, string fullName, Kind kind, bool isDeclaringSyntaxReferences, AccessorAttribute[] attributes, IDictionary<string, IAccessor> members, bool isReadOnly, bool isUnmanagedType, SpecialType specialType, bool isTupleType, bool isAnonymousType, NullableAnnotation nullableAnnotation, TypeKind typeKind, bool isRecord, AsyncType asyncType)
        {
            DeclaredAccessibility = declaredAccessibility;
            IsExtern = isExtern;
            IsSealed = isSealed;
            IsAbstract = isAbstract;
            IsOverride = isOverride;
            IsVirtual = isVirtual;
            IsStatic = isStatic;
            IsDefinition = isDefinition;
            Name = name;
            FullName = fullName;
            Kind = kind;
            IsDeclaringSyntaxReferences = isDeclaringSyntaxReferences;
            Attributes = attributes;
            Members = members;
            //IsReferenceType = isReferenceType;
            IsReadOnly = isReadOnly;
            IsUnmanagedType = isUnmanagedType;
            //IsRefLikeType = isRefLikeType;
            SpecialType = specialType;
            //IsNativeIntegerType = isNativeIntegerType;
            IsTupleType = isTupleType;
            IsAnonymousType = isAnonymousType;
            //IsValueType = isValueType;
            NullableAnnotation = nullableAnnotation;
            TypeKind = typeKind;
            IsRecord = isRecord;
            AsyncType = asyncType;
        }

        public override string ToString() => $"{Kind} {FullName}";

        #region Meta

        /// <summary>
        /// Gets a Microsoft.CodeAnalysis.Accessibility indicating the declared accessibility
        /// for the symbol. Returns NotApplicable if no accessibility is declared.
        /// </summary>
        public readonly Accessibility DeclaredAccessibility { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is defined externally.
        /// </summary>
        public readonly bool IsExtern { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is sealed.
        /// </summary>
        public readonly bool IsSealed { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is abstract.
        /// </summary>
        public readonly bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is an override of a base class symbol.
        /// </summary>
        public readonly bool IsOverride { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is virtual.
        /// </summary>
        public readonly bool IsVirtual { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is static.
        /// </summary>
        public readonly bool IsStatic { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is the original definition. Returns
        /// false if the symbol is derived from another symbol, by type substitution for
        /// instance.
        /// </summary>
        public readonly bool IsDefinition { get; }

        /// <summary>
        /// Gets the symbol name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string Name { get; }

        /// <summary>
        /// Gets the symbol full name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string FullName { get; }

        /// <summary>
        /// Gets the Microsoft.CodeAnalysis.SymbolKind indicating what kind of symbol it is.
        /// </summary>
        public readonly Kind Kind { get; }

        /// <summary>
        /// Whether the syntax node(s) where this symbol was declared in source.
        /// </summary>
        public readonly bool IsDeclaringSyntaxReferences { get; }

        /// <summary>
        /// Returns the list of custom attributes, if any, associated with the returned value. 
        /// </summary>
        public AccessorAttribute[] Attributes { get; }

        #endregion

        #region IAccessorType

        /// <summary>
        /// Get all the members of this symbol.
        /// </summary>
        /// <remarks>
        /// An ImmutableArray containing all the members of this symbol. If this symbol has
        /// no members, returns an empty ImmutableArray. Never returns Null.
        /// </remarks>
        public readonly IDictionary<string, IAccessor> Members { get; }

        /// <summary>
        /// True if the type is readonly.
        /// </summary>
        public readonly bool IsReadOnly { get; }

        /// <summary>
        /// True if the type is unmanaged according to language rules. False if managed or
        /// if the language has no concept of unmanaged types.
        /// </summary>
        public readonly bool IsUnmanagedType { get; }

        /// <summary>
        /// An enumerated value that identifies certain 'special' types such as System.Object.
        /// Returns Microsoft.CodeAnalysis.SpecialType.None if the type is not special.
        /// </summary>
        public readonly SpecialType SpecialType { get; }

        /// <summary>
        /// Is this a symbol for a tuple .
        /// </summary>
        public readonly bool IsTupleType { get; }

        /// <summary>
        /// Is this a symbol for an anonymous type (including anonymous VB delegate).
        /// </summary>
        public readonly bool IsAnonymousType { get; }

        /// <summary>
        /// Nullable annotation associated with the type, or Microsoft.CodeAnalysis.NullableAnnotation.None
        /// if there are none.
        /// </summary>
        public readonly NullableAnnotation NullableAnnotation { get; }

        /// <summary>
        /// An enumerated value that identifies whether this type is an array, pointer, enum,
        /// and so on.
        /// </summary>
        public readonly TypeKind TypeKind { get; }

        /// <summary>
        /// True if the type is a record.
        /// </summary>
        public readonly bool IsRecord { get; }

        /// <summary>
        /// Returns asynchronous type if this method is an async method.
        /// </summary>
        public readonly AsyncType AsyncType { get; }

        #endregion
    }

    /// <summary>
    /// Represents a type other than an array, a pointer, a type parameter.
    /// </summary>
    /// <remarks>
    /// This interface is reserved for implementation by its associated APIs. We reserve
    /// the right to change it in the future.
    /// </remarks>
    public readonly struct AccessorNamedType : IAccessorNamedType
    {
        public AccessorNamedType(Accessibility declaredAccessibility, bool isExtern, bool isSealed, bool isAbstract, bool isOverride, bool isVirtual, bool isStatic, bool isDefinition, string name, string fullName, Kind kind, bool isDeclaringSyntaxReferences, AccessorAttribute[] attributes, IDictionary<string, IAccessor> members, bool isReadOnly, bool isUnmanagedType, SpecialType specialType, bool isTupleType, bool isAnonymousType, NullableAnnotation nullableAnnotation, TypeKind typeKind, bool isRecord, AsyncType asyncType, IEnumerable<NullableAnnotation> typeArgumentNullableAnnotations, IEnumerable<IAccessorField> tupleElements, bool mightContainExtensionMethods, IEnumerable<IAccessorMethod> constructors, IAccessorNamedType enumUnderlyingType, IAccessorMethod delegateInvokeMethod, bool isSerializable, bool isPartial, IAccessorType[] typeArguments)
        {
            DeclaredAccessibility = declaredAccessibility;
            IsExtern = isExtern;
            IsSealed = isSealed;
            IsAbstract = isAbstract;
            IsOverride = isOverride;
            IsVirtual = isVirtual;
            IsStatic = isStatic;
            IsDefinition = isDefinition;
            Name = name;
            FullName = fullName;
            Kind = kind;
            IsDeclaringSyntaxReferences = isDeclaringSyntaxReferences;
            Attributes = attributes;
            Members = members;
            IsReadOnly = isReadOnly;
            IsUnmanagedType = isUnmanagedType;
            SpecialType = specialType;
            IsTupleType = isTupleType;
            IsAnonymousType = isAnonymousType;
            NullableAnnotation = nullableAnnotation;
            TypeKind = typeKind;
            IsRecord = isRecord;
            AsyncType = asyncType;
            TypeArgumentNullableAnnotations = typeArgumentNullableAnnotations;
            TupleElements = tupleElements;
            MightContainExtensionMethods = mightContainExtensionMethods;
            Constructors = constructors;
            EnumUnderlyingType = enumUnderlyingType;
            DelegateInvokeMethod = delegateInvokeMethod;
            IsSerializable = isSerializable;
            IsPartial = isPartial;
            TypeArguments = typeArguments;
        }

        public override string ToString() => $"{Kind} {FullName}";

        #region Meta

        /// <summary>
        /// Gets a Microsoft.CodeAnalysis.Accessibility indicating the declared accessibility
        /// for the symbol. Returns NotApplicable if no accessibility is declared.
        /// </summary>
        public readonly Accessibility DeclaredAccessibility { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is defined externally.
        /// </summary>
        public readonly bool IsExtern { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is sealed.
        /// </summary>
        public readonly bool IsSealed { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is abstract.
        /// </summary>
        public readonly bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is an override of a base class symbol.
        /// </summary>
        public readonly bool IsOverride { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is virtual.
        /// </summary>
        public readonly bool IsVirtual { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is static.
        /// </summary>
        public readonly bool IsStatic { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is the original definition. Returns
        /// false if the symbol is derived from another symbol, by type substitution for
        /// instance.
        /// </summary>
        public readonly bool IsDefinition { get; }

        /// <summary>
        /// Gets the symbol name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string Name { get; }

        /// <summary>
        /// Gets the symbol full name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string FullName { get; }

        /// <summary>
        /// Gets the Microsoft.CodeAnalysis.SymbolKind indicating what kind of symbol it is.
        /// </summary>
        public readonly Kind Kind { get; }

        /// <summary>
        /// Whether the syntax node(s) where this symbol was declared in source.
        /// </summary>
        public readonly bool IsDeclaringSyntaxReferences { get; }

        /// <summary>
        /// Returns the list of custom attributes, if any, associated with the returned value. 
        /// </summary>
        public AccessorAttribute[] Attributes { get; }

        #endregion

        #region IAccessorType

        /// <summary>
        /// Get all the members of this symbol.
        /// </summary>
        /// <remarks>
        /// An ImmutableArray containing all the members of this symbol. If this symbol has
        /// no members, returns an empty ImmutableArray. Never returns Null.
        /// </remarks>
        public readonly IDictionary<string, IAccessor> Members { get; }

        /// <summary>
        /// True if the type is readonly.
        /// </summary>
        public readonly bool IsReadOnly { get; }

        /// <summary>
        /// True if the type is unmanaged according to language rules. False if managed or
        /// if the language has no concept of unmanaged types.
        /// </summary>
        public readonly bool IsUnmanagedType { get; }

        /// <summary>
        /// An enumerated value that identifies certain 'special' types such as System.Object.
        /// Returns Microsoft.CodeAnalysis.SpecialType.None if the type is not special.
        /// </summary>
        public readonly SpecialType SpecialType { get; }

        /// <summary>
        /// Is this a symbol for a tuple .
        /// </summary>
        public readonly bool IsTupleType { get; }

        /// <summary>
        /// Is this a symbol for an anonymous type (including anonymous VB delegate).
        /// </summary>
        public readonly bool IsAnonymousType { get; }

        /// <summary>
        /// Nullable annotation associated with the type, or Microsoft.CodeAnalysis.NullableAnnotation.None
        /// if there are none.
        /// </summary>
        public readonly NullableAnnotation NullableAnnotation { get; }

        /// <summary>
        /// An enumerated value that identifies whether this type is an array, pointer, enum,
        /// and so on.
        /// </summary>
        public readonly TypeKind TypeKind { get; }

        /// <summary>
        /// True if the type is a record.
        /// </summary>
        public readonly bool IsRecord { get; }

        /// <summary>
        /// Returns asynchronous type if this method is an async method.
        /// </summary>
        public readonly AsyncType AsyncType { get; }

        #endregion

        #region IAccessorNamedType

        /// <summary>
        /// Returns the top-level nullability of the type arguments that have been substituted
        /// for the type parameters. If nothing has been substituted for a given type parameter,
        /// then Microsoft.CodeAnalysis.NullableAnnotation.None is returned for that type
        /// argument.
        /// </summary>
        public readonly IEnumerable<NullableAnnotation> TypeArgumentNullableAnnotations { get; }

        /// <summary>
        /// Returns fields that represent tuple elements for types that are tuples. If this
        /// type is not a tuple, then returns default.
        /// </summary>
        public readonly IEnumerable<IAccessorField> TupleElements { get; }

        /// <summary>
        /// Determines if the symbol might contain extension methods. If false, the symbol
        /// does not contain extension methods.
        /// </summary>
        public readonly bool MightContainExtensionMethods { get; }

        /// <summary>
        /// Get the both instance and static constructors for this type.
        /// </summary>
        public readonly IEnumerable<IAccessorMethod> Constructors { get; }

        /// <summary>
        /// For enum types, gets the underlying type. Returns null on all other kinds of
        /// types.
        /// </summary>
        public readonly IAccessorNamedType EnumUnderlyingType { get; }

        /// <summary>
        /// For delegate types, gets the delegate's invoke method. Returns null on all other
        /// kinds of types. Note that it is possible to have an ill-formed delegate type
        /// imported from metadata which does not have an Invoke method. Such a type will
        /// be classified as a delegate but its DelegateInvokeMethod would be null.
        /// </summary>
        public readonly IAccessorMethod DelegateInvokeMethod { get; }

        /// <summary>
        /// True if the type is serializable (has Serializable metadata flag).
        /// </summary>
        public readonly bool IsSerializable { get; }

        /// <summary>
        /// Is the partial keyword declared
        /// </summary>
        public readonly bool IsPartial { get; }

        /// <summary>
        /// Returns the type arguments that have been substituted for the type parameters.
        /// If nothing has been substituted for a given type parameter, then the type parameter
        /// itself is considered the type argument.
        /// </summary>
        public readonly IAccessorType[] TypeArguments { get; }

        #endregion
    }

    /// <summary>
    /// Represents a type parameter in a generic type or generic method.
    /// </summary>
    /// <remarks>
    /// This interface is reserved for implementation by its associated APIs. We reserve
    /// the right to change it in the future.
    /// </remarks>
    public readonly struct AccessorTypeParameter : IAccessorTypeParameter
    {
        public AccessorTypeParameter(Accessibility declaredAccessibility, bool isExtern, bool isSealed, bool isAbstract, bool isOverride, bool isVirtual, bool isStatic, bool isDefinition, string name, string fullName, Kind kind, bool isDeclaringSyntaxReferences, AccessorAttribute[] attributes, IDictionary<string, IAccessor> members, bool isReadOnly, bool isUnmanagedType, SpecialType specialType, bool isTupleType, bool isAnonymousType, NullableAnnotation nullableAnnotation, TypeKind typeKind, bool isRecord, AsyncType asyncType, int ordinal, VarianceKind variance, TypeParameterKind typeParameterKind, bool hasReferenceTypeConstraint, NullableAnnotation referenceTypeConstraintNullableAnnotation, bool hasValueTypeConstraint, bool hasUnmanagedTypeConstraint, bool hasNotNullConstraint, bool hasConstructorConstraint, IEnumerable<IAccessorType> constraintTypes)
        {
            DeclaredAccessibility = declaredAccessibility;
            IsExtern = isExtern;
            IsSealed = isSealed;
            IsAbstract = isAbstract;
            IsOverride = isOverride;
            IsVirtual = isVirtual;
            IsStatic = isStatic;
            IsDefinition = isDefinition;
            Name = name;
            FullName = fullName;
            Kind = kind;
            IsDeclaringSyntaxReferences = isDeclaringSyntaxReferences;
            Attributes = attributes;
            Members = members;
            IsReadOnly = isReadOnly;
            IsUnmanagedType = isUnmanagedType;
            SpecialType = specialType;
            IsTupleType = isTupleType;
            IsAnonymousType = isAnonymousType;
            NullableAnnotation = nullableAnnotation;
            TypeKind = typeKind;
            IsRecord = isRecord;
            AsyncType = asyncType;
            Ordinal = ordinal;
            Variance = variance;
            TypeParameterKind = typeParameterKind;
            HasReferenceTypeConstraint = hasReferenceTypeConstraint;
            ReferenceTypeConstraintNullableAnnotation = referenceTypeConstraintNullableAnnotation;
            HasValueTypeConstraint = hasValueTypeConstraint;
            HasUnmanagedTypeConstraint = hasUnmanagedTypeConstraint;
            HasNotNullConstraint = hasNotNullConstraint;
            HasConstructorConstraint = hasConstructorConstraint;
            ConstraintTypes = constraintTypes;
        }

        public override string ToString() => $"{Kind} {FullName}";

        #region Meta

        /// <summary>
        /// Gets a Microsoft.CodeAnalysis.Accessibility indicating the declared accessibility
        /// for the symbol. Returns NotApplicable if no accessibility is declared.
        /// </summary>
        public readonly Accessibility DeclaredAccessibility { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is defined externally.
        /// </summary>
        public readonly bool IsExtern { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is sealed.
        /// </summary>
        public readonly bool IsSealed { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is abstract.
        /// </summary>
        public readonly bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is an override of a base class symbol.
        /// </summary>
        public readonly bool IsOverride { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is virtual.
        /// </summary>
        public readonly bool IsVirtual { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is static.
        /// </summary>
        public readonly bool IsStatic { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is the original definition. Returns
        /// false if the symbol is derived from another symbol, by type substitution for
        /// instance.
        /// </summary>
        public readonly bool IsDefinition { get; }

        /// <summary>
        /// Gets the symbol name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string Name { get; }

        /// <summary>
        /// Gets the symbol full name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string FullName { get; }

        /// <summary>
        /// Gets the Microsoft.CodeAnalysis.SymbolKind indicating what kind of symbol it is.
        /// </summary>
        public readonly Kind Kind { get; }

        /// <summary>
        /// Whether the syntax node(s) where this symbol was declared in source.
        /// </summary>
        public readonly bool IsDeclaringSyntaxReferences { get; }

        /// <summary>
        /// Returns the list of custom attributes, if any, associated with the returned value. 
        /// </summary>
        public AccessorAttribute[] Attributes { get; }

        #endregion

        #region IAccessorType

        /// <summary>
        /// Get all the members of this symbol.
        /// </summary>
        /// <remarks>
        /// An ImmutableArray containing all the members of this symbol. If this symbol has
        /// no members, returns an empty ImmutableArray. Never returns Null.
        /// </remarks>
        public readonly IDictionary<string, IAccessor> Members { get; }

        /// <summary>
        /// True if the type is readonly.
        /// </summary>
        public readonly bool IsReadOnly { get; }

        /// <summary>
        /// True if the type is unmanaged according to language rules. False if managed or
        /// if the language has no concept of unmanaged types.
        /// </summary>
        public readonly bool IsUnmanagedType { get; }

        /// <summary>
        /// An enumerated value that identifies certain 'special' types such as System.Object.
        /// Returns Microsoft.CodeAnalysis.SpecialType.None if the type is not special.
        /// </summary>
        public readonly SpecialType SpecialType { get; }

        /// <summary>
        /// Is this a symbol for a tuple .
        /// </summary>
        public readonly bool IsTupleType { get; }

        /// <summary>
        /// Is this a symbol for an anonymous type (including anonymous VB delegate).
        /// </summary>
        public readonly bool IsAnonymousType { get; }

        /// <summary>
        /// Nullable annotation associated with the type, or Microsoft.CodeAnalysis.NullableAnnotation.None
        /// if there are none.
        /// </summary>
        public readonly NullableAnnotation NullableAnnotation { get; }

        /// <summary>
        /// An enumerated value that identifies whether this type is an array, pointer, enum,
        /// and so on.
        /// </summary>
        public readonly TypeKind TypeKind { get; }

        /// <summary>
        /// True if the type is a record.
        /// </summary>
        public readonly bool IsRecord { get; }

        /// <summary>
        /// Returns asynchronous type if this method is an async method.
        /// </summary>
        public readonly AsyncType AsyncType { get; }

        #endregion

        #region IAccessorTypeParameter

        /// <summary>
        /// The ordinal position of the type parameter in the parameter list which declares
        /// it. The first type parameter has ordinal zero.
        /// </summary>
        public readonly int Ordinal { get; }

        /// <summary>
        /// The variance annotation, if any, of the type parameter declaration. Type parameters
        /// may be declared as covariant (out), contravariant (in), or neither.
        /// </summary>
        public readonly VarianceKind Variance { get; }

        /// <summary>
        /// The type parameter kind of this type parameter.
        /// </summary>
        public readonly TypeParameterKind TypeParameterKind { get; }

        /// <summary>
        /// True if the reference type constraint (class) was specified for the type parameter.
        /// </summary>
        public readonly bool HasReferenceTypeConstraint { get; }

        /// <summary>
        /// If Microsoft.CodeAnalysis.ITypeParameterSymbol.HasReferenceTypeConstraint is
        /// true, returns the top-level nullability of the class constraint that was specified
        /// for the type parameter. If there was no class constraint, this returns Microsoft.CodeAnalysis.NullableAnnotation.None.
        /// </summary>
        public readonly NullableAnnotation ReferenceTypeConstraintNullableAnnotation { get; }

        /// <summary>
        /// True if the value type constraint (struct) was specified for the type parameter.
        /// </summary>
        public readonly bool HasValueTypeConstraint { get; }

        /// <summary>
        /// True if the value type constraint (unmanaged) was specified for the type parameter.
        /// </summary>
        public readonly bool HasUnmanagedTypeConstraint { get; }

        /// <summary>
        /// True if the notnull constraint (notnull) was specified for the type parameter.
        /// </summary>
        public readonly bool HasNotNullConstraint { get; }

        /// <summary>
        /// True if the parameterless constructor constraint (new()) was specified for the
        /// type parameter.
        /// </summary>
        public readonly bool HasConstructorConstraint { get; }

        /// <summary>
        /// The types that were directly specified as constraints on the type parameter.
        /// </summary>
        public readonly IEnumerable<IAccessorType> ConstraintTypes { get; }

        #endregion
    }

    /// <summary>
    /// Represents a parameter of a method or property.
    /// </summary>
    /// <remarks>
    /// This interface is reserved for implementation by its associated APIs. We reserve
    /// the right to change it in the future.
    /// </remarks>
    public readonly struct AccessorParameter : IAccessorParameter
    {
        public AccessorParameter(Accessibility declaredAccessibility, bool isExtern, bool isSealed, bool isAbstract, bool isOverride, bool isVirtual, bool isStatic, bool isDefinition, string name, string fullName, Kind kind, bool isDeclaringSyntaxReferences, AccessorAttribute[] attributes, RefKind refKind, bool isParams, bool isOptional, bool isThis, bool isDiscard, IAccessorType type, NullableAnnotation nullableAnnotation, int ordinal, bool hasExplicitDefaultValue, object explicitDefaultValue, bool implicitDefaultValue, Type runtimeType, TypeKind typeKind, bool isValueType, bool hasGenericType)
        {
            DeclaredAccessibility = declaredAccessibility;
            IsExtern = isExtern;
            IsSealed = isSealed;
            IsAbstract = isAbstract;
            IsOverride = isOverride;
            IsVirtual = isVirtual;
            IsStatic = isStatic;
            IsDefinition = isDefinition;
            Name = name;
            FullName = fullName;
            Kind = kind;
            IsDeclaringSyntaxReferences = isDeclaringSyntaxReferences;
            Attributes = attributes;
            RefKind = refKind;
            IsParams = isParams;
            IsOptional = isOptional;
            IsThis = isThis;
            IsDiscard = isDiscard;
            Type = type;
            NullableAnnotation = nullableAnnotation;

            Ordinal = ordinal;
            HasExplicitDefaultValue = hasExplicitDefaultValue;
            ExplicitDefaultValue = explicitDefaultValue;
            ImplicitDefaultValue = implicitDefaultValue;

            RuntimeType = runtimeType;
            TypeKind = typeKind;
            IsValueType = isValueType;
            HasGenericType = hasGenericType;
        }

        public override string ToString() => $"{Kind} {FullName}";

        #region Meta

        /// <summary>
        /// Gets a Microsoft.CodeAnalysis.Accessibility indicating the declared accessibility
        /// for the symbol. Returns NotApplicable if no accessibility is declared.
        /// </summary>
        public readonly Accessibility DeclaredAccessibility { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is defined externally.
        /// </summary>
        public readonly bool IsExtern { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is sealed.
        /// </summary>
        public readonly bool IsSealed { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is abstract.
        /// </summary>
        public readonly bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is an override of a base class symbol.
        /// </summary>
        public readonly bool IsOverride { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is virtual.
        /// </summary>
        public readonly bool IsVirtual { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is static.
        /// </summary>
        public readonly bool IsStatic { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is the original definition. Returns
        /// false if the symbol is derived from another symbol, by type substitution for
        /// instance.
        /// </summary>
        public readonly bool IsDefinition { get; }

        /// <summary>
        /// Gets the symbol name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string Name { get; }

        /// <summary>
        /// Gets the symbol full name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string FullName { get; }

        /// <summary>
        /// Gets the Microsoft.CodeAnalysis.SymbolKind indicating what kind of symbol it is.
        /// </summary>
        public readonly Kind Kind { get; }

        /// <summary>
        /// Whether the syntax node(s) where this symbol was declared in source.
        /// </summary>
        public readonly bool IsDeclaringSyntaxReferences { get; }

        /// <summary>
        /// Returns the list of custom attributes, if any, associated with the returned value. 
        /// </summary>
        public AccessorAttribute[] Attributes { get; }

        #endregion

        #region IAccessorParameter

        /// <summary>
        /// Whether the parameter passed by value or by reference.
        /// </summary>
        public readonly RefKind RefKind { get; }

        /// <summary>
        /// Returns true if the parameter was declared as a parameter array.
        /// </summary>
        public readonly bool IsParams { get; }

        /// <summary>
        /// Returns true if the parameter is optional.
        /// </summary>
        public readonly bool IsOptional { get; }

        /// <summary>
        /// Returns true if the parameter is the hidden 'this' ('Me' in Visual Basic) parameter.
        /// </summary>
        public readonly bool IsThis { get; }

        /// <summary>
        /// Returns true if the parameter is a discard parameter.
        /// </summary>
        public readonly bool IsDiscard { get; }

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        public readonly IAccessorType Type { get; }

        /// <summary>
        /// Gets the top-level nullability of the parameter.
        /// </summary>
        public readonly NullableAnnotation NullableAnnotation { get; }

        /// <summary>
        /// Gets the ordinal position of the parameter. The first parameter has ordinal zero.
        /// The 'this' parameter ('Me' in Visual Basic) has ordinal -1.
        /// </summary>
        public readonly int Ordinal { get; }

        /// <summary>
        /// Returns true if the parameter specifies a default value to be passed when no
        /// value is provided as an argument to a call. The default value can be obtained
        /// with the Microsoft.CodeAnalysis.IParameterSymbol.ExplicitDefaultValue property.
        /// </summary>
        public readonly bool HasExplicitDefaultValue { get; }

        /// <summary>
        /// Returns the default value of the parameter.
        /// </summary>
        /// <remarks>
        /// Returns null if the parameter type is a struct and the default value of the parameter
        /// is the default value of the struct type.
        /// </remarks>
        public readonly object ExplicitDefaultValue { get; }

        /// <summary>
        /// Whether Implicit default value.
        /// </summary>
        public readonly bool ImplicitDefaultValue { get; }

        #endregion

        #region IParameterMeta

        /// <summary>
        /// Gets the runtime type of the parameter.
        /// </summary>
        public readonly Type RuntimeType { get; }

        /// <summary>
        /// An enumerated value that identifies whether this type is an array, pointer, enum,
        /// and so on.
        /// </summary>
        public readonly TypeKind TypeKind { get; }

        /// <summary>
        /// True if this type is known to be a value type.
        /// </summary>
        public readonly bool IsValueType { get; }

        /// <summary>
        /// Whether contain generic types.
        /// </summary>
        public readonly bool HasGenericType { get; }

        #endregion
    }

    /// <summary>
    /// Represents a method collection.
    /// </summary>
    public readonly struct AccessorMethodCollection : IAccessorMethodCollection
    {
        readonly IEnumerable<IAccessorMethod> accessorMethod;

        public AccessorMethodCollection(IEnumerable<IAccessorMethod> accessorMethod) => this.accessorMethod = accessorMethod;

        public IEnumerator<IAccessorMethod> GetEnumerator() => accessorMethod?.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => "Business.SourceGenerator.Meta.AccessorMethodCollection";
    }

    /// <summary>
    /// Represents a method or method-like symbol (including constructor, destructor,
    /// operator, or property/event accessor).
    /// </summary>
    /// <remarks>
    /// This interface is reserved for implementation by its associated APIs. We reserve
    /// the right to change it in the future.
    /// </remarks>
    public readonly struct AccessorMethod : IAccessorMethod
    {
        public AccessorMethod(Accessibility declaredAccessibility, bool isExtern, bool isSealed, bool isAbstract, bool isOverride, bool isVirtual, bool isStatic, bool isDefinition, string name, string fullName, Kind kind, bool isDeclaringSyntaxReferences, AccessorAttribute[] attributes, IAccessorParameter[] parameters, bool isPartial, IDictionary<string, IAccessorTypeParameter> typeParameters, MethodKind methodKind, bool isGeneric, bool isExtension, bool isAsync, bool returnsVoid, RefKind refKind, IAccessorType returnType, Func<object, CheckedParameterValue[], object[], object> invoke, Func<object, CheckedParameterValue[], object[], Task<object>> invokeAsync, int parametersRealLength, int parametersMustLength)
        {
            DeclaredAccessibility = declaredAccessibility;
            IsExtern = isExtern;
            IsSealed = isSealed;
            IsAbstract = isAbstract;
            IsOverride = isOverride;
            IsVirtual = isVirtual;
            IsStatic = isStatic;
            IsDefinition = isDefinition;
            Name = name;
            FullName = fullName;
            Kind = kind;
            IsDeclaringSyntaxReferences = isDeclaringSyntaxReferences;
            Attributes = attributes;
            Parameters = parameters;
            IsPartial = isPartial;
            TypeParameters = typeParameters;
            MethodKind = methodKind;
            IsGeneric = isGeneric;
            IsExtension = isExtension;
            IsAsync = isAsync;
            ReturnsVoid = returnsVoid;
            RefKind = refKind;
            ReturnType = returnType;
            //IsClone = isClone;
            //ReceiverType = receiverType;
            Invoke = invoke;
            InvokeAsync = invokeAsync;

            ParametersRealLength = parametersRealLength;
            ParametersMustLength = parametersMustLength;
        }

        public override string ToString() => $"{Kind} {FullName}";

        #region Meta

        /// <summary>
        /// Gets a Microsoft.CodeAnalysis.Accessibility indicating the declared accessibility
        /// for the symbol. Returns NotApplicable if no accessibility is declared.
        /// </summary>
        public readonly Accessibility DeclaredAccessibility { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is defined externally.
        /// </summary>
        public readonly bool IsExtern { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is sealed.
        /// </summary>
        public readonly bool IsSealed { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is abstract.
        /// </summary>
        public readonly bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is an override of a base class symbol.
        /// </summary>
        public readonly bool IsOverride { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is virtual.
        /// </summary>
        public readonly bool IsVirtual { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is static.
        /// </summary>
        public readonly bool IsStatic { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is the original definition. Returns
        /// false if the symbol is derived from another symbol, by type substitution for
        /// instance.
        /// </summary>
        public readonly bool IsDefinition { get; }

        /// <summary>
        /// Gets the symbol name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string Name { get; }

        /// <summary>
        /// Gets the symbol full name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string FullName { get; }

        /// <summary>
        /// Gets the Microsoft.CodeAnalysis.SymbolKind indicating what kind of symbol it is.
        /// </summary>
        public readonly Kind Kind { get; }

        /// <summary>
        /// Whether the syntax node(s) where this symbol was declared in source.
        /// </summary>
        public readonly bool IsDeclaringSyntaxReferences { get; }

        /// <summary>
        /// Returns the list of custom attributes, if any, associated with the returned value. 
        /// </summary>
        public AccessorAttribute[] Attributes { get; }

        #endregion

        #region IAccessorMethod

        /// <summary>
        /// Gets the parameters of this method. If this method has no parameters, returns
        /// an empty list.
        /// </summary>
        public readonly IAccessorParameter[] Parameters { get; }

        /// <summary>
        /// Return true if this is a partial method definition without a body. If there is
        /// an implementing body, it can be retrieved with Microsoft.CodeAnalysis.IMethodSymbol.PartialImplementationPart.
        /// </summary>
        public readonly bool IsPartial { get; }

        /// <summary>
        /// Get the type parameters on this method. If the method has not generic, returns
        /// an empty list.
        /// </summary>
        public readonly IDictionary<string, IAccessorTypeParameter> TypeParameters { get; }

        /// <summary>
        /// Gets what kind of method this is. There are several different kinds of things
        /// in the C# language that are represented as methods. This property allow distinguishing
        /// those things without having to decode the name of the method.
        /// </summary>
        public readonly MethodKind MethodKind { get; }

        /// <summary>
        /// Returns whether this method is generic; i.e., does it have any type parameters?
        /// </summary>
        public readonly bool IsGeneric { get; }

        /// <summary>
        /// Returns true if this method is an extension method.
        /// </summary>
        public readonly bool IsExtension { get; }

        /// <summary>
        /// Returns true if this method is an async method
        /// </summary>
        public readonly bool IsAsync { get; }

        /// <summary>
        /// Returns true if this method has no return type; i.e., returns "void".
        /// </summary>
        public readonly bool ReturnsVoid { get; }

        /// <summary>
        /// Returns the RefKind of the method.
        /// </summary>
        public readonly RefKind RefKind { get; }

        /// <summary>
        /// Gets the return type of the method.
        /// </summary>
        public readonly IAccessorType ReturnType { get; }

        ///// <summary>
        ///// Is it a clone.
        ///// </summary>
        //public bool IsClone { get; }

        ///// <summary>
        ///// If this method can be applied to an object, returns the type of object it is
        ///// applied to.
        ///// </summary>
        //public string ReceiverType { get; }

        #endregion

        #region IMethodMeta

        /// <summary>
        /// Skip the Out type to obtain the parameter count for this method. If the condition is not met, it returns 0.
        /// </summary>
        public readonly int ParametersRealLength { get; }

        /// <summary>
        /// Skip the Out type and default value and Params to obtain the parameter count for this method. If the condition is not met, it returns 0.
        /// </summary>
        public readonly int ParametersMustLength { get; }

        public readonly IParameterMeta[] ParametersMeta => Parameters;

        /// <summary>
        /// Call this method.
        /// </summary>
        public readonly Func<object, CheckedParameterValue[], object[], object> Invoke { get; }

        /// <summary>
        /// Invoke this method asynchronously.
        /// </summary>
        public readonly Func<object, CheckedParameterValue[], object[], Task<object>> InvokeAsync { get; }

        #endregion
    }

    /// <summary>
    /// Represents a field in a class, struct or enum.
    /// </summary>
    /// <remarks>
    /// This interface is reserved for implementation by its associated APIs. We reserve
    /// the right to change it in the future.
    /// </remarks>
    public readonly struct AccessorField : IAccessorField
    {
        public AccessorField(Accessibility declaredAccessibility, bool isExtern, bool isSealed, bool isAbstract, bool isOverride, bool isVirtual, bool isStatic, bool isDefinition, string name, string fullName, Kind kind, bool isDeclaringSyntaxReferences, AccessorAttribute[] attributes, bool isReadOnly, IAccessorType type, string typeFullName, bool isConst, bool isVolatile, bool hasConstantValue, object constantValue, bool isExplicitlyNamedTupleElement, Func<IGeneratorAccessor, object> getValue, SetValue setValue)
        {
            DeclaredAccessibility = declaredAccessibility;
            IsExtern = isExtern;
            IsSealed = isSealed;
            IsAbstract = isAbstract;
            IsOverride = isOverride;
            IsVirtual = isVirtual;
            IsStatic = isStatic;
            IsDefinition = isDefinition;
            Name = name;
            FullName = fullName;
            Kind = kind;
            IsDeclaringSyntaxReferences = isDeclaringSyntaxReferences;
            Attributes = attributes;
            IsReadOnly = isReadOnly;
            Type = type;
            TypeFullName = typeFullName;
            IsConst = isConst;
            IsVolatile = isVolatile;
            HasConstantValue = hasConstantValue;
            ConstantValue = constantValue;
            IsExplicitlyNamedTupleElement = isExplicitlyNamedTupleElement;
            GetValue = getValue;
            SetValue = setValue;
        }

        public override string ToString() => $"{Kind} {FullName}";

        #region Meta

        /// <summary>
        /// Gets a Microsoft.CodeAnalysis.Accessibility indicating the declared accessibility
        /// for the symbol. Returns NotApplicable if no accessibility is declared.
        /// </summary>
        public readonly Accessibility DeclaredAccessibility { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is defined externally.
        /// </summary>
        public readonly bool IsExtern { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is sealed.
        /// </summary>
        public readonly bool IsSealed { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is abstract.
        /// </summary>
        public readonly bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is an override of a base class symbol.
        /// </summary>
        public readonly bool IsOverride { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is virtual.
        /// </summary>
        public readonly bool IsVirtual { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is static.
        /// </summary>
        public readonly bool IsStatic { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is the original definition. Returns
        /// false if the symbol is derived from another symbol, by type substitution for
        /// instance.
        /// </summary>
        public readonly bool IsDefinition { get; }

        /// <summary>
        /// Gets the symbol name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string Name { get; }

        /// <summary>
        /// Gets the symbol full name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string FullName { get; }

        /// <summary>
        /// Gets the Microsoft.CodeAnalysis.SymbolKind indicating what kind of symbol it is.
        /// </summary>
        public readonly Kind Kind { get; }

        /// <summary>
        /// Whether the syntax node(s) where this symbol was declared in source.
        /// </summary>
        public readonly bool IsDeclaringSyntaxReferences { get; }

        /// <summary>
        /// Returns the list of custom attributes, if any, associated with the returned value. 
        /// </summary>
        public AccessorAttribute[] Attributes { get; }

        #endregion

        #region IAccessorMember

        /// <summary>
        /// Returns true if this field was declared as "readonly".
        /// True if this is a read-only property; that is, a property with no set accessor.
        /// </summary>
        public readonly bool IsReadOnly { get; }

        /// <summary>
        /// Gets the type of this field.
        /// </summary>
        public readonly IAccessorType Type { get; }

        /// <summary>
        /// Gets the type full name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string TypeFullName { get; }

        /// <summary>
        /// Get return value.
        /// </summary>
        public readonly Func<IGeneratorAccessor, object> GetValue { get; }

        /// <summary>
        /// Set value.
        /// </summary>
        public readonly SetValue SetValue { get; }

        #endregion

        #region IAccessorField

        /// <summary>
        /// Returns true if this field was declared as "const" (i.e. is a constant declaration).
        /// Also returns true for an enum member.
        /// </summary>
        public readonly bool IsConst { get; }

        /// <summary>
        /// Returns true if this field was declared as "volatile".
        /// </summary>
        public readonly bool IsVolatile { get; }

        /// <summary>
        /// Returns false if the field wasn't declared as "const", or constant value was
        /// omitted or erroneous. True otherwise.
        /// </summary>
        public readonly bool HasConstantValue { get; }

        /// <summary>
        /// Gets the constant value of this field
        /// </summary>
        public readonly object ConstantValue { get; }

        /// <summary>
        /// Returns true if this field represents a tuple element which was given an explicit
        /// name.
        /// </summary>
        public readonly bool IsExplicitlyNamedTupleElement { get; }

        #endregion
    }

    /// <summary>
    /// Represents a property or indexer.
    /// </summary>
    /// <remarks>
    /// This interface is reserved for implementation by its associated APIs. We reserve
    /// the right to change it in the future.
    /// </remarks>
    public readonly struct AccessorProperty : IAccessorProperty
    {
        public AccessorProperty(Accessibility declaredAccessibility, bool isExtern, bool isSealed, bool isAbstract, bool isOverride, bool isVirtual, bool isStatic, bool isDefinition, string name, string fullName, Kind kind, bool isDeclaringSyntaxReferences, AccessorAttribute[] attributes, bool isReadOnly, IAccessorType type, string typeFullName, RefKind refKind, bool isWriteOnly, Func<IGeneratorAccessor, object> getValue, SetValue setValue)
        {
            DeclaredAccessibility = declaredAccessibility;
            IsExtern = isExtern;
            IsSealed = isSealed;
            IsAbstract = isAbstract;
            IsOverride = isOverride;
            IsVirtual = isVirtual;
            IsStatic = isStatic;
            IsDefinition = isDefinition;
            Name = name;
            FullName = fullName;
            Kind = kind;
            IsDeclaringSyntaxReferences = isDeclaringSyntaxReferences;
            Attributes = attributes;
            IsReadOnly = isReadOnly;
            Type = type;
            TypeFullName = typeFullName;
            RefKind = refKind;
            IsWriteOnly = isWriteOnly;
            GetValue = getValue;
            SetValue = setValue;
        }

        public override string ToString() => $"{Kind} {FullName}";

        #region Meta

        /// <summary>
        /// Gets a Microsoft.CodeAnalysis.Accessibility indicating the declared accessibility
        /// for the symbol. Returns NotApplicable if no accessibility is declared.
        /// </summary>
        public readonly Accessibility DeclaredAccessibility { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is defined externally.
        /// </summary>
        public readonly bool IsExtern { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is sealed.
        /// </summary>
        public readonly bool IsSealed { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is abstract.
        /// </summary>
        public readonly bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is an override of a base class symbol.
        /// </summary>
        public readonly bool IsOverride { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is virtual.
        /// </summary>
        public readonly bool IsVirtual { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is static.
        /// </summary>
        public readonly bool IsStatic { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is the original definition. Returns
        /// false if the symbol is derived from another symbol, by type substitution for
        /// instance.
        /// </summary>
        public readonly bool IsDefinition { get; }

        /// <summary>
        /// Gets the symbol name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string Name { get; }

        /// <summary>
        /// Gets the symbol full name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string FullName { get; }

        /// <summary>
        /// Gets the Microsoft.CodeAnalysis.SymbolKind indicating what kind of symbol it is.
        /// </summary>
        public readonly Kind Kind { get; }

        /// <summary>
        /// Whether the syntax node(s) where this symbol was declared in source.
        /// </summary>
        public readonly bool IsDeclaringSyntaxReferences { get; }

        /// <summary>
        /// Returns the list of custom attributes, if any, associated with the returned value. 
        /// </summary>
        public AccessorAttribute[] Attributes { get; }

        #endregion

        #region IAccessorMember

        /// <summary>
        /// Returns true if this field was declared as "readonly".
        /// True if this is a read-only property; that is, a property with no set accessor.
        /// </summary>
        public readonly bool IsReadOnly { get; }

        /// <summary>
        /// Gets the type of this field.
        /// </summary>
        public readonly IAccessorType Type { get; }

        /// <summary>
        /// Gets the type full name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string TypeFullName { get; }

        /// <summary>
        /// Get return value.
        /// </summary>
        public readonly Func<IGeneratorAccessor, object> GetValue { get; }

        /// <summary>
        /// Set value.
        /// </summary>
        public readonly SetValue SetValue { get; }

        #endregion

        #region IAccessorProperty

        /// <summary>
        /// Returns the RefKind of the property.
        /// </summary>
        public readonly RefKind RefKind { get; }

        /// <summary>
        /// True if this is a write-only property; that is, a property with no get accessor.
        /// </summary>
        public readonly bool IsWriteOnly { get; }

        #endregion
    }

    /// <summary>
    /// Represents an event.
    /// </summary>
    /// <remarks>
    /// This interface is reserved for implementation by its associated APIs. We reserve
    /// the right to change it in the future.
    /// </remarks>
    public readonly struct AccessorEvent : IAccessorEvent
    {
        public AccessorEvent(Accessibility declaredAccessibility, bool isExtern, bool isSealed, bool isAbstract, bool isOverride, bool isVirtual, bool isStatic, bool isDefinition, string name, string fullName, Kind kind, bool isDeclaringSyntaxReferences, AccessorAttribute[] attributes, bool isReadOnly, IAccessorType type, string typeFullName, bool isWindowsRuntimeEvent, IAccessorMethod addMethod, IAccessorMethod removeMethod, Func<IGeneratorAccessor, object> getValue, SetValue setValue)
        {
            DeclaredAccessibility = declaredAccessibility;
            IsExtern = isExtern;
            IsSealed = isSealed;
            IsAbstract = isAbstract;
            IsOverride = isOverride;
            IsVirtual = isVirtual;
            IsStatic = isStatic;
            IsDefinition = isDefinition;
            Name = name;
            FullName = fullName;
            Kind = kind;
            IsDeclaringSyntaxReferences = isDeclaringSyntaxReferences;
            Attributes = attributes;
            IsReadOnly = isReadOnly;
            Type = type;
            TypeFullName = typeFullName;
            IsWindowsRuntimeEvent = isWindowsRuntimeEvent;
            AddMethod = addMethod;
            RemoveMethod = removeMethod;
            GetValue = getValue;
            SetValue = setValue;
        }

        public override string ToString() => $"{Kind} {FullName}";

        #region Meta

        /// <summary>
        /// Gets a Microsoft.CodeAnalysis.Accessibility indicating the declared accessibility
        /// for the symbol. Returns NotApplicable if no accessibility is declared.
        /// </summary>
        public readonly Accessibility DeclaredAccessibility { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is defined externally.
        /// </summary>
        public readonly bool IsExtern { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is sealed.
        /// </summary>
        public readonly bool IsSealed { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is abstract.
        /// </summary>
        public readonly bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is an override of a base class symbol.
        /// </summary>
        public readonly bool IsOverride { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is virtual.
        /// </summary>
        public readonly bool IsVirtual { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is static.
        /// </summary>
        public readonly bool IsStatic { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol is the original definition. Returns
        /// false if the symbol is derived from another symbol, by type substitution for
        /// instance.
        /// </summary>
        public readonly bool IsDefinition { get; }

        /// <summary>
        /// Gets the symbol name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string Name { get; }

        /// <summary>
        /// Gets the symbol full name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string FullName { get; }

        /// <summary>
        /// Gets the Microsoft.CodeAnalysis.SymbolKind indicating what kind of symbol it is.
        /// </summary>
        public readonly Kind Kind { get; }

        /// <summary>
        /// Whether the syntax node(s) where this symbol was declared in source.
        /// </summary>
        public readonly bool IsDeclaringSyntaxReferences { get; }

        /// <summary>
        /// Returns the list of custom attributes, if any, associated with the returned value. 
        /// </summary>
        public AccessorAttribute[] Attributes { get; }

        #endregion

        #region IAccessorMember

        /// <summary>
        /// Returns true if this field was declared as "readonly".
        /// True if this is a read-only property; that is, a property with no set accessor.
        /// </summary>
        public readonly bool IsReadOnly { get; }

        /// <summary>
        /// Gets the type of this field, property, event.
        /// </summary>
        public readonly IAccessorType Type { get; }

        /// <summary>
        /// Gets the type full name. Returns the empty string if unnamed.
        /// </summary>
        public readonly string TypeFullName { get; }

        /// <summary>
        /// Get return value.
        /// </summary>
        public readonly Func<IGeneratorAccessor, object> GetValue { get; }

        /// <summary>
        /// Set value.
        /// </summary>
        public readonly SetValue SetValue { get; }

        #endregion

        #region IAccessorEvent

        /// <summary>
        /// Returns true if the event is a WinRT type event.
        /// </summary>
        public readonly bool IsWindowsRuntimeEvent { get; }

        /// <summary>
        /// The 'add' accessor of the event. Null only in error scenarios.
        /// </summary>
        public readonly IAccessorMethod AddMethod { get; }

        /// <summary>
        /// The 'remove' accessor of the event. Null only in error scenarios.
        /// </summary>
        public readonly IAccessorMethod RemoveMethod { get; }

        #endregion
    }

    public readonly struct AccessorAttribute
    {
        public AccessorAttribute(string name, TypedConstant[] constructor)
        {
            Name = name;
            Constructor = constructor;
        }

        public readonly string Name { get; }

        public readonly TypedConstant[] Constructor { get; }
    }

    public readonly struct TypedConstant
    {
        public TypedConstant(string name, Type type, bool isNull, TypedConstantKind kind, object value)
        {
            Name = name;
            Type = type;
            IsNull = isNull;
            Kind = kind;
            Value = value;
        }

        public readonly string Name { get; }

        public readonly Type Type { get; }

        public readonly bool IsNull { get; }

        public readonly TypedConstantKind Kind { get; }

        public readonly object Value { get; }
    }
}