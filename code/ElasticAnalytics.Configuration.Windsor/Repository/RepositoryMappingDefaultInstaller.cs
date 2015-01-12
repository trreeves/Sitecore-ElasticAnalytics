namespace ElasticAnalytics.Configuration.Windsor.Repository
{
    using AutoMapper;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ElasticAnalytics.Repository.Elasticsearch.Mapping;

    public class RepositoryMappingDefaultInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            // Type Mapping (Convention based)
            container.Register(
                Classes.FromAssemblyInThisApplication()
                    .InSameNamespaceAs<ContactRepoModelMappingProfile>()
                    .If(t => t.BaseType == typeof(Profile))
                    .WithServiceSelect((type, types) => new[] { typeof(Profile) })
                    .Configure(c => c.LifestyleSingleton()));
        }
    }
}
