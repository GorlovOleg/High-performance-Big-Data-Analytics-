/*
Author          : Technical  Architect / Application Developer Oleg Gorlov
Description:	: Class CacheManager to serve StackExchange.Redis. 
                : Class is  that include methods to insert and update SQL database Tables and used Entity Framework functionality .NET Core.
Copyright       : DACgroup 
email           : ogorlov@dacgroup.com
Date            : 10/28/2017
Release         : 1.1.0
Comment         : 
*/
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Newtonsoft;
using System.Collections.Generic;
using System;
using StackExchange.Redis;
using Polly;
using System.Text.RegularExpressions;
using System.Linq;

namespace DAC.LLM.Cache.Redis
{
    public class CacheManager : ICacheManager
    {
        private const int TRIES = 4;
        private const string DEFAULT_DB_PATTERN = @"defaultDatabase\=(?<dbNum>\d{1,2})";
        private const int CONNECTION_EXPIRATION_TIME = 5;

        private StackExchangeRedisCacheClient cacheClient;
        private string connectionString;

        private static readonly object checkExpirationLock = new object();
        private static readonly object lockObject = new object();
        private static volatile Dictionary<string, Tuple<DateTime, StackExchangeRedisCacheClient>> openConnections = new Dictionary<string, Tuple<DateTime, StackExchangeRedisCacheClient>>();

        public CacheManager() : this(string.Empty) { }

        public CacheManager(string connectionString)
        {
            this.connectionString = connectionString;

            cacheClient = CreateInstance(connectionString);
        }

