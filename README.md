# Business.SourceGenerator
This is a Roslyn-Analyzers SourceGenerator based library that mainly solves System.Type.MakeGenericType and AOP.

Because AOT mode cannot dynamically generate code and types at run time, it requires that the types that need to be used must be completely determined at compile time.

**Install**
[![NuGet Version](https://img.shields.io/nuget/v/Business.SourceGenerator.svg?style=flat)](https://www.nuget.org/packages/Business.SourceGenerator)
[![NuGet](https://img.shields.io/nuget/dt/Business.SourceGenerator.svg)](https://www.nuget.org/packages/Business.SourceGenerator)
[![](https://img.shields.io/badge/License-MIT-blue?style=flat-square)](https://github.com/xlievo/Business.SourceGenerator/blob/master/LICENSE)
***

## [MakeGenericType]
**Replace System.Type.MakeGenericType(typeArguments) by generating generic type code in advance.**

1. Declare [SourceGenerator.Analysis.GeneratorGenericType] features on struct or class or interface that need to be generated in advance.
2. Call Business.SourceGenerator.Utils.GeneratorCode.MakeGenericType(type, typeArguments) to get the specified type.
3. Call Business.SourceGenerator.Utils.GeneratorCode.CreateGenericType(type, typeArguments) to get an instance of the specified type.

## [About distributing nuget packages]
1. Please create the following directory in the project root directory
```C#
----Assets
----------build
---------------package.props
----------src
---------------Your source code file, and set the generation operation to No. 
	The code here does not participate in compilation, but requires syntax modeling.
	<Only store the code marked with [SourceGenerator.Analysis.GeneratorGenericType]>
```

2. package.props <Your project name, replace dots with underscores "_"->".">
```C#
<Project>
	<PropertyGroup>
		<Your project name>$(MSBuildThisFileDirectory)../</Your project name>
	</PropertyGroup>
	<ItemGroup>
		<CompilerVisibleProperty Include="Your project name" />
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
```