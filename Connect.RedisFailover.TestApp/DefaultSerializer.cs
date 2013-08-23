using System.IO;
using System;
using System.Xml.Serialization;
using SS.Architecture.Interfaces.Serialization;

namespace Connect.RedisFailover.TestApp
{
    public class DefaultSerializer : ISerializer
    {
        private readonly XmlSerializer _serializer;

        public DefaultSerializer()
        {
            _serializer = new XmlSerializer(typeof(string));
        }

        public byte[] SerializeObject<T>(T obj)
        {
            byte[] serializedObj;
            using (var stream = new System.IO.MemoryStream())
            {
                _serializer.Serialize(stream, obj);
                serializedObj = stream.ToArray();
            }
            return serializedObj;
        }

        public T DeserializeObject<T>(byte[] serializedObj)
        {
            var obj = default(T);
            if (serializedObj != null)
            {
                using (var stream = new MemoryStream(serializedObj))
                {
                    obj = (T)_serializer.Deserialize(stream);
                }
            }
            return obj;
        }

        public T DeserializeObject<T>(byte[] serializedObj, Type type)
        {
            var obj = default(T);
            if (serializedObj != null)
            {
                using (var stream = new MemoryStream(serializedObj))
                {
                    obj = (T)_serializer.Deserialize(stream);
                }
            }
            return obj;
        }

        public bool CanSerialize(Type type)
        {
            return type.GetType() == typeof(String);
        }
    }
}
