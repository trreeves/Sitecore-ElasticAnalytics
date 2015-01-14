namespace ElasticAnalytics.Repository.Elasticsearch.Contacts.PersistenceModel
{
    using ElasticAnalytics.Model.Contact;

    public class EsContactIdentification
    {
        public string Identity { get; set; }

        public AuthenticationLevel AuthLevel { get; set; }

        public IdentificationLevel IdentityLevel { get; set; }
    }
}
