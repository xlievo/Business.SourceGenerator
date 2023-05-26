# Business.SourceGenerator
This is a based on Roslyn Analyzers the SourceGenerator library, mainly to solve in the &lt;IlcDisableReflection&gt; scenarios for [System.Type.MakeGenericType(type)] generated code.

Due to the inability of AOT &lt;IlcDisableReflection&gt; mode to dynamically reflection types at runtime, it requires a complete determination of the types to be used at compile time.

**Core competence:**  
1: Constructors will be generated for all known types.  
2: Access will be generated for custom types.  
3: Provides an implementation of a single type parameter [System.Type.MakeGenericType(type)] for all known types.  

**Install**
[![NuGet Version](https://img.shields.io/nuget/v/Business.SourceGenerator.svg?style=flat)](https://www.nuget.org/packages/Business.SourceGenerator)
[![NuGet](https://img.shields.io/nuget/dt/Business.SourceGenerator.svg)](https://www.nuget.org/packages/Business.SourceGenerator)
[![](https://img.shields.io/badge/License-MIT-blue?style=flat-square)](https://github.com/xlievo/Business.SourceGenerator/blob/master/LICENSE)
***

## GetGenericType
**Replace System.Type.MakeGenericType(typeArguments) by generating generic type code in advance, Gets the specified generic type.**

1. Declare [Business.SourceGenerator.Meta.GeneratorType] and partial key features on struct or class or interface that need to be generated in advance.
```C#
[Business.SourceGenerator.Meta.GeneratorType]
public partial struct MyStruct<T>
{
	public string A { get; set; }

	public T B { get; set; }

	public MyStruct(string a)
	{
		this.A = a ?? throw new ArgumentNullException(nameof(a));
	}

	public ValueTask<(int c1, string c2)> StructMethod(string? a, ref (int c1, string c2) b, out (int? c1, string? c2) c)
	{
		b.c1 = 888;
		c = (333, "xxx");
		return ValueTask.FromResult(b);
	}
}

/* typeof(MyStruct<>).GetGenericType<int>(); */
```

## CreateInstance
**Gets an instance of the specified type.**
```C#
typeof(MyStruct<>)
	.GetGenericType<int>()
	.CreateInstance(params object[] args);
```

## AccessorMethod & AccessorMethodAsync
**Access the members and methods of the specified instance.**
**The specified class or struct needs to declare the 'partial' keyword.**
```C#
var myStruct = typeof(MyStruct<>)
	.GetGenericType(typeof(int))
	.CreateInstance<IGeneratorAccessor>("666");

var args = new object[] { 
			string.Empty,
			RefArg.Ref((55, "66")),
			RefArg.Out<(int? c1, string? c2)>()
		};
		
var result = await myStruct.AccessorMethodAsync("StructMethod", args);
	
/*
typeof(MyStruct<>)
	.GetGenericType<int>()
	.CreateInstance<>(params object[] args)
	.AccessorMethod(string name, out result, params object[] args);
	
	AccessorMethodAsync<Type>(string name, params object[] args);
*/
```

## AccessorSet & AccessorGet
**Set the value of a field or property.**
**The specified class or struct needs to declare the 'partial' keyword.**
```C#
typeof(MyStruct<>)
	.GetGenericType(typeof(int))
	.CreateInstance<IGeneratorAccessor>()
	.AccessorSet<IGeneratorAccessor>("A", "666")
	.AccessorSet<IGeneratorAccessor>("B", 777)
	.AccessorGet("B", out int result);
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
