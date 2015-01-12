namespace ElasticAnalytics.Configuration.Windsor.Common
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ElasticAnalytics.Utils.AutoMapper;

    public class TypeMapperDefaultInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component
                    .For<ITypeMapper>()
                    .ImplementedBy<AutoMapperTypeMapper>()
                    .LifestyleSingleton()
                    .Named("AutoMapperTypeMapper")
            );
        }
    }
}
