namespace ElasticAnalytics.Repository.Elasticsearch.QueryContext
{
    public class EsSearchQueryContext : EsQueryContext, ISearchQueryContext
    {
        public EsSearchQueryContext(string index, string type, string shardKey = null, int skip = 0, int take = 1) :
            base(index, type, shardKey)
        {
            this.Skip = skip;
            this.Take = take;
        }

        public int Skip { get; private set; }

        public int Take { get; private set; }
    }
}