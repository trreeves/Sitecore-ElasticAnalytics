﻿namespace ElasticAnalytics.Repository.Elasticsearch.Contacts.PersistenceModel
{
    using ElasticAnalytics.Repository.Elasticsearch.PersistenceModel;

    public class EsDocument : IEsDoc
    {
        public string _id { get; set; } 
    }
}