        public static StackExchangeRedisCacheClient CreateInstance(string connectionString)
        {
            lock (lockObject)
            {
                if (openConnections == null)
                    openConnections = new Dictionary<string, Tuple<DateTime, StackExchangeRedisCacheClient>>();

                if (!openConnections.ContainsKey(connectionString))
                {
                    var redisConnection = Policy
                                   .Handle<Exception>()
                                   .WaitAndRetry(TRIES, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                                   .Execute(() => GetCacheClient(connectionString));

                    openConnections.Add(connectionString, new Tuple<DateTime, StackExchangeRedisCacheClient>(DateTime.UtcNow, redisConnection));
                }

                return openConnections[connectionString].Item2;
            }
        }
        
        private void CloseConnection()
        {
            //Invalidate the connection after {TRIES} times exceptions.
            try
            {
                cacheClient.Dispose();
            }
            catch (Exception){}
            try
            {
                openConnections.Remove(connectionString);
            }
            catch (Exception){}
        }

        private static StackExchangeRedisCacheClient GetCacheClient(string connectionString)
        {
            var serializer = new NewtonsoftSerializer();
            if (string.IsNullOrWhiteSpace(connectionString))
                return new StackExchangeRedisCacheClient(serializer);
            else
            {
                return new StackExchangeRedisCacheClient(serializer, connectionString, GetDefaultDatabase(connectionString));
            }
        }

        private static int GetDefaultDatabase(string connectionString)
        {
            try
            {
                var databaseNumber = 0;
                if (Regex.IsMatch(connectionString, DEFAULT_DB_PATTERN))
                {
                    databaseNumber = int.Parse(Regex.Match(connectionString, DEFAULT_DB_PATTERN, RegexOptions.IgnoreCase).Groups["dbNum"].Value);
                }
                return databaseNumber;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private TResult ExecuteWithRetry<TResult>(Func<TResult> action)
        {
            try
            {
                SanitizeConnection();

                //  Math.Pow()
                //  2 ^ 1 = 2 seconds then
                //  2 ^ 2 = 4 seconds then
                //  2 ^ 3 = 8 seconds then
                //  2 ^ 4 = 16 seconds then
                return Policy
                           .Handle<Exception>()
                           .WaitAndRetry(TRIES, retryAttempt =>
                           {
                               //If I am trying to recreate my connection for the second time or more, try to recreate the connection.
                               if (retryAttempt > 1)
                               {
                                   SanitizeConnection(forceRecreation: true);
                               }
                               return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                           })
                           .Execute(action);
            }
            catch (Exception)
            {
                CloseConnection();
                throw;
            }
        }

        private void SanitizeConnection(bool forceRecreation = false)
        {
            lock (checkExpirationLock)
            {
                //It will recreate a connection if forced OR
                //if the conection exists in the dictionaryCache and is older than CONNECTION_EXPIRATION_TIME constant
                if (forceRecreation || (openConnections.ContainsKey(connectionString) && (DateTime.UtcNow - openConnections[connectionString].Item1).TotalMinutes >= CONNECTION_EXPIRATION_TIME))
                {
                    //Point to the current CacheClient
                    var tempDisposableClient = cacheClient;

                    //Remove it from the cache Dictionary
                    openConnections.Remove(connectionString);

                    //Create a new one and store in the cache
                    cacheClient = CreateInstance(connectionString);

                    //Dispose the old cache client
                    tempDisposableClient.Dispose();
                    tempDisposableClient = null;
                }
            }
        }

        public bool Contains(string key)
        {
            return ExecuteWithRetry(() => cacheClient.Database.KeyExists(key));
        }


        public bool Add(string key, string value)
        {
            return ExecuteWithRetry(() => cacheClient.Database.StringSet(key, value));
        }

        public bool Replace(string key, string value)
        {
            return Add(key, value);
        }

        public string Get(string key)
        {
            return ExecuteWithRetry<string>(() =>
            {
                if (cacheClient.Database.KeyExists(key))
                    return cacheClient.Database.StringGet(key);

                return string.Empty;
            });
        }

        public bool Remove(string key)
        {
            return ExecuteWithRetry(() =>
            {
                if (cacheClient.Database.KeyExists(key))
                    return cacheClient.Database.KeyDelete(key);

                return true;
            });
        }

        public void RemoveAll(IEnumerable<string> keys)
        {
            try
            {
                Policy
                    .Handle<Exception>()
                    .WaitAndRetry(TRIES, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                    .Execute(() => cacheClient.RemoveAllAsync(keys));
            }
            catch (Exception)
            {
                CloseConnection();
                throw;
            }
        }

        public void RemoveAll(string pattern)
        {
            try
            {
                Policy
                .Handle<Exception>()
                .WaitAndRetry(TRIES, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                .Execute(() => cacheClient.Database.ScriptEvaluate("local keys = redis.call('keys', ARGV[1]) for i=1,#keys,5000 do redis.call('del', unpack(keys, i, math.min(i+4999, #keys))) end return keys", null, new RedisValue[] { pattern }));
            }
            catch (Exception)
            {
                CloseConnection();
                throw;
            }
        }

        public T Get<T>(string key)
        {
            return ExecuteWithRetry(() => cacheClient.Get<T>(key));
        }

        public bool Add<T>(string key, T value)
        {
            return ExecuteWithRetry(() => cacheClient.Add(key, value));
        }

        public bool AddAll<T>(IList<Tuple<string, T>> items)
        {
            var result = false;

            var pages = items.Count / 1000.0;

            for (int i = 0; i < pages; i++)
            {
                var newItems = items.Skip(i * 1000).Take(1000).ToList();
                result = ExecuteWithRetry(() => cacheClient.AddAll(newItems));
            }

            return result;
        }

        public bool Replace<T>(string key, T value)
        {
            return ExecuteWithRetry(() => cacheClient.Replace(key, value));
        }

        public IEnumerable<string> SearchKeys(string pattern)
        {
            var result =  ExecuteWithRetry(() => (RedisResult[])cacheClient.Database.ScriptEvaluateAsync("return redis.call('keys', ARGV[1])", null, new RedisValue[] { pattern }).Result);
            return result.ToList().ConvertAll(item => item.ToString());
        }

        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
        {
            return ExecuteWithRetry(() => cacheClient.GetAll<T>(keys));
        }

        public bool Add<T>(string key, T value, TimeSpan expiresIn)
        {
            return ExecuteWithRetry(() => cacheClient.Add(key, value, expiresIn));
        }

        public bool Replace<T>(string key, T value, TimeSpan expiresIn)
        {
            return ExecuteWithRetry(() => cacheClient.Replace(key, value, expiresIn));
        }

        public bool HashSet<T>(string hashKey, string key, T value, bool fireAndForget = false)
        {
            return ExecuteWithRetry(() => cacheClient.HashSet(hashKey, key, value, false, fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None));
        }

        public T HashGet<T>(string hashKey, string key)
        {
            return ExecuteWithRetry(() => cacheClient.HashGet<T>(hashKey, key));
        }

        public IDictionary<string, T> HashGetAll<T>(string hashKey)
        {
            return ExecuteWithRetry(() => cacheClient.HashGetAll<T>(hashKey));
        }

        public bool HashDelete(string hashKey, string key)
        {
            return ExecuteWithRetry(() => cacheClient.HashDelete(hashKey, key));
        }

        /// <summary>
        /// Uses the http://redis.io/commands/sadd
        /// </summary>
        public bool SetAdd(string key, string value)
        {
            return ExecuteWithRetry(() => cacheClient.Database.SetAdd(key, value));
        }

        /// <summary>
        /// Uses the http://redis.io/commands/srem
        /// </summary>
        public bool SetRemove(string key, string value)
        {
            return ExecuteWithRetry(() => cacheClient.Database.SetRemove(key, value));
        }

        /// <summary>
        /// Uses the http://redis.io/commands/smembers
        /// </summary>
        public string[] ListSetMembers(string key)
        {
            return ExecuteWithRetry(() => cacheClient.Database.SetMembers(key).ToStringArray());
        }


        /// <summary>
        /// Uses the http://redis.io/commands/zadd
        /// </summary>
        public bool SortedSetAdd(string key, string value, double score = 2)
        {
            return ExecuteWithRetry(() => cacheClient.Database.SortedSetAdd(key, value, score));
        }

        /// <summary>
        /// Uses the http://redis.io/commands/zrem
        /// </summary>
        public bool SortedSetRemove(string key, string value)
        {
            return ExecuteWithRetry(() => cacheClient.Database.SortedSetRemove(key, value));
        }

        /// <summary>
        /// Uses the http://redis.io/commands/zrange
        /// </summary>
        public string[] ListSortedSetMembers(string key)
        {
            return ExecuteWithRetry(() => cacheClient.Database.SortedSetRangeByRank(key).ToStringArray());
        }

        public long StringIncrement(string key, long value = 1)
        {
            return ExecuteWithRetry(() => cacheClient.Database.StringIncrement(key, value));
        }

        public long StringDecrement(string key, long value = 1)
        {
            return ExecuteWithRetry(() => cacheClient.Database.StringDecrement(key, value));
        }

        public void Dispose()
        {
            try
            {
                cacheClient = null;
                GC.SuppressFinalize(this);
            }
            catch (Exception)
            {
                return;
            }
        }
...
