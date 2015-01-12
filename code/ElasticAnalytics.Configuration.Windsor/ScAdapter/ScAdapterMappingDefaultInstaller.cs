namespace ElasticAnalytics.Configuration.Windsor.ScAdapter
{
    using AutoMapper;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ElasticAnalytics.ScAdapter.Mapping;

    public class ScAdapterMappingDefaultInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            // Type Mapping (Convention based)
            container.Register(
                Classes.FromAssemblyInThisApplication()
                    .InSameNamespaceAs<ContactModelMappingProfile>()
                    .If(t => t.BaseType == typeof(Profile))
                    .WithServiceSelect((type, types) => new[] { typeof(Profile) })
                    .Configure(c => c.LifestyleSingleton()));
        }
    }
}
