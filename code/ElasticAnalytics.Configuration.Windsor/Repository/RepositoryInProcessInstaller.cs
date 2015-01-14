namespace ElasticAnalytics.Configuration.Windsor.Repository
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ElasticAnalytics.Repository.Elasticsearch;
    using ElasticAnalytics.Repository.Elasticsearch.Client;
    using ElasticAnalytics.Repository.Elasticsearch.Serialization;

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

                // Request Configurations (that specify where documents are stored)

                // Custom configs

                Classes.FromAssemblyInThisApplication()
                       .Pick()
                       .If(t => t.BaseType.IsGenericType && 
                            t.BaseType.GetGenericTypeDefinition().UnderlyingSystemType == typeof(EsRequestConfiguration<,>))
                       .WithServiceSelect((type, types) => new [] { type.BaseType })
                       .Configure(c => c.LifestyleSingleton()),

               // Default configuration (choose an index and type name based on the repo model type name)

               Component
                   .For(typeof(EsRequestConfiguration<,>))
                   .ImplementedBy(typeof(EsRequestConfiguration<,>))
                   .LifestyleTransient()
            );
        }
    }
}
