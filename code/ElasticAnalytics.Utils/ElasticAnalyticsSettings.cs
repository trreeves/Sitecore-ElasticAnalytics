using System;

namespace ElasticAnalytics.Utils
{
    public static class ElasticAnalyticsSettings
    {
        public static class SettingsKeys
        {
            public const string SystemKey = "ElasticAnalytics.SystemKey";

            public const string InstanceKeyKey = "ElasticAnalytics.InstanceKey";

            public const string EsConnectionString = "ElasticAnalytics.Repository.EsConnectionString";
        }

        public static readonly TimeSpan MaxAcceptedClockDeviation = new TimeSpan(1000);

        public static class EsStorage
        {
            public const string ContactIndexName = "contact";

            public const string ContactTypeName = "contact";

            public const string LeaseTypeName = "lease";

            public const string IdentityMapTypeName = "identitymap";
        }
    }
}
