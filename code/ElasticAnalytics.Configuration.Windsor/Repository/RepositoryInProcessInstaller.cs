namespace ElasticAnalytics.Configuration.Windsor.Repository
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ElasticAnalytics.Model.Contact;
    using ElasticAnalytics.Model.Locking;
    using ElasticAnalytics.Repository.Elasticsearch;
    using ElasticAnalytics.Repository.Elasticsearch.Client;
    using ElasticAnalytics.Repository.Elasticsearch.PersistenceModel;
    using ElasticAnalytics.Repository.Elasticsearch.Repositories;
    using ElasticAnalytics.Repository.Elasticsearch.Serialization;
    using ElasticAnalytics.Repository.Types.Repositories;

    using Nest;

    using Newtonsoft.Json;

    public class RepositoryInProcessInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(

                // Client

                Component
                    .For<IElasticClientFactory>()
                    .ImplementedBy<ElasticClientFactory>()
                    .LifestyleSingleton(),

                Component
                    .For<ElasticClient>()  // ElasticClient is thread safe by the way
                    .UsingFactory((IElasticClientFactory f) => f.Create()),

                Component
                    .For<IEsClient>()
                    .ImplementedBy<EsClient>()
                    .LifestyleSingleton(),

                Component
                    .For<JsonConverter>()
                    .ImplementedBy<EsApiFriendlyGuidConverter>()
                    .LifestyleSingleton(),

                // Repositories

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
                    .LifestyleTransient(),

                Component
                    .For(typeof(EsRequestConfiguration<,>))
                    .ImplementedBy(typeof(EsRequestConfiguration<,>))
                    .LifestyleTransient()
            );
        }
    }
}
