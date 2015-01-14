namespace ElasticAnalytics.Configuration.Windsor.ScAdapter
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ElasticAnalytics.ScAdapter.FacetModelSerialization;
    using ElasticAnalytics.ScAdapter.Requests;
    using ElasticAnalytics.Service.Types;
    using ElasticAnalytics.SessionRepository.Types.Configuration;
    using ElasticAnalytics.Utils.Configuration;

    public class ScAdapterInProcessInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(

                // Common services
                Component
                        .For<ISystemContext>()
                        .ImplementedBy<SystemContext>()
                        .LifestyleSingleton(),

                Component
                    .For<ISettingsProvider>()
                    .Instance(
                        new CachingSettingsProvider(
                            new CompositeSettingsProvider(
                                new DotNetConfigSettingsProvider()))), // TODO: add sitecore settings provider

                // Contact Serialization
                Classes.FromAssemblyInDirectory(new AssemblyFilter(".", "ElasticAnalytics.*"))
                    .BasedOn<IModelMemberSerializer>()
                    .WithServiceSelect((type, types) => new[] { typeof(IModelMemberSerializer) })
                    .Configure(c => c.LifestyleSingleton()),

                Component
                    .For<IElementJsonConverter>()
                    .ImplementedBy<ElementSerializer>()
                    .LifestyleSingleton()
               );
        }
    }
}
