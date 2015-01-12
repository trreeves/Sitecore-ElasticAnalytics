namespace ElasticAnalytics.Repository.Elasticsearch.QueryContext
{
    public class EsQueryContext : IQueryContext
    {
        public EsQueryContext(string index, string type, string shardKey = null)
        {
            this.Index = index;
            this.Type = type;
            this.ShardKey = shardKey;
        }

        public string Index { get; private set; }

        public string Type { get; private set; }

        public string ShardKey { get; private set; }
    }
}