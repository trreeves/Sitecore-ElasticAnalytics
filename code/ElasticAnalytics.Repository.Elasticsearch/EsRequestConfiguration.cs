namespace ElasticAnalytics.Repository.Elasticsearch
{
    using ElasticAnalytics.Model.Contact;
    using ElasticAnalytics.Model.Locking;
    using ElasticAnalytics.Repository.Types;
    using ElasticAnalytics.Service.Types;
    using ElasticAnalytics.Utils;

    public class EsRequestConfiguration<T, TRepo> : IRequestConfiguration
    {
        private readonly string indexIdentifier;

        private readonly string typeIdentifier;

        public EsRequestConfiguration()
        {
            if (typeof(T) == typeof(ElasticContact))
            {
                this.indexIdentifier = ElasticAnalyticsSettings.EsStorage.ContactIndexName;
                this.typeIdentifier = ElasticAnalyticsSettings.EsStorage.ContactTypeName;
            }

            else if (typeof(T) == typeof(ElasticLease))
            {
                this.indexIdentifier = ElasticAnalyticsSettings.EsStorage.ContactIndexName;
                this.typeIdentifier = ElasticAnalyticsSettings.EsStorage.LeaseTypeName;
            }

            else if (typeof(T) == typeof(ElasticContactIdentityMap))
            {
                this.indexIdentifier = ElasticAnalyticsSettings.EsStorage.ContactIndexName;
                this.typeIdentifier = ElasticAnalyticsSettings.EsStorage.IdentityMapTypeName;
            }

            this.indexIdentifier = this.indexIdentifier.Trim().ToLower();
            this.typeIdentifier = this.typeIdentifier.Trim().ToLower();
        }

        public string GenerateIndexIdentifier(ISystemContext ctx)
        {
            return string.Format("{0}_{1}_{2}", ctx.SystemPrefix, ctx.InstanceKey, this.indexIdentifier).ToLower();
        }

        public string GenerateTypeIdentifier(ISystemContext ctx)
        {
            return this.typeIdentifier;
        }
    }
}
