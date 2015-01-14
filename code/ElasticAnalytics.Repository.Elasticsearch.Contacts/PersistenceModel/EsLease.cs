namespace ElasticAnalytics.Repository.Elasticsearch.Contacts.PersistenceModel
{
    using System;

    using Newtonsoft.Json;

    public class EsLease : EsDocument
    {
        public DateTime Expires { get; set; }

        public EsLeaseOwner Owner { get; set; }

        [JsonIgnore]
        public long Version { get; set; }
    }
}
