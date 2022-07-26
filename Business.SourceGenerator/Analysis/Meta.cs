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

namespace Business.SourceGenerator.Analysis.Meta
{
    using System.Collections.Generic;
    using System.Linq;

    public enum NullableAnnotation : byte
    {
        None = 0,
        NotAnnotated = 1,
        Annotated = 2
    }

    public enum Kind
    {
        Alias = 0,
        ArrayType = 1,
        Assembly = 2,
        DynamicType = 3,
        ErrorType = 4,
        Event = 5,
        Field = 6,
        Label = 7,
        Local = 8,
        Method = 9,
        NetModule = 10,
        NamedType = 11,
        Namespace = 12,
        Parameter = 13,
        PointerType = 14,
        Property = 15,
        RangeVariable = 16,
        TypeParameter = 17,
        Preprocessing = 18,
        Discard = 19,
        FunctionPointerType = 20
    }

    public enum TypeKind : byte
    {
        Unknown = 0,
        Array = 1,
        Class = 2,
        Delegate = 3,
        Dynamic = 4,
        Enum = 5,
        Error = 6,
        Interface = 7,
        Module = 8,
        Pointer = 9,
        Struct = 10,
        Structure = 10,
        TypeParameter = 11,
        Submission = 12,
        FunctionPointer = 13
    }

    public enum RefKind : byte
    {
        None = 0,
        Ref = 1,
        Out = 2,
        In = 3,
        RefReadOnly = 3
    }

    public enum SpecialType : sbyte
    {
        None = 0,
        System_Object = 1,
        System_Enum = 2,
        System_MulticastDelegate = 3,
        System_Delegate = 4,
        System_ValueType = 5,
        System_Void = 6,
        System_Boolean = 7,
        System_Char = 8,
        System_SByte = 9,
        System_Byte = 10,
        System_Int16 = 11,
        System_UInt16 = 12,
        System_Int32 = 13,
        System_UInt32 = 14,
        System_Int64 = 15,
        System_UInt64 = 16,
        System_Decimal = 17,
        System_Single = 18,
        System_Double = 19,
        System_String = 20,
        System_IntPtr = 21,
        System_UIntPtr = 22,
        System_Array = 23,
        System_Collections_IEnumerable = 24,
        System_Collections_Generic_IEnumerable_T = 25,
        System_Collections_Generic_IList_T = 26,
        System_Collections_Generic_ICollection_T = 27,
        System_Collections_IEnumerator = 28,
        System_Collections_Generic_IEnumerator_T = 29,
        System_Collections_Generic_IReadOnlyList_T = 30,
        System_Collections_Generic_IReadOnlyCollection_T = 31,
        System_Nullable_T = 32,
        System_DateTime = 33,
        System_Runtime_CompilerServices_IsVolatile = 34,
        System_IDisposable = 35,
        System_TypedReference = 36,
        System_ArgIterator = 37,
        System_RuntimeArgumentHandle = 38,
        System_RuntimeFieldHandle = 39,
        System_RuntimeMethodHandle = 40,
        System_RuntimeTypeHandle = 41,
        System_IAsyncResult = 42,
        System_AsyncCallback = 43,
        System_Runtime_CompilerServices_RuntimeFeature = 44,
        System_Runtime_CompilerServices_PreserveBaseOverridesAttribute = 45,
        Count = 45
    }

    public enum MemberAccessibility
    {
        NotApplicable = 0,
        Private = 1,
        ProtectedAndInternal = 2,
        ProtectedAndFriend = 2,
        Protected = 3,
        Internal = 4,
        Friend = 4,
        ProtectedOrInternal = 5,
        ProtectedOrFriend = 5,
        Public = 6
    }

    public interface IAccessorMeta
    {
        public string Name { get; }

        public string FullName { get; }

        public Kind Kind { get; }

        public MemberAccessibility Accessibility { get; }

        public bool CanBeReferencedByName { get; }

        public bool IsImplicitlyDeclared { get; }

        public bool IsExtern { get; }

        public bool IsSealed { get; }

        public bool IsAbstract { get; }

        public bool IsOverride { get; }

        public bool IsVirtual { get; }

        public bool IsStatic { get; }

        public bool IsDefinition { get; }
    }

    public interface IAccessorType : IAccessorMeta
    {
        public bool IsGeneric { get; }

        public IEnumerable<IAccessorType> TypeParameters { get; }

