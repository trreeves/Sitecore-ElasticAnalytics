using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticAnalytics.Utils.Configuration
{
    using ElasticAnalytics.SessionRepository.Types.Configuration;

    public class InMemorySettingsProvider : ISettingsProvider
    {
        private readonly Dictionary<string, string> _settings;
 
        public InMemorySettingsProvider(params KeyValuePair<string, string>[] settings)
        {
            _settings = new Dictionary<string, string>();
            if (settings != null)
            {
                foreach (var keyValuePair in settings)
                {
                    _settings.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
        }

        public void AddValue(string key, string value)
        {
            _settings[key] = value;
        }

        public string Value(string key)
        {
            return _settings.ContainsKey(key) ? _settings[key] : null;
        }
    }
}
