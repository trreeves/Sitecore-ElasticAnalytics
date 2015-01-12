﻿namespace ElasticAnalytics.Repository.Elasticsearch.PersistenceModel
{
    using ElasticAnalytics.Model.Locking;

    public class EsLeaseOwner
    {
        public string Id { get; set; }

        public LeaseOwnerType Type { get; set; }
    }
}
