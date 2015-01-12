namespace ElasticAnalytics.Tests.Common.Installers
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ElasticAnalytics.Tests.Common.Utils;
    using ElasticAnalytics.Utils.DateTime;

    public class TimeTravelInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IDateTimeController>()
                    .ImplementedBy<DateTimeTimeTraveller>()
                    .Named("timeTraveller")
                    .LifestyleSingleton()
                    .IsDefault());
        }
    }
}