        public IEnumerable<IAccessorMeta> Members { get; }

        public bool IsNamespace { get; }

        public bool IsType { get; }

        public bool IsReferenceType { get; }

        public bool IsReadOnly { get; }

        public bool IsUnmanagedType { get; }

        public bool IsRefLikeType { get; }

        public SpecialType SpecialType { get; }

        //IAccessorType OriginalDefinition { get; }

        public bool IsNativeIntegerType { get; }

        public bool IsTupleType { get; }

        public bool IsAnonymousType { get; }

        public bool IsValueType { get; }

        public IEnumerable<IAccessorType> AllInterfaces { get; }

        public IAccessorType BaseType { get; }

        public TypeKind TypeKind { get; }

        public bool IsRecord { get; }
    }

    public interface IAccessorMethod : IAccessorMeta
    {
        public bool IsPartialDefinition { get; }

        public bool IsConditional { get; }

        public int Arity { get; }

        public bool IsGeneric { get; }

        public bool IsExtension { get; }

        public bool IsVararg { get; }

        public bool IsCheckedBuiltin { get; }

        public bool IsAsync { get; }

        public bool ReturnsVoid { get; }

        public bool ReturnsByRef { get; }

        public bool ReturnsByRefReadonly { get; }

        public RefKind RefKind { get; }

        public IAccessorType ReturnType { get; }

        public IEnumerable<IAccessorType> TypeParameters { get; }

        public bool IsReadOnly { get; }

        public bool IsInitOnly { get; }

        public IEnumerable<IAccessorParameter> Parameters { get; }

        public bool HidesBaseMethodsByName { get; }
    }

    public interface IAccessorParameter : IAccessorMeta//, System.IEquatable<IAccessorMeta>
    {
        public RefKind RefKind { get; }

        public bool IsParams { get; }

        public bool IsOptional { get; }

        public bool IsThis { get; }

        public bool IsDiscard { get; }

        public IAccessorType Type { get; }

        public NullableAnnotation NullableAnnotation { get; }

        public int Ordinal { get; }

        public bool HasExplicitDefaultValue { get; }

        public object ExplicitDefaultValue { get; }
    }

    public interface IAccessorFieldOrProperty : IAccessorMeta
    {
        public bool IsReadOnly { get; }

        public bool CanSet { get; }

        public IAccessorType Type { get; }
    }

    public interface IAccessorField : IAccessorFieldOrProperty
    {
        public bool IsConst { get; }

        //public bool IsReadOnly { get; }

        public bool IsVolatile { get; }

        public bool IsFixedSizeBuffer { get; }

        public int FixedSize { get; }

        //public IAccessorType Type { get; }

        public NullableAnnotation NullableAnnotation { get; }

        public bool HasConstantValue { get; }

        public object ConstantValue { get; }

        public bool IsExplicitlyNamedTupleElement { get; }
    }

    public interface IAccessorProperty : IAccessorFieldOrProperty
    {
        public bool IsIndexer { get; }

        //public bool CanSet { get; }

        public IEnumerable<IAccessorParameter> Parameters { get; }

        public NullableAnnotation NullableAnnotation { get; }

        public RefKind RefKind { get; }

        public bool ReturnsByRefReadonly { get; }

        public bool ReturnsByRef { get; }

        public bool IsWithEvents { get; }

        public bool IsWriteOnly { get; }

        //public bool IsReadOnly { get; }

        //public IAccessorType Type { get; }
    }

