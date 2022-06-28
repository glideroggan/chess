using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace app.Services
{
    public interface IPerfService
    {
        void TimeStart([CallerMemberName] string key = default);
        void LogOut();
        void TimeStop([CallerMemberName] string key = default);
        void On();
        void Off();
        void CacheHit(string key);
        void CacheMiss(string key);
    }   
    public class PerfService : IPerfService
    {
        private Dictionary<string, long> checks = new();
        private Dictionary<string, Stopwatch> _stopwatches = new();
        private Dictionary<string, int> _hits = new();
        private Dictionary<string, int> _misses = new();
        private ILogger<PerfService> Logger;
        private bool _on;

        public PerfService(ILogger<PerfService> logger)
        {
            Logger = logger;
        }
        public void TimeStart([CallerMemberName]string key="")
        {
            if (!_on) return;
            
            if (checks.ContainsKey(key))
            {
                var ch = checks[key];
                ch++;
                checks[key] = ch;

                var s = _stopwatches[key];
                if (!s.IsRunning) s.Start();
                return;
            }
            checks.Add(key, 1);
            _stopwatches.Add(key, Stopwatch.StartNew());
        }

        public void LogOut()
        {
            if (!_on) return;
            // Logger.LogWarning($"Logging out {checks.Keys.Count} props");
            foreach (var check in checks)
            {
                var s = _stopwatches[check.Key];
                var average = s.Elapsed.TotalMilliseconds / check.Value;
                if (average > 2)
                {
                    Logger.LogInformation($"{check.Key} took {average:F1} ms on average and " +
                                          $"was called {check.Value}|Total: {s.ElapsedMilliseconds} ms");                    
                }
                
            }

            var goodCacheHits = (from c in _hits
                    where c.Value > 1
                    group c by c.Key.Substring(0, c.Key.IndexOf('-'))
                    into method
                    orderby method.Key
                    select method)
                .Select(x => new { Key = x.Key, Count = x.Sum(x => x.Value)});
            foreach (var cacheHit in goodCacheHits.ToList())
            {
                Logger.LogInformation($"Cache hits: [{cacheHit.Key}]:{cacheHit.Count}");
            }
            var badCacheHits = (from c in _misses
                    group c by c.Key.Substring(0, c.Key.IndexOf('-'))
                    into method
                    from good in goodCacheHits
                    where good.Key != method.Key
                    orderby method.Key
                    select method)
                .Select(x => new { Key = x.Key, Count = x.Sum(x => x.Value)});
            foreach (var cacheHit in badCacheHits.ToList())
            {
                Logger.LogInformation($"Cache misses: [{cacheHit.Key}]:{cacheHit.Count}");
            }
        }

        public void TimeStop([CallerMemberName]string key="")
        {
            if (!_on) return;
            
            var s = _stopwatches[key];
            s.Stop();
        }

        public void On()
        {
            _stopwatches = new();
            checks = new();
            _on = true;
        }

        public void Off()
        {
            _on = false;
        }
        public void CacheHit(string key)
        {
            if (!_hits.ContainsKey(key)) _hits.Add(key, 0);
            _hits[key] += 1;
        }

        public void CacheMiss(string key)
        {
            if (!_misses.ContainsKey(key)) _misses.Add(key, 0);
            _misses[key] += 1;
        }
    }
}