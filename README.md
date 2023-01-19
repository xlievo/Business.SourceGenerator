# Business.SourceGenerator
This is a Roslyn-Analyzers SourceGenerator based library that mainly solves System.Type.MakeGenericType and AOP.

Because AOT mode cannot dynamically generate code and types at run time, it requires that the types that need to be used must be completely determined at compile time.

**Install**
[![NuGet Version](https://img.shields.io/nuget/v/Business.SourceGenerator.svg?style=flat)](https://www.nuget.org/packages/Business.SourceGenerator)
[![NuGet](https://img.shields.io/nuget/dt/Business.SourceGenerator.svg)](https://www.nuget.org/packages/Business.SourceGenerator)
[![](https://img.shields.io/badge/License-MIT-blue?style=flat-square)](https://github.com/xlievo/Business.SourceGenerator/blob/master/LICENSE)
***

## GetGenericType
**Replace System.Type.MakeGenericType(typeArguments) by generating generic type code in advance, Gets the specified generic type.**

1. Declare [Business.SourceGenerator.Meta.GeneratorType] features on struct or class or interface that need to be generated in advance.
```C#
    typeof(MyClass<>).GetGenericType<int>();
```

## CreateInstance
**Gets an instance of the specified type.**
```C#
    typeof(MyClass<>)
        .GetGenericType<int>()
        .CreateInstance(params object[] args);
```

## AccessorGet & AccessorGetAsync
**Access the members and methods of the specified instance.**
**The specified class or struct needs to declare the 'partial' keyword.**
```C#
    typeof(MyClass<>)
        .GetGenericType<int>()
        .CreateInstance<>(params object[] args)
        .AccessorGetAsync(string name, params object[] args);
        
        AccessorGetAsync<Type>(string name, params object[] args);
```

## AccessorSet
**Set the value of a field or property.**
**The specified class or struct needs to declare the 'partial' keyword.**
```C#
    typeof(MyClass<int>)
        .CreateInstance<IGeneratorAccessor>()
        .AccessorSet<IGeneratorAccessor>("A", "WWW")
        .AccessorSet<IGeneratorAccessor>("B", new Dictionary<string, string>());
```

## About the secondary distribution of nuget packages [direct reference without watching]
1. Please create the following directory in the project root directory
```C#
----Assets
----------build
---------------package.props
----------src
---------------Your source code file, and set the generation operation to No. 
	The code here does not participate in compilation, but requires syntax modeling.
	<Only store the code marked with [Business.SourceGenerator.Meta.GeneratorType]>
```

2. package.props <Your project name, replace dots with underscores "_"->".">
```C#
<Project>
    <PropertyGroup>
        <Your_project_name>$(MSBuildThisFileDirectory)../</Your_project_name>
    </PropertyGroup>
    <ItemGroup>
        <CompilerVisibleProperty Include="Your_project_name" />
    </ItemGroup>
    <ItemGroup>
        <PackageReference Include="Microsoft.Net.Compilers.Toolset" Version="4.3.1" />
    </ItemGroup>
</Project>
```

3. Edit your .csproj file and add the following attributes
```C#
<Target Name="IncludeAllDependencies" BeforeTargets="_GetPackageFiles">
    <ItemGroup>
        <None Include="Assets\build\package.props" Pack="True" PackagePath="build\$(PackageId).props" />
        <None Include="Assets\src\**" Pack="True" PackagePath="src" />
    </ItemGroup>
</Target>
<ItemGroup>
    <Compile Remove="Assets\src\IResult.cs" />
    <None Include="Assets\src\IResult.cs" />
</ItemGroup>
<ItemGroup>
    <PackageReference Include="Business.SourceGenerator" Version="0.0.5-pre.4" />
</ItemGroup>
```
