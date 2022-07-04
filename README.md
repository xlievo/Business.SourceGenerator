# Business.SourceGenerator
This is a Roslyn-Analyzers SourceGenerator based library that mainly solves System.Type.MakeGenericType and AOP.

Because AOT mode cannot dynamically generate code and types at run time, it requires that the types that need to be used must be completely determined at compile time.

**Install**
[![NuGet Version](https://img.shields.io/nuget/v/Business.SourceGenerator.svg?style=flat)](https://www.nuget.org/packages/Business.SourceGenerator)
[![NuGet](https://img.shields.io/nuget/dt/Business.SourceGenerator.svg)](https://www.nuget.org/packages/Business.SourceGenerator)
***

## [MakeGenericType]
**Replace System.Type.MakeGenericType(typeArguments) by generating generic type code in advance.**

1. Declare 
```C#
[SourceGenerator.Analysis.GeneratorGenericType]
```
features on structures or classes or interfaces that need to be generated in advance.
2. Call ```C#Business.SourceGenerator.Utils.GeneratorCode.MakeGenericType(type, typeArguments)``` to get the specified type.
3. Call ```C#Business.SourceGenerator.Utils.GeneratorCode.CreateGenericType(type, typeArguments)``` to get an instance of the specified type.
