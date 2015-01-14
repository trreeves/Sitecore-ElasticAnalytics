namespace ElasticAnalytics.Tests.Common.EsUtils
{
    using System;

    using ElasticAnalytics.Model.Configuration;
    using ElasticAnalytics.Repository.Types;
    using ElasticAnalytics.Service.Types;
    using ElasticAnalytics.SessionRepository.Types.Configuration;

    using Nest;

    using ElasticAnalyticsSettings = ElasticAnalytics.Utils.ElasticAnalyticsSettings;

    public class TestIndexUtils : IDisposable
    {
        private readonly ISystemContext ctx;

        private readonly ElasticClient  client;

        private readonly IRequestConfiguration[] requestConfigs;

        public TestIndexUtils(
            ISettingsProvider settings, 
            ISystemContext ctx,
            params IRequestConfiguration[] requestConfig)
        {
            this.ctx = ctx;
            this.requestConfigs = requestConfig;

            this.client =
                new ElasticClient(
                    new ConnectionSettings
                        (uri: new Uri(settings.Value(ElasticAnalyticsSettings.SettingsKeys.EsConnectionString)))
                        .DisableAutomaticProxyDetection(false));

            this.CreateIndices();
        }

        private void CreateIndices()
        {
            foreach (var esRequestConfiguration in this.requestConfigs)
            {
                this.CreateIndex(esRequestConfiguration);
            }    
        }

        private void CreateIndex(IRequestConfiguration requestConfig)
        {
            var indexName = requestConfig.GenerateIndexIdentifier(this.ctx);
            this.client.Raw.IndicesCreate(indexName, null);

            const int CircuitBreaker = 2;
            int count = 0;
            do
            {
                if (count > 0) // on retries - wait a bit
                    System.Threading.Thread.Sleep(25);

                // ES creates new indices in an async fashion, so you have to check manually when the index is ready
                // across the whole cluster to receive writes.  Waiting for one healthy shard is good enough.
                this.client.ClusterHealth(p => p.Index(indexName).WaitForActiveShards(1).Timeout("5s"));
                count++;
            }
            while (count <= CircuitBreaker);
        }

        private void DeleteIndices()
        {
            foreach (var esRequestConfiguration in this.requestConfigs)
            {
                this.DeleteIndex(esRequestConfiguration);
            }    
        }

        private void DeleteIndex(IRequestConfiguration requestConfig)
        {
            this.client.Raw.IndicesDelete(requestConfig.GenerateIndexIdentifier(this.ctx));
        }

        public void Dispose()
        {
            DeleteIndices();
        }
    }
}
