using ServiceStack.Redis;
using SS.Architecture.Interfaces.Caching;
using SS.Architecture.Interfaces.Caching.Monitoring;
using SS.Architecture.Interfaces.Serialization;
using SS.Architecture.Logging.Contract;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace SS.Architecture.Cache.Redis
{
    public partial class RedisCacheClient : ICacheClient
    {
        protected readonly ISerializer Serializer;
        protected readonly ILogger Logger;
        protected readonly ISentinelClientsManager SentinelClientsManager;

        public RedisCacheClient(ILogger logger, ISerializer serializer, ICacheConfig cacheConfig, ISentinelClientsManager sentinelClientsManager)
        {
            Contract.Assert(cacheConfig != null, "Cache Configuration cannot be null!");
            Contract.Assert(logger != null);
            Contract.Assert(serializer != null);

            Config = cacheConfig;
            Serializer = serializer;
            Logger = logger;
            SentinelClientsManager = sentinelClientsManager;

            Initialize();
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <returns></returns>
        public T Get<T>(string key, string keyPrefix)
        {
            return Get<T>(key, keyPrefix, typeof(T));
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <param name="inheritedType">Type of the inherited.</param>
        /// <returns></returns>
        public T Get<T>(string key, string keyPrefix, Type inheritedType)
        {
            key = string.Format("{0}{1}", keyPrefix, key);
            return Get<T>(key, inheritedType);
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TQ">The type of the Q.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <param name="queryFunction">The query function.</param>
        /// <param name="queryFunctionParameter">The query function parameter.</param>
        /// <returns></returns>
        public T Get<T, TQ>(string key, string keyPrefix, Func<TQ, T> queryFunction, TQ queryFunctionParameter) where T : class
        {
            return Get(key, keyPrefix, queryFunction, queryFunctionParameter, typeof(T), null);
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TQ">The type of the Q.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <param name="queryFunction">The query function.</param>
        /// <param name="queryFunctionParameter">The query function parameter.</param>
        /// <param name="inheritedType">Type of the inherited.</param>
        /// <returns></returns>
        public T Get<T, TQ>(string key, string keyPrefix, Func<TQ, T> queryFunction, TQ queryFunctionParameter, Type inheritedType) where T : class
        {
            return Get(key, keyPrefix, queryFunction, queryFunctionParameter, inheritedType, null);
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TQ">The type of the Q.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <param name="queryFunction">The query function.</param>
        /// <param name="queryFunctionParameter">The query function parameter.</param>
        /// <param name="expireIn">The expire in.</param>
        /// <returns></returns>
        public T Get<T, TQ>(string key, string keyPrefix, Func<TQ, T> queryFunction, TQ queryFunctionParameter, TimeSpan? expireIn) where T : class
        {
            return Get(key, keyPrefix, queryFunction, queryFunctionParameter, typeof(T), expireIn);
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TQ">The type of the Q.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <param name="queryFunction">The query function.</param>
        /// <param name="queryFunctionParameter">The query function parameter.</param>
        /// <param name="inheritedType">Type of the inherited.</param>
        /// <param name="expireIn">The expire in.</param>
        /// <returns></returns>
        public T Get<T, TQ>(string key, string keyPrefix, Func<TQ, T> queryFunction, TQ queryFunctionParameter, Type inheritedType, TimeSpan? expireIn) where T : class
        {
            var value = default(T);

            if (Config.UseCache)
            {
                value = Get<T>(key, keyPrefix);

                if (value == null)
                {
                    value = queryFunction.Invoke(queryFunctionParameter);

                    if (value != null)
                    {
                        if (expireIn.HasValue) { Set(key, keyPrefix, expireIn.Value); }
                        else { Set(key, keyPrefix, value); }
                    }
                }
            }
            else
            {
                value = queryFunction.Invoke(queryFunctionParameter);
            }

            return value;
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="inheritedType">Type of the inherited.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <returns></returns>
        private T Get<T>(string key, Type inheritedType)
        {
            var value = default(T);
            if (Config.UseCache)
            {
                using (var client = GetRedisReadOnlyClient())
                {
                    try
                    {
                        value = Serializer.DeserializeObject<T>(client.Get<byte[]>(key), inheritedType);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("GetValue threw Exception.", ex, "key", key);
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Gets all by prefix.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        public IEnumerable<T> GetAllByPrefix<T>(string prefix)
        {
            IEnumerable<T> result = null;
            if (Config.UseCache)
            {
                try
                {
                    using (var client = GetRedisReadOnlyClient())
                    {
                        result = GetAllByPrefix<T>(client, prefix);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("GetAllByPrefix threw Exception.", ex, "prefix", prefix);
                }
            }
            return result;
        }

        /// <summary>
        /// Gets all by prefix.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client">The client.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        private IEnumerable<T> GetAllByPrefix<T>(IRedisClient client, string prefix)
        {
            IEnumerable<string> keys = client.GetAllKeys();
            if (keys != null && keys.Any())
            {
                keys = keys.Where(x => x.StartsWith(prefix));
                var values = client.GetAll<byte[]>(keys);
                return values.Select(item => Serializer.DeserializeObject<T>(item.Value)).ToList();
            }
            return null;
        }

        /// <summary>
        /// Sets the expiry.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <param name="expireIn">The expire in.</param>
        /// <returns></returns>
        public bool SetExpiry(string key, string keyPrefix, TimeSpan expireIn)
        {
            var result = false;

            if (Config.UseCache)
            {
                try
                {
                    using (var client = GetRedisClient())
                    {
                        result = client.ExpireEntryIn(key, expireIn);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("SetExpiry Set threw Exception.", ex, "key", key, "expireIn", expireIn);
                }
            }
            return result;
        }

        /// <summary>
        /// Sets the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <param name="value">The value.</param>
        public void Set<T>(string key, string keyPrefix, T value)
        {
            if (Config.UseCache)
            {
                try
                {
                    using (var client = GetRedisClient())
                    {
                        client.Set(key, Serializer.SerializeObject(value));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Set<T> Set threw Exception.", ex, "key", key, "value", value);
                }
            }
        }

        /// <summary>
        /// Sets the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <param name="value">The value.</param>
        /// <param name="expireIn">The expire in.</param>
        public void Set<T>(string key, string keyPrefix, T value, TimeSpan expireIn)
        {
            if (Config.UseCache)
            {
                try
                {
                    using (var client = GetRedisClient())
                    {
                        client.Set(key, Serializer.SerializeObject(value), expireIn);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Set<T> Set threw Exception.", ex, "key", key, "value", value);
                }
            }
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="keyPrefix">The key prefix.</param>
        public void Remove(string key, string keyPrefix)
        {
            if (Config.UseCache)
            {
                try
                {
                    key = keyPrefix + key;
                    using (var client = GetRedisClient())
                    {
                        client.Remove(key);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Remove threw Exception.", ex, "key", key);
                }
            }
        }

        /// <summary>
        /// Removes all.
        /// </summary>
        /// <param name="keyPattern">The key pattern.</param>
        /// <param name="keyPrefix">The key prefix.</param>
        public void RemoveAll(string keyPattern, string keyPrefix)
        {
            if (Config.UseCache)
            {
                using (var client = GetRedisClient())
                {
                    keyPattern = keyPrefix + keyPattern;
                    try
                    {
                        var keys = client.SearchKeys(keyPattern);
                        client.RemoveAll(keys);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Remove threw Exception.", ex, "keyPattern", keyPattern);
                    }
                }
            }
        }

        /// <summary>
        /// Increments the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <returns></returns>
        public long IncrementValue(string key, string keyPrefix)
        {
            long value = 0;
            if (Config.UseCache)
            {
                key = keyPrefix + key;
                try
                {
                    using (var client = GetRedisClient())
                    {
                        value = client.IncrementValue(key);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("IncrementValue threw Exception.", ex, "key", key);
                }
                
            }
            return value;
        }

        /// <summary>
        /// Gets the query hash.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public string GetQueryHash(IDictionary<string, object> query)
        {
            var queryString = string.Join(",", query.Select(kvp => kvp.Key + "," + kvp.Value.ToString()));
            var queryStringHash = GetHash(queryString);

            return queryStringHash;
        }

        /// <summary>
        /// Gets the hash.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public string GetHash(string data)
        {
            return data;
            //Going to remove key hashing temporarily to see if this helps with potential caching problems
            //var hash = data;
            //var hash = _verifiableHash.GenerateHash(data);
            //hash = hash.ToBase64ForUrl();
            //return hash;
        }

        public ICacheConfig Config { get; private set; }

    }
}
