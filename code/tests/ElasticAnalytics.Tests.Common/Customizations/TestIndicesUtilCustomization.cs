namespace ElasticAnalytics.Tests.Common.Customizations
{
    using ElasticAnalytics.Repository.Types;
    using ElasticAnalytics.Service.Types;
    using ElasticAnalytics.SessionRepository.Types.Configuration;
    using ElasticAnalytics.Tests.Common.EsUtils;

    using Ploeh.AutoFixture;

    public class TestIndicesUtilCustomization : ICustomization
    {
        private readonly IRequestConfiguration requestConfig;
        private readonly ISettingsProvider settings;

        public TestIndicesUtilCustomization(
            IRequestConfiguration requestConfig,
            ISettingsProvider settings = null)
        {
            this.settings = settings;
            this.requestConfig = requestConfig;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Register(() => new TestIndexUtils(
                this.settings?? fixture.Create<ISettingsProvider>(), 
                fixture.Create<ISystemContext>(), 
                this.requestConfig));
        }
    }
}