    public readonly struct AccessorMethod : IAccessorMethod
    {
        public AccessorMethod(string name, string fullName, Kind kind, MemberAccessibility accessibility, bool canBeReferencedByName, bool isImplicitlyDeclared, bool isExtern, bool isSealed, bool isAbstract, bool isOverride, bool isVirtual, bool isStatic, bool isDefinition, bool isPartialDefinition, bool isConditional, int arity, bool isGeneric, bool isExtension, bool isVararg, bool isCheckedBuiltin, bool isAsync, bool returnsVoid, bool returnsByRef, bool returnsByRefReadonly, RefKind refKind, IAccessorType returnType, IEnumerable<IAccessorType> typeParameters, bool isReadOnly, bool isInitOnly, IEnumerable<IAccessorParameter> parameters, bool hidesBaseMethodsByName)
        {
            Name = name;
            FullName = fullName;
            Kind = kind;
            Accessibility = accessibility;
            CanBeReferencedByName = canBeReferencedByName;
            IsImplicitlyDeclared = isImplicitlyDeclared;
            IsExtern = isExtern;
            IsSealed = isSealed;
            IsAbstract = isAbstract;
            IsOverride = isOverride;
            IsVirtual = isVirtual;
            IsStatic = isStatic;
            IsDefinition = isDefinition;
            IsPartialDefinition = isPartialDefinition;
            IsConditional = isConditional;
            Arity = arity;
            IsGeneric = isGeneric;
            IsExtension = isExtension;
            IsVararg = isVararg;
            IsCheckedBuiltin = isCheckedBuiltin;
            IsAsync = isAsync;
            ReturnsVoid = returnsVoid;
            ReturnsByRef = returnsByRef;
            ReturnsByRefReadonly = returnsByRefReadonly;
            RefKind = refKind;
            ReturnType = returnType;
            TypeParameters = typeParameters;
            IsReadOnly = isReadOnly;
            IsInitOnly = isInitOnly;
            Parameters = parameters;
            HidesBaseMethodsByName = hidesBaseMethodsByName;
        }

        public static IAccessorMethod Create(Microsoft.CodeAnalysis.IMethodSymbol methodSymbol) => new AccessorMethod(
            //======Meta======//

            methodSymbol.Name,
            methodSymbol.GetFullName(),
            (Kind)methodSymbol.Kind,
            (MemberAccessibility)methodSymbol.DeclaredAccessibility,
            methodSymbol.CanBeReferencedByName,
            methodSymbol.IsImplicitlyDeclared,
            methodSymbol.IsExtern,
            methodSymbol.IsSealed,
            methodSymbol.IsAbstract,
            methodSymbol.IsOverride,
            methodSymbol.IsVirtual,
            methodSymbol.IsStatic,
            methodSymbol.IsDefinition,

            //======IMethodSymbol======//

            methodSymbol.IsPartialDefinition,
            methodSymbol.IsConditional,
            methodSymbol.Arity,
            methodSymbol.IsGenericMethod,
            methodSymbol.IsExtensionMethod,
            methodSymbol.IsVararg,
            methodSymbol.IsCheckedBuiltin,
            methodSymbol.IsAsync,
            methodSymbol.ReturnsVoid,
            methodSymbol.ReturnsByRef,
            methodSymbol.ReturnsByRefReadonly,
            (RefKind)methodSymbol.RefKind,
            AccessorType.Create(methodSymbol.ReturnType),
            methodSymbol.TypeParameters.Select(c => AccessorType.Create(c)),
            methodSymbol.IsReadOnly,
            methodSymbol.IsInitOnly,
            methodSymbol.Parameters.Select(c => AccessorParameter.Create(c)),
            methodSymbol.HidesBaseMethodsByName
            );

        public override string ToString() => $"{Kind} {FullName}";

        #region Meta

        public string Name { get; }

        public string FullName { get; }

        public Kind Kind { get; }

        public MemberAccessibility Accessibility { get; }

        public bool CanBeReferencedByName { get; }

        public bool IsImplicitlyDeclared { get; }

        public bool IsExtern { get; }

        public bool IsSealed { get; }

        public bool IsAbstract { get; }

        public bool IsOverride { get; }

        public bool IsVirtual { get; }

        public bool IsStatic { get; }

        public bool IsDefinition { get; }

        #endregion

        public bool IsPartialDefinition { get; }

        public bool IsConditional { get; }

        public int Arity { get; }

        public bool IsGeneric { get; }

        public bool IsExtension { get; }

        public bool IsVararg { get; }

        public bool IsCheckedBuiltin { get; }

        public bool IsAsync { get; }

        public bool ReturnsVoid { get; }

        public bool ReturnsByRef { get; }

        public bool ReturnsByRefReadonly { get; }

        public RefKind RefKind { get; }

        public IAccessorType ReturnType { get; }

        public IEnumerable<IAccessorType> TypeParameters { get; }

        public bool IsReadOnly { get; }

        public bool IsInitOnly { get; }

        public IEnumerable<IAccessorParameter> Parameters { get; }

        public bool HidesBaseMethodsByName { get; }
    }

