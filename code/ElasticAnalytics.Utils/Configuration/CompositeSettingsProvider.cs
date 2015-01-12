using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticAnalytics.Utils.Configuration
{
    using ElasticAnalytics.SessionRepository.Types.Configuration;

    public class CompositeSettingsProvider : ISettingsProvider
    {
        private readonly IEnumerable<ISettingsProvider> _providers;
 
        public CompositeSettingsProvider(params ISettingsProvider[] providers)
        {
            _providers = providers??Enumerable.Empty<ISettingsProvider>();
        }

        public string Value(string key)
        {
            return _providers.Select(p => p.Value(key)).FirstOrDefault(v => v != null);
        }
    }
}
