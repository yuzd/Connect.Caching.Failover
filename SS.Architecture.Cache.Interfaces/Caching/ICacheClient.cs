using System;
using System.Collections.Generic;

namespace SS.Architecture.Interfaces.Caching
{
    public interface ICacheClient
    {
        T Get<T>(string key, string keyPrefix);
        T Get<T>(string key, string keyPrefix, Type inheritedType);
        T Get<T, TQ>(string key, string keyPrefix, Func<TQ, T> queryFunction, TQ queryFunctionParameter) where T : class;
        T Get<T, TQ>(string key, string keyPrefix, Func<TQ, T> queryFunction, TQ queryFunctionParameter, Type inheritedType) where T : class;
        T Get<T, TQ>(string key, string keyPrefix, Func<TQ, T> queryFunction, TQ queryFunctionParameter, TimeSpan? expireIn) where T : class;
        T Get<T, TQ>(string key, string keyPrefix, Func<TQ, T> queryFunction, TQ queryFunctionParameter, Type inheritedType, TimeSpan? expireIn) where T : class;
        IEnumerable<T> GetAllByPrefix<T>(string prefix);
        bool SetExpiry(string key, string keyPrefix, TimeSpan expireIn);
        void Set<T>(string key, string keyPrefix, T value);
        void Set<T>(string key, string keyPrefix, T value, TimeSpan expireIn);
        void Remove(string key, string keyPrefix);
        void RemoveAll(string keyPattern, string keyPrefix);
        long IncrementValue(string key, string keyPrefix);
        string GetQueryHash(IDictionary<string, object> query);
        string GetHash(string data);

        ICacheConfig Config { get; }
    }
}
