namespace ElasticAnalytics.Repository.Elasticsearch.Contacts.PersistenceModel
{
    using System;

    using ElasticAnalytics.Repository.Elasticsearch.PersistenceModel;

    using Newtonsoft.Json;

    public class EsLease : EsDocument
    {
        public DateTime Expires { get; set; }

        public EsLeaseOwner Owner { get; set; }

        [JsonIgnore] // This is an inbuilt field in ES, so we don't want to bother round tripping it as part of the normal serialization.
        public long Version { get; set; }
    }
}
