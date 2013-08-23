using System;

namespace SS.Architecture.Interfaces.Serialization
{
    public interface ISerializer
    {
        byte[] SerializeObject<T>(T obj);
        T DeserializeObject<T>(byte[] serializedObj);
        T DeserializeObject<T>(byte[] serializedObj, Type type);
        bool CanSerialize(Type type);
    }
}