    public readonly struct AccessorParameter : IAccessorParameter
    {
        public AccessorParameter(string name, string fullName, Kind kind, MemberAccessibility accessibility, bool canBeReferencedByName, bool isImplicitlyDeclared, bool isExtern, bool isSealed, bool isAbstract, bool isOverride, bool isVirtual, bool isStatic, bool isDefinition, RefKind refKind, bool isParams, bool isOptional, bool isThis, bool isDiscard, IAccessorType type, NullableAnnotation nullableAnnotation, int ordinal, bool hasExplicitDefaultValue, object explicitDefaultValue)
        {
            Name = name;
            FullName = fullName;
            Kind = kind;
            Accessibility = accessibility;
            CanBeReferencedByName = canBeReferencedByName;
            IsImplicitlyDeclared = isImplicitlyDeclared;
            IsExtern = isExtern;
            IsSealed = isSealed;
            IsAbstract = isAbstract;
            IsOverride = isOverride;
            IsVirtual = isVirtual;
            IsStatic = isStatic;
            IsDefinition = isDefinition;
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
        }

        public static IAccessorParameter Create(Microsoft.CodeAnalysis.IParameterSymbol parameterSymbol) => new AccessorParameter(
            //======Meta======//

            parameterSymbol.Name,
            parameterSymbol.GetFullName(),
            (Kind)parameterSymbol.Kind,
            (MemberAccessibility)parameterSymbol.DeclaredAccessibility,
            parameterSymbol.CanBeReferencedByName,
            parameterSymbol.IsImplicitlyDeclared,
            parameterSymbol.IsExtern,
            parameterSymbol.IsSealed,
            parameterSymbol.IsAbstract,
            parameterSymbol.IsOverride,
            parameterSymbol.IsVirtual,
            parameterSymbol.IsStatic,
            parameterSymbol.IsDefinition,

            //======IParameterSymbol======//

            (RefKind)parameterSymbol.RefKind,
            parameterSymbol.IsParams,
            parameterSymbol.IsOptional,
            parameterSymbol.IsThis,
            parameterSymbol.IsDiscard,
            AccessorType.Create(parameterSymbol.Type),
            (NullableAnnotation)parameterSymbol.NullableAnnotation,
            parameterSymbol.Ordinal,
            parameterSymbol.HasExplicitDefaultValue,
            parameterSymbol.HasExplicitDefaultValue ? parameterSymbol.ExplicitDefaultValue : default
            );

        public override string ToString() => $"{Kind} {FullName}";

        #region Meta

        public string Name { get; }

        public string FullName { get; }

        public Kind Kind { get; }

        public MemberAccessibility Accessibility { get; }

        public bool CanBeReferencedByName { get; }

        public bool IsImplicitlyDeclared { get; }

        public bool IsExtern { get; }

        public bool IsSealed { get; }

        public bool IsAbstract { get; }

        public bool IsOverride { get; }

        public bool IsVirtual { get; }

        public bool IsStatic { get; }

        public bool IsDefinition { get; }

        #endregion

        public RefKind RefKind { get; }

        public bool IsParams { get; }

        public bool IsOptional { get; }

        public bool IsThis { get; }

        public bool IsDiscard { get; }

        public IAccessorType Type { get; }

        public NullableAnnotation NullableAnnotation { get; }

        public int Ordinal { get; }

        public bool HasExplicitDefaultValue { get; }

        public object ExplicitDefaultValue { get; }
    }

