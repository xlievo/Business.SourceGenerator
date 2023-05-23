namespace Business.SourceGenerator.Test
{
    [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = true)]
    public sealed class GeneratorType2Attribute : System.Attribute
    {
        public GeneratorType2Attribute(System.Type type, string a = default, int b = default)
        {

        }
    }

    /// <summary>
    /// IResult
    /// </summary>
    public interface IResult
    {
        /// <summary>
        /// The results of the state is greater than or equal 
        /// to 1: success, equal to 0: system level exceptions, less than 0: business class error.
        /// </summary>
        int State { get; }

        /// <summary>
        /// Success can be null
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        dynamic Data { get; }

        /// <summary>
        /// Whether there is value
        /// </summary>
        bool HasData { get; }

        /// <summary>
        /// Data type
        /// </summary>
        System.Type DataType { get; }

        /// <summary>
        /// Gets the token of this result, used for callback
        /// </summary>
        string Callback { get; set; }

        /// <summary>
        /// ProtoBuf,MessagePack or Other
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <returns></returns>
        byte[] ToBytes(bool dataBytes = true);

        /// <summary>
        /// Json
        /// </summary>
        /// <returns></returns>
        string ToString();

        /// <summary>
        /// System.Type object that represents a generic type definition from which the current generic type can be constructed.
        /// </summary>
        System.Type GenericDefinition { get; }

        /// <summary>
        /// Return data or not
        /// </summary>
        bool HasDataResult { get; }
    }

    /// <summary>
    /// IResult
    /// </summary>
    /// <typeparam name="DataType"></typeparam>
    [Business.SourceGenerator.Meta.GeneratorType]
    public interface IResult2<DataType> : IResult
    {
        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        new DataType Data { get; }
    }

    /// <summary>
    /// result
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    public struct ResultObject<Type> : IResult2<Type>
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
        public ResultObject(System.Type dataType, Type data, int state = 1, string message = null, string callback = null, System.Type genericDefinition = null, bool checkData = true, bool hasData = false, bool hasDataResult = false)
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
        public ResultObject(Type data, int state, string message, bool hasData)
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
        dynamic IResult.Data { get => Data; }

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
        public byte[] ToBytes(bool dataBytes = true) => throw new System.NotImplementedException(); //Utils.Help.ProtoBufSerialize(this);
    }

    /// <summary>
    /// result
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    public partial struct ResultObject2<Type> : IResult2<Type>
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
        public ResultObject2(System.Type dataType, Type data, int state = 1, string message = null, string callback = null, System.Type genericDefinition = null, bool checkData = true, bool hasData = false, bool hasDataResult = false)
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
        public ResultObject2(Type data, int state, string message, bool hasData)
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
        dynamic IResult.Data { get => Data; }

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
        public byte[] ToBytes(bool dataBytes = true) => throw new System.NotImplementedException(); //Utils.Help.ProtoBufSerialize(this);
    }

    /// <summary>
    /// result
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    public struct ResultObject3<Type> : IResult2<Type>
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
        public ResultObject3(System.Type dataType, Type data, int state = 1, string message = null, string callback = null, System.Type genericDefinition = null, bool checkData = true, bool hasData = false, bool hasDataResult = false)
        {
            this.DataType = dataType;
            this.Data = data;
            this.State = state;
            this.Message = message;
            this.HasData = checkData ? !Equals(null, data) : hasData;
            //typeof(MyObject).MakeGenericType<int>().CreateInstance();
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
        public ResultObject3(Type data, int state, string message, bool hasData)
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
        dynamic IResult.Data { get => Data; }

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
        public byte[] ToBytes(bool dataBytes = true) => throw new System.NotImplementedException(); //Utils.Help.ProtoBufSerialize(this);
    }
}
