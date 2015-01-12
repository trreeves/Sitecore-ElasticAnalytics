namespace ElasticAnalytics.Configuration.Windsor.Service
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ElasticAnalytics.Model.Locking;
    using ElasticAnalytics.Repository.Types.Repositories;
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
                        .DependsOn(Dependency.OnComponent(typeof(IConcurrencyControlRepository<ElasticLease>), "ContactLeaserRepo"))
                        .Named("ContactLeaser")
                        .LifestyleSingleton(),

                    Component
                        .For<IContactIdentityMapService>()
                        .ImplementedBy<ContactIdentityMapService>()
                        .LifestyleSingleton(),
                        
                    Component
                            .For<IElasticAnalyticsContactService>()
                            .ImplementedBy<ContactService>()
                            .DependsOn(Dependency.OnComponent(typeof(ILeaseService), "ContactLeaser")) // magic string currently matched in repo installer
                            .LifestyleSingleton(),
                            
                    Component
                            .For<IElasticAnalyticsInteractionService>()
                            .ImplementedBy<InteractionService>()
                            .LifestyleSingleton());
            }
    }
}