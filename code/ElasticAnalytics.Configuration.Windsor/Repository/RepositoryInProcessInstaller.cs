namespace ElasticAnalytics.Configuration.Windsor.Repository
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ElasticAnalytics.Container.Windsor;
    using ElasticAnalytics.Model.Contact;
    using ElasticAnalytics.Model.Locking;
    using ElasticAnalytics.Repository.Elasticsearch.Client;
    using ElasticAnalytics.Repository.Elasticsearch.PersistenceModel;
    using ElasticAnalytics.Repository.Elasticsearch.QueryContext;
    using ElasticAnalytics.Repository.Elasticsearch.Repositories;
    using ElasticAnalytics.Repository.Elasticsearch.Serialization;
    using ElasticAnalytics.Repository.Types;
    using ElasticAnalytics.Repository.Types.Repositories;
    using ElasticAnalytics.Utils;

    using Nest;

    using Newtonsoft.Json;

    public class RepositoryInProcessInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            // Indices
            var contactStorageConfig = 
                Component
                    .For<IRequestConfiguration>()
                    .Instance(
                        new StandardRequestConfiguration(
                            ElasticAnalyticsSettings.EsStorage.ContactIndexName, 
                            ElasticAnalyticsSettings.EsStorage.ContactTypeName))
                    .LifestyleSingleton()
                    .Named(ElasticAnalyticsWindsorSettings.Configuration.ContactStorageSettingsKey);

            var contactLeaseStorageConfig =
                Component
                    .For<IRequestConfiguration>()
                    .Instance(
                        new StandardRequestConfiguration(
                            ElasticAnalyticsSettings.EsStorage.ContactIndexName, 
                            ElasticAnalyticsSettings.EsStorage.LeaseTypeName))
                    .LifestyleSingleton()
                    .Named(ElasticAnalyticsWindsorSettings.Configuration.ContactLeaseStorageSettingsKey);

            var contactIdentifierStorageConfig =
                Component
                    .For<IRequestConfiguration>()
                    .Instance(
                        new StandardRequestConfiguration(
                            ElasticAnalyticsSettings.EsStorage.ContactIndexName, 
                            ElasticAnalyticsSettings.EsStorage.IdentityMapTypeName))
                    .LifestyleSingleton()
                    .Named(ElasticAnalyticsWindsorSettings.Configuration.ContactIdentityMapSettingsKey);

            container.Register(

                // ES client
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

                // ------ Contact -----------

                contactStorageConfig,
                contactLeaseStorageConfig, 
                contactIdentifierStorageConfig,

                Component
                    .For<IRepository<ElasticContact>>()
                    .ImplementedBy<EsRepository<ElasticContact, EsContact>>()
                    .DependsOn(
                        Dependency.OnComponent(typeof(IRequestConfiguration), ElasticAnalyticsWindsorSettings.Configuration.ContactStorageSettingsKey))
                    .LifestyleTransient(),

                Component
                    .For<IConcurrencyControlRepository<ElasticLease>>()
                    .ImplementedBy<EsConcurrencyControlRepository<ElasticLease, EsLease>>()
                    .DependsOn(Dependency.OnComponent(typeof(IRequestConfiguration), ElasticAnalyticsWindsorSettings.Configuration.ContactLeaseStorageSettingsKey))
                    .LifestyleTransient(),

                Component
                    .For<IRepository<ElasticContactIdentityMap>>()
                    .ImplementedBy<EsRepository<ElasticContactIdentityMap, EsContactIdentityMap>>()
                    .DependsOn(Dependency.OnComponent(typeof(IRequestConfiguration), ElasticAnalyticsWindsorSettings.Configuration.ContactIdentityMapSettingsKey))
                    .LifestyleTransient()
            );
        }
    }
}
