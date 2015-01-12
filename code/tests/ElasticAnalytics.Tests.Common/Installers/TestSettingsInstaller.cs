namespace ElasticAnalytics.Tests.Common.Installers
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ElasticAnalytics.Model.Configuration;
    using ElasticAnalytics.SessionRepository.Types.Configuration;
    using ElasticAnalytics.Utils.Configuration;

    using ElasticAnalyticsSettings = ElasticAnalytics.Utils.ElasticAnalyticsSettings;

    public class TestSettingsInstaller : IWindsorInstaller
    {
        private readonly string systemKey;

        private readonly string instanceKey;

        public TestSettingsInstaller(string systemKey, string instanceKey)
        {
            this.systemKey = systemKey;
            this.instanceKey = instanceKey;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var settings = new InMemorySettingsProvider();
            settings.AddValue(ElasticAnalyticsSettings.SettingsKeys.SystemKey, "test_" + this.systemKey);
            settings.AddValue(ElasticAnalyticsSettings.SettingsKeys.InstanceKeyKey, this.instanceKey);

            container.Register( // only use windsor when relying on production installers - either directly or in dependency chain from a windsor'd root.
                Component.For<ISettingsProvider>()
                    .Instance(
                        new CompositeSettingsProvider(
                            new DotNetConfigSettingsProvider(),
                            settings))
                    .Named("testInMemory")
                    .IsDefault());
        }
    }
}