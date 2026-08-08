using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
        /// Defines contract for distributed cache operations.
        /// Supports Redis/Upstash implementation.
        public interface ICacheService
        {
     
            Task<T?> GetAsync<T>(string key);

           
            Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

            Task RemoveAsync(string key);


           
            Task<bool> ExistsAsync(string key);


            /// Gets or creates a cached value using a factory function.
            Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
        }
}
