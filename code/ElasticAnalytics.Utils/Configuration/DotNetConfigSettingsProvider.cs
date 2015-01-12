using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticAnalytics.Utils.Configuration
{
    using System.Configuration;

    using ElasticAnalytics.SessionRepository.Types.Configuration;

    public class DotNetConfigSettingsProvider : ISettingsProvider
    {
        public string Value(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
