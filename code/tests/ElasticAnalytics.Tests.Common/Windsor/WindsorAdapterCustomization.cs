namespace ElasticAnalytics.Tests.Common
{
    using Castle.Windsor;

    using Ploeh.AutoFixture;

    public class WindsorAdapterCustomization : ICustomization
    {
        private readonly IWindsorContainer _container;

        public WindsorAdapterCustomization(IWindsorContainer container = null)
        {
            this._container = container ?? new WindsorContainer();
        }

        public void Customize(IFixture fixture)
        {
            fixture.Customizations.Add(new WindsorAdapter(this._container));
        }
    }
}