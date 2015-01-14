namespace ElasticAnalytics.Tests.Common.Customizations
{
    using ElasticAnalytics.Service.Types;
    using ElasticAnalytics.SessionRepository.Types.Configuration;
    using ElasticAnalytics.Utils;

    using Moq;

    using Ploeh.AutoFixture;

    public class SystemContextCustomization : ICustomization
    {
        private ISettingsProvider settings;

        public SystemContextCustomization(ISettingsProvider settings = null)
        {
            this.settings = settings;
        }

        public void Customize(IFixture fixture)
        {
            if (settings == null)
            {
                this.settings = fixture.Create<ISettingsProvider>();
            }
            
            fixture.Register(() =>
                        Mock.Of<ISystemContext>(c =>
                            c.InstanceKey == this.settings.Value(ElasticAnalyticsSettings.SettingsKeys.InstanceKeyKey) &&
                            c.SystemPrefix == this.settings.Value(ElasticAnalyticsSettings.SettingsKeys.SystemKey)));

            fixture.Freeze<ISystemContext>();
        }
    }
}
