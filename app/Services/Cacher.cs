using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using app.Models;
using Microsoft.Extensions.Logging;

namespace app.Services
{
    public interface ICacher
    {
        Cached<T> Get<T>(string[] key, [CallerMemberName] string name=default);
        void Set<T>(T val, string[] keys, [CallerMemberName] string name=default);
    }

    public class Cacher : BaseService<Cacher>, ICacher
    {
        private Dictionary<string, object> cache = new();

        public Cacher(ILogger<Cacher> logger, IPerfService perfService) : base(null, logger, perfService)
        {
        }
        
        public Cached<T> Get<T>(string[] keys, [CallerMemberName] string name=default)
        {
            var builder = new StringBuilder();
            builder.Append(name);
            builder.Append('-');
            builder.AppendJoin('-', keys);
            var key = builder.ToString();
            
            if (!cache.ContainsKey(key)) return default;

            PerfService.CacheHit(key);
            return (Cached<T>) cache[key];
        }

        // private static readonly StringBuilder Builder = new(30);
        public void Set<T>(T val, string[] keys, [CallerMemberName] string name=default)
        {
            var builder = new StringBuilder();
            builder.Append(name);
            builder.Append('-');
            builder.AppendJoin('-', keys);
            var key = builder.ToString();
            if (cache.ContainsKey(key)) return;
            
            PerfService.CacheMiss(key);
            cache.Add(key, new Cached<T> { Value = val, HasValue = true});
        }


        public void Clear()
        {
            cache.Clear();
        }
    }

    public struct Cached<T>
    {
        public bool HasValue { get; set; }
        public T Value { get; init; }
    }
}