namespace ElasticAnalytics.Repository.Elasticsearch.Client
{
    using Nest;

    public interface IElasticClientFactory
    {
        ElasticClient Create();
    }
}
