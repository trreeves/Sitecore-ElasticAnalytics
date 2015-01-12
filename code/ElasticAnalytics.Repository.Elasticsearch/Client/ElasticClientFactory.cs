namespace ElasticAnalytics.Repository.Elasticsearch.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ElasticAnalytics.SessionRepository.Types.Configuration;

    using Nest;

    using Newtonsoft.Json;

    using ElasticAnalyticsSettings = ElasticAnalytics.Utils.ElasticAnalyticsSettings;

    public class ElasticClientFactory : IElasticClientFactory
    {
        protected readonly ISettingsProvider settings;

        protected readonly IEnumerable<JsonConverter> converters;

        public ElasticClientFactory(
            ISettingsProvider settings,
            IEnumerable<JsonConverter> converters)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (converters == null)
            {
                converters = Enumerable.Empty<JsonConverter>();
            }

            this.settings = settings;
            this.converters = converters;
        }

        public ElasticClient Create()
        {
            var connectionString = this.settings.Value(ElasticAnalyticsSettings.SettingsKeys.EsConnectionString);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(ElasticAnalyticsSettings.SettingsKeys.EsConnectionString, "No ES connection string was present");    
            }

            return new ElasticClient(
                new ConnectionSettings(
                    uri: new Uri(connectionString))
                    .DisableAutomaticProxyDetection(false)
                    .AddContractJsonConverters(
                        this.converters.Select<JsonConverter, Func<Type, JsonConverter>>(c => (type) => c.CanConvert(type) ? c : null).ToArray()));  
        }
    }
}