    public readonly struct AccessorField : IAccessorField
    {
        public AccessorField(string name, string fullName, Kind kind, MemberAccessibility accessibility, bool canBeReferencedByName, bool isImplicitlyDeclared, bool isExtern, bool isSealed, bool isAbstract, bool isOverride, bool isVirtual, bool isStatic, bool isDefinition, bool canSet, bool isConst, bool isReadOnly, bool isVolatile, bool isFixedSizeBuffer, int fixedSize, IAccessorType type, NullableAnnotation nullableAnnotation, bool hasConstantValue, object constantValue, bool isExplicitlyNamedTupleElement)
        {
            Name = name;
            FullName = fullName;
            Kind = kind;
            Accessibility = accessibility;
            CanBeReferencedByName = canBeReferencedByName;
            IsImplicitlyDeclared = isImplicitlyDeclared;
            IsExtern = isExtern;
            IsSealed = isSealed;
            IsAbstract = isAbstract;
            IsOverride = isOverride;
            IsVirtual = isVirtual;
            IsStatic = isStatic;
            IsDefinition = isDefinition;
            CanSet = canSet;
            IsConst = isConst;
            IsReadOnly = isReadOnly;
            IsVolatile = isVolatile;
            IsFixedSizeBuffer = isFixedSizeBuffer;
            FixedSize = fixedSize;
            Type = type;
            NullableAnnotation = nullableAnnotation;
            HasConstantValue = hasConstantValue;
            ConstantValue = constantValue;
            IsExplicitlyNamedTupleElement = isExplicitlyNamedTupleElement;
        }

        public static IAccessorField Create(Microsoft.CodeAnalysis.IFieldSymbol fieldSymbol) => new AccessorField(
            //======Meta======//

            fieldSymbol.Name,
            fieldSymbol.GetFullName(),
            (Kind)fieldSymbol.Kind,
            (MemberAccessibility)fieldSymbol.DeclaredAccessibility,
            fieldSymbol.CanBeReferencedByName,
            fieldSymbol.IsImplicitlyDeclared,
            fieldSymbol.IsExtern,
            fieldSymbol.IsSealed,
            fieldSymbol.IsAbstract,
            fieldSymbol.IsOverride,
            fieldSymbol.IsVirtual,
            fieldSymbol.IsStatic,
            fieldSymbol.IsDefinition,

            //======IAccessorField======//

            !fieldSymbol.IsReadOnly,
            fieldSymbol.IsConst,
            fieldSymbol.IsReadOnly,
            fieldSymbol.IsVolatile,
            fieldSymbol.IsFixedSizeBuffer,
            fieldSymbol.FixedSize,
            AccessorType.Create(fieldSymbol.Type),
            (NullableAnnotation)fieldSymbol.NullableAnnotation,
            fieldSymbol.HasConstantValue,
            fieldSymbol.ConstantValue,
            fieldSymbol.IsExplicitlyNamedTupleElement
            );

        public override string ToString() => $"{Kind} {FullName}";

        #region Meta

        public string Name { get; }

        public string FullName { get; }

        public Kind Kind { get; }

        public MemberAccessibility Accessibility { get; }

        public bool CanBeReferencedByName { get; }

        public bool IsImplicitlyDeclared { get; }

        public bool IsExtern { get; }

        public bool IsSealed { get; }

        public bool IsAbstract { get; }

        public bool IsOverride { get; }

        public bool IsVirtual { get; }

        public bool IsStatic { get; }

        public bool IsDefinition { get; }

        #endregion

        public bool CanSet { get; }

        public bool IsConst { get; }

        public bool IsReadOnly { get; }

        public bool IsVolatile { get; }

        public bool IsFixedSizeBuffer { get; }

        public int FixedSize { get; }

        public IAccessorType Type { get; }

        public NullableAnnotation NullableAnnotation { get; }

        public bool HasConstantValue { get; }

        public object ConstantValue { get; }

        public bool IsExplicitlyNamedTupleElement { get; }
    }

