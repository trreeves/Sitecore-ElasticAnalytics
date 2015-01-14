namespace ElasticAnalytics.Repository.Elasticsearch.PersistenceModel
{
    using ElasticAnalytics.Model.Contact;
    using ElasticAnalytics.Model.Locking;
    using ElasticAnalytics.Utils;

    public class ContactStorageConfiguration : EsRequestConfiguration<ElasticContact, EsContact>
    {
        public ContactStorageConfiguration()
            : base(
                ElasticAnalyticsSettings.EsStorage.ContactIndexName,
                ElasticAnalyticsSettings.EsStorage.ContactTypeName)
        {
        }
    }

    public class LeaseStorageConfiguration : EsRequestConfiguration<ElasticLease, EsLease>
    {
        public LeaseStorageConfiguration()
            : base(
                ElasticAnalyticsSettings.EsStorage.ContactIndexName,
                ElasticAnalyticsSettings.EsStorage.LeaseTypeName)
        {
        }
    }

    public class IdentityMapStorageConfiguration : EsRequestConfiguration<ElasticContactIdentityMap, EsContactIdentityMap>
    {
        public IdentityMapStorageConfiguration()
            : base(
                ElasticAnalyticsSettings.EsStorage.ContactIndexName,
                ElasticAnalyticsSettings.EsStorage.IdentityMapTypeName)
        {
        }
    }
}
