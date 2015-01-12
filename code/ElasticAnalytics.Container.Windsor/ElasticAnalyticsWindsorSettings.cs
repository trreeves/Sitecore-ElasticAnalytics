using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticAnalytics.Container.Windsor
{
    public static class ElasticAnalyticsWindsorSettings
    {
        public static class Configuration
        {
            public const string ContactStorageSettingsKey = "ContactStorage";

            public const string ContactLeaseStorageSettingsKey = "ContactLeaseStorage";

            public const string ContactIdentityMapSettingsKey = "ContactIdentityMap";
        }
    }
}
