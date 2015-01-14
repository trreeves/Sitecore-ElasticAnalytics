namespace ElasticAnalytics.Configuration.Windsor.Repository
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ElasticAnalytics.Model.Contact;
    using ElasticAnalytics.Model.Locking;
    using ElasticAnalytics.Repository.Elasticsearch.Contacts.PersistenceModel;
    using ElasticAnalytics.Repository.Elasticsearch.Repositories;
    using ElasticAnalytics.Repository.Types.Repositories;

    public class ContactsRepositoryInProcessInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(

                Component
                    .For<IRepository<ElasticContact>>()
                    .ImplementedBy<EsRepository<ElasticContact, EsContact>>()
                    .LifestyleTransient(),

                Component
                    .For<IConcurrencyControlRepository<ElasticLease>>()
                    .ImplementedBy<EsConcurrencyControlRepository<ElasticLease, EsLease>>()
                    .LifestyleTransient(),

                Component
                    .For<IRepository<ElasticContactIdentityMap>>()
                    .ImplementedBy<EsRepository<ElasticContactIdentityMap, EsContactIdentityMap>>()
                    .LifestyleTransient()
            );
        }
    }
}