    public readonly struct AccessorProperty : IAccessorProperty
    {
        public AccessorProperty(string name, string fullName, Kind kind, MemberAccessibility accessibility, bool canBeReferencedByName, bool isImplicitlyDeclared, bool isExtern, bool isSealed, bool isAbstract, bool isOverride, bool isVirtual, bool isStatic, bool isDefinition, bool canSet, bool isIndexer, IEnumerable<IAccessorParameter> parameters, NullableAnnotation nullableAnnotation, RefKind refKind, bool returnsByRefReadonly, bool returnsByRef, bool isWithEvents, bool isWriteOnly, bool isReadOnly, IAccessorType type)
        {
            Name = name;
            FullName = fullName;
            Kind = kind;
            Accessibility = accessibility;
            CanBeReferencedByName = canBeReferencedByName;
            IsImplicitlyDeclared = isImplicitlyDeclared;
            IsExtern = isExtern;
            IsSealed = isSealed;
            IsAbstract = isAbstract;
            IsOverride = isOverride;
            IsVirtual = isVirtual;
            IsStatic = isStatic;
            IsDefinition = isDefinition;
            CanSet = canSet;
            IsIndexer = isIndexer;
            Parameters = parameters;
            NullableAnnotation = nullableAnnotation;
            RefKind = refKind;
            ReturnsByRefReadonly = returnsByRefReadonly;
            ReturnsByRef = returnsByRef;
            IsWithEvents = isWithEvents;
            IsWriteOnly = isWriteOnly;
            IsReadOnly = isReadOnly;
            Type = type;
        }

        public static IAccessorProperty Create(Microsoft.CodeAnalysis.IPropertySymbol propertySymbol) => new AccessorProperty(
            //======Meta======//

            propertySymbol.Name,
            propertySymbol.GetFullName(),
            (Kind)propertySymbol.Kind,
            (MemberAccessibility)propertySymbol.DeclaredAccessibility,
            propertySymbol.CanBeReferencedByName,
            propertySymbol.IsImplicitlyDeclared,
            propertySymbol.IsExtern,
            propertySymbol.IsSealed,
            propertySymbol.IsAbstract,
            propertySymbol.IsOverride,
            propertySymbol.IsVirtual,
            propertySymbol.IsStatic,
            propertySymbol.IsDefinition,

            //======IPropertySymbol======//

            null != propertySymbol.SetMethod,
            propertySymbol.IsIndexer,
            propertySymbol.Parameters.Select(c => AccessorParameter.Create(c)),
            (NullableAnnotation)propertySymbol.NullableAnnotation,
            (RefKind)propertySymbol.RefKind,
            propertySymbol.ReturnsByRefReadonly,
            propertySymbol.ReturnsByRef,
            propertySymbol.IsWithEvents,
            propertySymbol.IsWriteOnly,
            propertySymbol.IsReadOnly,
            AccessorType.Create(propertySymbol.Type)
            );

        public override string ToString() => $"{Kind} {FullName}";

        #region Meta

        public string Name { get; }

        public string FullName { get; }

        public Kind Kind { get; }

        public MemberAccessibility Accessibility { get; }

        public bool CanBeReferencedByName { get; }

        public bool IsImplicitlyDeclared { get; }

        public bool IsExtern { get; }

        public bool IsSealed { get; }

        public bool IsAbstract { get; }

        public bool IsOverride { get; }

        public bool IsVirtual { get; }

        public bool IsStatic { get; }

        public bool IsDefinition { get; }

        #endregion

        public bool CanSet { get; }

        public bool IsIndexer { get; }

        public IEnumerable<IAccessorParameter> Parameters { get; }

        public NullableAnnotation NullableAnnotation { get; }

        public RefKind RefKind { get; }

        public bool ReturnsByRefReadonly { get; }

        public bool ReturnsByRef { get; }

        public bool IsWithEvents { get; }

        public bool IsWriteOnly { get; }

        public bool IsReadOnly { get; }

        public IAccessorType Type { get; }
    }

