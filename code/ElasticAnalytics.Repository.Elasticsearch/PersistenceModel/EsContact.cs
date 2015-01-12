namespace ElasticAnalytics.Repository.Elasticsearch.PersistenceModel
{
    using System;

    using Newtonsoft.Json.Linq;

    public class EsContact : EsDocument
    {
        public bool Obsolete { get; set; }

        public EsContactIdentification Identification { get; set; }

        public Guid? Successor { get; set; }

        public JObject Metadata { get; set; }

        public JObject Facets { get; set; }
    }
}
