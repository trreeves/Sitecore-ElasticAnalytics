namespace ElasticAnalytics.Repository.Elasticsearch.Contacts.PersistenceModel
{
    using System;

    using ElasticAnalytics.Repository.Elasticsearch.PersistenceModel;

    /// <summary>The _id is the contact identity by the way.</summary>
    public class EsContactIdentityMap : EsDocument
    {
        public virtual Guid ContactId { get; set; }

        public virtual DateTime Updated { get; set; }
    }
}
