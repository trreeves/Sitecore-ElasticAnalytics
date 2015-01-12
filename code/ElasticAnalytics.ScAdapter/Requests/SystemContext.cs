namespace ElasticAnalytics.ScAdapter.Requests
{
    using ElasticAnalytics.Service.Types;
    using ElasticAnalytics.SessionRepository.Types.Configuration;

    using ElasticAnalyticsSettings = ElasticAnalytics.Utils.ElasticAnalyticsSettings;

    public class SystemContext : ISystemContext
    {
        public SystemContext(ISettingsProvider settings)
        {
            this.InstanceKey = string.IsNullOrWhiteSpace(settings.Value(ElasticAnalyticsSettings.SettingsKeys.InstanceKeyKey)) ?
                "website" :
                settings.Value(ElasticAnalyticsSettings.SettingsKeys.InstanceKeyKey);

            this.SystemPrefix = string.IsNullOrWhiteSpace(settings.Value(ElasticAnalyticsSettings.SettingsKeys.SystemKey)) ?
                "EA" :
                settings.Value(ElasticAnalyticsSettings.SettingsKeys.SystemKey);
        }

        public string InstanceKey { get; private set; }

        public string SystemPrefix { get; private set; }
    }
}
