namespace ElasticAnalytics.Repository.Elasticsearch.QueryContext
{
    using ElasticAnalytics.Repository.Types;
    using ElasticAnalytics.Service.Types;

    public class StandardRequestConfiguration : IRequestConfiguration
    {
        private readonly string indexIdentifier;

        private readonly string typeIdentifier;

        public StandardRequestConfiguration(string indexIdentifier, string typeIdentifier)
        {
            this.indexIdentifier = indexIdentifier.ToLower();
            this.typeIdentifier = typeIdentifier.ToLower();
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
