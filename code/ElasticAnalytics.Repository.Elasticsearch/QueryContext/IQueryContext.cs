namespace ElasticAnalytics.Repository.Elasticsearch.QueryContext
{
    public interface IQueryContext
    {
        string Index { get; }
        string Type { get; }
        string ShardKey { get; }
    }
}
