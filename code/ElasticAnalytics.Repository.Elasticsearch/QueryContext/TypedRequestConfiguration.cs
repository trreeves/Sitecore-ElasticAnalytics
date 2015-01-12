namespace ElasticAnalytics.Repository.Elasticsearch.QueryContext
{
    using ElasticAnalytics.Repository.Types;
    using ElasticAnalytics.Service.Types;

    public class TypedRequestConfiguration<TIndex, TType> : IEsRequestConfiguration
    {
        private readonly string indexIdentifier;

        private readonly string typeIdentifier;

        public TypedRequestConfiguration()
        {
            this.indexIdentifier = typeof(TIndex).Name.ToLower();
            this.typeIdentifier = typeof(TType).Name.ToLower();
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