    public readonly struct AccessorType : IAccessorType
    {
        public AccessorType(string name, string fullName, Kind kind, MemberAccessibility accessibility, bool canBeReferencedByName, bool isImplicitlyDeclared, bool isExtern, bool isSealed, bool isAbstract, bool isOverride, bool isVirtual, bool isStatic, bool isDefinition, bool isGeneric, IEnumerable<IAccessorType> typeParameters, IEnumerable<IAccessorMeta> members, bool isNamespace, bool isType, bool isReferenceType, bool isReadOnly, bool isUnmanagedType, bool isRefLikeType, SpecialType specialType, bool isNativeIntegerType, bool isTupleType, bool isAnonymousType, bool isValueType, IEnumerable<IAccessorType> allInterfaces, IAccessorType baseType, TypeKind typeKind, bool isRecord)
        {
            Name = name;
            FullName = fullName;
            Kind = kind;
            Accessibility = accessibility;
            CanBeReferencedByName = canBeReferencedByName;
            IsImplicitlyDeclared = isImplicitlyDeclared;
            IsExtern = isExtern;
            IsSealed = isSealed;
            IsAbstract = isAbstract;
            IsOverride = isOverride;
            IsVirtual = isVirtual;
            IsStatic = isStatic;
            IsDefinition = isDefinition;
            IsGeneric = isGeneric;
            TypeParameters = typeParameters;
            Members = members;
            IsNamespace = isNamespace;
            IsType = isType;
            IsReferenceType = isReferenceType;
            IsReadOnly = isReadOnly;
            IsUnmanagedType = isUnmanagedType;
            IsRefLikeType = isRefLikeType;
            SpecialType = specialType;
            IsNativeIntegerType = isNativeIntegerType;
            IsTupleType = isTupleType;
            IsAnonymousType = isAnonymousType;
            IsValueType = isValueType;
            AllInterfaces = allInterfaces;
            BaseType = baseType;
            TypeKind = typeKind;
            IsRecord = isRecord;
        }

        public static IAccessorType Create(Microsoft.CodeAnalysis.ITypeSymbol typeSymbol, bool isGeneric = false, IEnumerable<IAccessorType> typeParameters = null) => new AccessorType(
            typeSymbol.Name,
            typeSymbol.GetFullName(),
            (Kind)typeSymbol.Kind,
            (MemberAccessibility)typeSymbol.DeclaredAccessibility,
            typeSymbol.CanBeReferencedByName,
            typeSymbol.IsImplicitlyDeclared,
            typeSymbol.IsExtern,
            typeSymbol.IsSealed,
            typeSymbol.IsAbstract,
            typeSymbol.IsOverride,
            typeSymbol.IsVirtual,
            typeSymbol.IsStatic,
            typeSymbol.IsDefinition,
            isGeneric,
            typeParameters,

            typeSymbol.GetMembers().Where(c => c is Microsoft.CodeAnalysis.IFieldSymbol || c is Microsoft.CodeAnalysis.IPropertySymbol || c is Microsoft.CodeAnalysis.IMethodSymbol).Select(c =>
            {
                switch (c)
                {
                    case Microsoft.CodeAnalysis.IFieldSymbol symbol: return AccessorField.Create(symbol);
                    case Microsoft.CodeAnalysis.IPropertySymbol symbol: return AccessorProperty.Create(symbol);
                    case Microsoft.CodeAnalysis.IMethodSymbol symbol: return AccessorMethod.Create(symbol);
                    default: return default(IAccessorMeta);
                }
            }),

            //typeSymbol.GetMembers().Where(c => (c is Microsoft.CodeAnalysis.IFieldSymbol).Select(c => AccessorMember.Create(c as Microsoft.CodeAnalysis.ITypeSymbol)), // members,
            typeSymbol.IsNamespace,
            typeSymbol.IsType,
            typeSymbol.IsReferenceType,
            typeSymbol.IsReadOnly,
            typeSymbol.IsUnmanagedType,
            typeSymbol.IsRefLikeType,
            (SpecialType)typeSymbol.SpecialType,
            typeSymbol.IsNativeIntegerType,
            typeSymbol.IsTupleType,
            typeSymbol.IsAnonymousType,
            typeSymbol.IsValueType,
            typeSymbol.AllInterfaces.Any() ? typeSymbol.AllInterfaces.Select(c => Create(c)) : default,
            null == typeSymbol.BaseType ? default : Create(typeSymbol.BaseType),
            (TypeKind)typeSymbol.TypeKind,
            typeSymbol.IsRecord
            );

        public static IAccessorType Create(Microsoft.CodeAnalysis.INamedTypeSymbol typeSymbol) => Create(typeSymbol, typeSymbol.IsGenericType, typeSymbol.TypeParameters.Any() ? typeSymbol.TypeParameters.Select(c => Create(c)) : default);

        public override string ToString() => $"{Kind} {FullName}";

        #region Meta

        public string Name { get; }

        public string FullName { get; }

        public Kind Kind { get; }

        public MemberAccessibility Accessibility { get; }

        public bool CanBeReferencedByName { get; }

        public bool IsImplicitlyDeclared { get; }

        public bool IsExtern { get; }

        public bool IsSealed { get; }

        public bool IsAbstract { get; }

        public bool IsOverride { get; }

        public bool IsVirtual { get; }

        public bool IsStatic { get; }

        public bool IsDefinition { get; }

        #endregion

        public bool IsGeneric { get; }

        public IEnumerable<IAccessorType> TypeParameters { get; }

        public IEnumerable<IAccessorMeta> Members { get; }

        public bool IsNamespace { get; }

        public bool IsType { get; }

        public bool IsReferenceType { get; }

        public bool IsReadOnly { get; }

        public bool IsUnmanagedType { get; }

        public bool IsRefLikeType { get; }

        public SpecialType SpecialType { get; }

        //IAccessorType OriginalDefinition { get; }

        public bool IsNativeIntegerType { get; }

        public bool IsTupleType { get; }

        public bool IsAnonymousType { get; }

        public bool IsValueType { get; }

        public IEnumerable<IAccessorType> AllInterfaces { get; }

        public IAccessorType BaseType { get; }

        public TypeKind TypeKind { get; }

        public bool IsRecord { get; }
    }
}
