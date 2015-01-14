namespace ElasticAnalytics.Repository.Elasticsearch
{
    using ElasticAnalytics.Repository.Types;
    using ElasticAnalytics.Service.Types;

    public class EsRequestConfiguration<T, TRepo> : IRequestConfiguration
    {
        protected readonly string indexIdentifier;

        protected readonly string typeIdentifier;

        public EsRequestConfiguration(string index = null, string type = null)
        {
            this.indexIdentifier = (index ?? typeof(T).Name).Trim().ToLower();
            this.typeIdentifier = (type ?? typeof(T).Name).Trim().ToLower();
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
