namespace ElasticAnalytics.Repository.Elasticsearch.Extensions
{
    using System;

    public static class GuidExtensions
    {
        public static string ToEsSafeId(this Guid guid)
        {
            return guid.ToString("N");
        }
    }
}
