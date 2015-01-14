namespace ElasticAnalytics.Configuration.Windsor.Service
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ElasticAnalytics.Service.Services;
    using ElasticAnalytics.Service.Types;
    using ElasticAnalytics.Service.Types.Services;
    using ElasticAnalytics.Utils.DateTime;

    public class ServiceInProcessInstaller : IWindsorInstaller
    {
            public void Install(IWindsorContainer container, IConfigurationStore store)
            {
                container.Register(
                    Component.For<IDateTimeController>()
                        .ImplementedBy<StandardDateTimeController>()
                        .LifestyleSingleton(),

                    Component
                        .For<ILeaseService>()
                        .ImplementedBy<LeaseService>()
                        .LifestyleSingleton(),

                    Component
                        .For<IContactIdentityMapService>()
                        .ImplementedBy<ContactIdentityMapService>()
                        .LifestyleSingleton(),
                        
                    Component
                        .For<IElasticAnalyticsContactService>()
                        .ImplementedBy<ContactService>()
                        .LifestyleSingleton(),
                            
                    Component
                        .For<IElasticAnalyticsInteractionService>()
                        .ImplementedBy<InteractionService>()
                        .LifestyleSingleton());
            }
    }
}