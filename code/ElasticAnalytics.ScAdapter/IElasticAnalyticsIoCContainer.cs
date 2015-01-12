namespace ElasticAnalytics.ScAdapter
{
    public interface IElasticAnalyticsIoCContainer
    {
        IElasticAnalyticsIoCContainer Initialize(params string[] tags);

        T Resolve<T>();
    }
}
