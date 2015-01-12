namespace ElasticAnalytics.Tests.Customizations.Customizations
{
    using System;

    using ElasticAnalytics.Model.Locking;

    using Ploeh.AutoFixture;

    public class ElasticLeaseCustomization : ICustomization
    {
        private readonly TimeSpan expiresIn;

        public ElasticLeaseCustomization(TimeSpan expiresIn)
        {
            this.expiresIn = expiresIn;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Register(() =>
                    new ElasticLease(
                        fixture.Create<Guid>(), 
                        DateTime.UtcNow + this.expiresIn, 
                        fixture.Create<ElasticLeaseOwner>()));
        }
    }
}