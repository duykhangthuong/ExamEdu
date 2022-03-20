using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace BackEnd.Services.Cache
{
    public class CacheProvider : ICacheProvider
    {
        private readonly IRedisCacheClient _cache;

        public CacheProvider(IRedisCacheClient cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Get a value (include class data) in cache using key
        /// </summary>
        /// <typeparam name="T">A class</typeparam>
        public async Task<T> GetFromCache<T>(string key) where T : class
        {
            var cacheRes = await _cache.Db0.GetAsync<T>(key);
            return cacheRes == null ? null : cacheRes;
        }

        /// <summary>
        /// Get all key value in database
        /// </summary>
        /// <typeparam name="T">A class</typeparam>
        public async Task<List<T>> GetAllValueFromCache<T>() where T : class
        {
            List<T> listResult = new List<T>();
            IEnumerable<string> keyList = await _cache.Db0.SearchKeysAsync("*@*");

            foreach (var key in keyList)
            {
                var temp = await GetFromCache<List<T>>(key);
                foreach (var value in temp)
                {
                    listResult.Add(value);
                }
            }
            return listResult == null ? null : listResult;
        }

        /// <summary>
        /// Insert new data (include class data) to cache databse
        /// </summary>
        /// <typeparam name="T">A class</typeparam>
        public async Task SetCache<T>(string key, T value) where T : class
        {
            bool added = await _cache.Db0.AddAsync(key, value);
        }

        /// <summary>
        /// Add value to existing key
        /// </summary>
        /// <typeparam name="T">A class</typeparam>
        public async Task AddValueToKey<T>(string key, T value)
        {
            LinkedList<T> valueList = new LinkedList<T>();
            try
            {
                valueList = await GetFromCache<LinkedList<T>>(key);
                valueList.AddLast(value);
            }
            catch (Exception)
            {
                valueList = new LinkedList<T>();
                valueList.AddLast(value);
            }
            await SetCache<LinkedList<T>>(key, valueList);
        }

        /// <summary>
        /// Clear value in cache using key
        /// </summary>
        /// <param name="key">What to clear</param>
        public async Task ClearCache(string key)
        {
            await _cache.Db0.RemoveAsync(key);
        }
    }
}