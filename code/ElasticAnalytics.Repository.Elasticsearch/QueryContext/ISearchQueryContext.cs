namespace ElasticAnalytics.Repository.Elasticsearch.QueryContext
{
    public interface ISearchQueryContext : IQueryContext
    {
        int Skip { get; }
        int Take { get; }
    }
}