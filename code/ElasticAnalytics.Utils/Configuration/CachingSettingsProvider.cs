using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticAnalytics.Utils.Configuration
{
    using ElasticAnalytics.SessionRepository.Types.Configuration;

    public class CachingSettingsProvider : ISettingsProvider
    {
        private readonly ISettingsProvider _provider;

        private readonly Dictionary<string, string> _cache;
 
        public CachingSettingsProvider(ISettingsProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            _provider = provider;
            _cache = new Dictionary<string, string>();
        }

        public string Value(string key)
        {
            if (!_cache.ContainsKey(key))
            {
                var value = _provider.Value(key);
                _cache[key] = value;
                return value;
            }

            return _cache[key];
        }
    }
}
