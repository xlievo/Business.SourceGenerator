using Business.SourceGenerator.Test;
using System;
using System.Threading.Tasks;

[Business.SourceGenerator.Meta.GeneratorType]
public struct MyStruct<T>
//where T : class
{
    public string A { get; set; }

    public T B { get; set; }

    public MyStruct(string a)
    {
        this.A = a ?? throw new ArgumentNullException(nameof(a));
    }

    public ValueTask<T> StructMethod<T2>(string? a, ref T b, out (int? c1, string? c2) c)
    //where T2 : class, System.Collections.Generic.IList<int>
    {
        //b.c1 = (T)888;
        c = (333, "xxx");
        return ValueTask.FromResult(b);
    }
}

public struct ResultObject4<Type> : IResult2<Type>
{
    /// <summary>
    /// Activator.CreateInstance
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="data"></param>
    /// <param name="state"></param>
    /// <param name="message"></param>
    /// <param name="callback"></param>
    /// <param name="genericDefinition"></param>
    /// <param name="checkData"></param>
    /// <param name="hasData"></param>
    /// <param name="hasDataResult"></param>
    public ResultObject4(System.Type dataType, Type data, int state = 1, string message = null, string callback = null, System.Type genericDefinition = null, bool checkData = true, bool hasData = false, bool hasDataResult = false)
    {
        this.DataType = dataType;
        this.Data = data;
        this.State = state;
        this.Message = message;
        this.HasData = checkData ? !Equals(null, data) : hasData;

        this.Callback = callback;
        this.GenericDefinition = genericDefinition;
        this.HasDataResult = hasDataResult;
    }

    /// <summary>
    /// MessagePack.MessagePackSerializer.Serialize(this)
    /// </summary>
    /// <param name="data"></param>
    /// <param name="state"></param>
    /// <param name="message"></param>
    /// <param name="hasData"></param>
    public ResultObject4(Type data, int state, string message, bool hasData)
    {
        this.Data = data;
        this.State = state;
        this.Message = message;
        this.HasData = hasData;

        this.Callback = null;
        this.DataType = null;
        this.GenericDefinition = null;
        this.HasDataResult = false;
    }

    /// <summary>
    /// The results of the state is greater than or equal to 1: success, equal to 0: system level exceptions, less than 0: business class error.
    /// </summary>
    public int State { get; }

    /// <summary>
    /// Success can be null
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Specific dynamic data objects
    /// </summary>
    dynamic Business.SourceGenerator.Test.IResult.Data { get => Data; }

    /// <summary>
    /// Specific Byte/Json data objects
    /// </summary>
    public Type Data { get; set; }

    /// <summary>
    /// Whether there is value
    /// </summary>
    public bool HasData { get; }

    /// <summary>
    /// Gets the token of this result, used for callback
    /// </summary>
    public string Callback { get; set; }

    /// <summary>
    /// Data type
    /// </summary>
    public System.Type DataType { get; }

    /// <summary>
    /// GenericDefinition
    /// </summary>
    public System.Type GenericDefinition { get; }

    /// <summary>
    /// Return data or not
    /// </summary>
    public bool HasDataResult { get; }

    ///// <summary>
    ///// Json format
    ///// </summary>
    ///// <returns></returns>
    //public override string ToString() => Business.Core.Utils.Help.JsonSerialize(this);

    /// <summary>
    /// ProtoBuf,MessagePack or Other
    /// </summary>
    /// <param name="dataBytes"></param>
    /// <returns></returns>
    public byte[] ToBytes(bool dataBytes = true) => throw new NotImplementedException(); //Utils.Help.ProtoBufSerialize(this);
}


public unsafe struct MyStruct
{
    public string? aaa { get; set; }

    public int? bbb { get; set; }

    public DateTimeOffset? ccc { get; set; }

    public int* ddd { get; set; }

    public MyStruct(string aaa)
    {
        this.aaa = aaa ?? throw new ArgumentNullException(nameof(aaa));
    }

    public (int c1, string c2) StructMember2(ref string? a, out int b, ref (int c1, string c2) c, out (int? c1, string? c2) d)
    {
        d = default;
        var dd = c.c1 as dynamic;

        b = default;

        return c;
    }

    [GeneratorType2(typeof(DateTimeOffset), "999", 333)]
    [GeneratorType2(typeof(DateTimeOffset), b: 666)]
    public ValueTask<(int c1, string c2)> StructMethod7([GeneratorType2(typeof(string), "1", 1)] ref string? a, [GeneratorType2(typeof(int?), "2", 2)] out int? b, ref (int c1, string c2) c, out (int? c1, string? c2) d)
    {
        d = default;
        var dd = c.c1 as dynamic;

        c.c1 = 66778899;

        //this.A = a;
        b = 9;

        return ValueTask.FromResult(c);
    }

    public ValueTask<(int c1, string c2)> StructMethod7(ref string? a, ref (int c1, string c2) b, out (int? c1, string? c2) c)
    {
        c = (333, "xxx");
        return ValueTask.FromResult(b);
    }
}

//[Serde.GenerateSerialize]
//[Serde.GenerateDeserialize]
public struct MyStruct2
{
    public string? aaa { get; set; }

    public int? bbb { get; set; }

    public MyStruct2(string aaa)
    {
        this.aaa = aaa ?? throw new ArgumentNullException(nameof(aaa));
    }
}

public struct MyStruct3
{
    public string aaa { get; set; }

    public MyStruct3(string aaa)
    {
        this.aaa = aaa ?? throw new ArgumentNullException(nameof(aaa));
    }
}

public struct MyStruct4
{
    public string aaa { get; set; }

    public MyStruct4(string aaa)
    {
        this.aaa = aaa ?? throw new ArgumentNullException(nameof(aaa));
    }
}

public struct MyStruct5
{
    public string aaa { get; set; }

    public MyStruct5(string aaa)
    {
        this.aaa = aaa ?? throw new ArgumentNullException(nameof(aaa));
    }
}

public struct MyStruct6
{
    public string aaa { get; set; }

    public MyStruct6(string aaa)
    {
        this.aaa = aaa ?? throw new ArgumentNullException(nameof(aaa));
    }
}

public struct MyStruct7
{
    public string aaa { get; set; }

    public MyStruct7(string aaa)
    {
        this.aaa = aaa ?? throw new ArgumentNullException(nameof(aaa));
    }
}

public struct MyStruct8
{
    public string aaa { get; set; }

    public MyStruct8(string aaa)
    {
        this.aaa = aaa ?? throw new ArgumentNullException(nameof(aaa));
    }
}

public struct MyStruct9
{
    public string aaa { get; set; }

    public MyStruct9(string aaa)
    {
        this.aaa = aaa ?? throw new ArgumentNullException(nameof(aaa));
    }
}

public struct MyStruct10
{
    public string aaa { get; set; }

    public MyStruct10(string aaa)
    {
        this.aaa = aaa ?? throw new ArgumentNullException(nameof(aaa));
    }
}

public struct MyStruct11
{
    public string aaa { get; set; }

    public MyStruct11(string aaa)
    {
        this.aaa = aaa ?? throw new ArgumentNullException(nameof(aaa));
    }
}

public struct MyStruct12
{
    public string aaa { get; set; }

    public MyStruct12(string aaa)
    {
        this.aaa = aaa ?? throw new ArgumentNullException(nameof(aaa));
    }
}

public class ClassGeneric<T> : System.Collections.Generic.Dictionary<T, T>
{
    public T A { get; set; }

    public System.Collections.Generic.Dictionary<T, T> B { get; set; }
}

public struct MethodInvoke<T>
    where T : System.IDisposable
{

}
