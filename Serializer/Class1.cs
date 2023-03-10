using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Serializer
{
    public interface ISerializerOptions
    {
        string FilePath { get; set; }
    }

    public class MyJSonSerializeOptions : JsonSerializerOptions
    {
        public string FilePath { get ; set; }


    }



    public interface IDeserializerOptions
    {
        // define deserializer options
    }

    public interface ISerializer<T>
    {
        string Serialize(T data, ISerializerOptions options);
        T Deserialize(string data, IDeserializerOptions options);
    }


    public class MyJsonSerializer<T> : ISerializer<T>
    {
        public string Serialize(T data, ISerializerOptions options)
        {
            JsonSerializer.Serialize()
            JsonSerializer.Deserialize<List<T>>((JSonSerializeOptions)options.FilePath);
        }

        public T Deserialize(string data, IDeserializerOptions options)
        {
            // deserialize the data using the JSON format and the options
        }
    }

    //public class XmlSerializer<T> : ISerializer<T>
    //{
    //    public string Serialize(T data, ISerializerOptions options)
    //    {
    //        // serialize the data using the XML format and the options
    //    }
        //public T Deserialize(string data, IDeserializerOptions options)
        //{
        //    // deserialize the data using the XML format and the options
        //}
    //}

   


   

    public class SerializerStrategy<T>
    {
        private readonly ISerializer<T> serializer;

        public SerializerStrategy(ISerializer<T> serializer)
        {
            this.serializer = serializer;
        }

        public string Serialize(T data, ISerializerOptions options)
        {
            return serializer.Serialize(data, options);
        }
    }

    public class DeserializerStrategy<T>
    {
        private readonly IDeserializer<T> deserializer;

        public DeserializerStrategy(IDeserializer<T> deserializer)
        {
            this.deserializer = deserializer;
        }

        public T Deserialize(string data, IDeserializerOptions options)
        {
            return deserializer.Deserialize(data, options);
        }
    }



}
