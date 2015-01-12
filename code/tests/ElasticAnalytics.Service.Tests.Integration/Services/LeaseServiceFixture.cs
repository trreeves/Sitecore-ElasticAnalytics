namespace ElasticAnalytics.Service.Tests.Integration.Services
{
    using System;

    using Castle.Windsor;

    using ElasticAnalytics.Configuration.Windsor.Common;
    using ElasticAnalytics.Configuration.Windsor.Repository;
    using ElasticAnalytics.Configuration.Windsor.Service;
    using ElasticAnalytics.Model.Locking;
    using ElasticAnalytics.Repository.Types;
    using ElasticAnalytics.Service.Types;
    using ElasticAnalytics.Tests.Common;
    using ElasticAnalytics.Tests.Common.Customizations;
    using ElasticAnalytics.Tests.Common.EsUtils;
    using ElasticAnalytics.Tests.Common.Installers;
    using ElasticAnalytics.Tests.Customizations.Customizations;

    using FluentAssertions;

    using Ploeh.AutoFixture;
    using Ploeh.AutoFixture.AutoMoq;

    using Treevs.Essentials.AutoFixture.Xunit.AutoSetup;

    using Xunit.Extensions;

    public class LeaseServiceFixture
    {
        public static Action<IFixture> AutoSetup()
        {
            return (f) =>
            {
                const string systemKey = "ElasticAnalyticsContactServiceFixture";
                var instanceKey = f.Create<string>();

                // Setup Windsor as in production...
                var container = new WindsorContainer()
                    .Install(
                        new ContainerFeaturesDefaultInstaller(),
                        new TypeMapperDefaultInstaller(),

                        new RepositoryMappingDefaultInstaller(),
                        new RepositoryInProcessInstaller(),

                        new ServiceInProcessInstaller(),

                        new TestSettingsInstaller(systemKey, instanceKey));

                // Setup AutoFixture...
                f
                    // Objects resolved by Windsor first, then (Auto)Moq, then AutoFixture
                    .Customize(new AutoMoqCustomization())
                    .Customize(new WindsorAdapterCustomization(container))
                    .Customize(new ElasticContactCustomization()) // create meaningful contacts
                    .Customize(new IRequestContextCustomization(systemKey, instanceKey))
                    .Customize(new TestIndicesUtilCustomization(container.Resolve<IEsRequestConfiguration>("ContactStorage")));

                f.Freeze<ISystemContext>();
            };
        }

        public static Action<IFixture> ActiveLease()
        {
            return (f) => f.Customize(new ElasticLeaseCustomization(TimeSpan.FromHours(3)));
        }

        public static Action<IFixture> InActiveLease()
        {
            return (f) => f.Customize(new ElasticLeaseCustomization(TimeSpan.FromHours(-3)));
        }

        [Theory]
        [AutoSetup("ActiveLease")]
        public void TryObtainLock_NoLeaseExists_LockObtained(
            ILeaseService service,
            ISystemContext ctx,
            ElasticLease lease,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                ElasticLease winner;
                service.TryObtainLock(lease, ctx, out winner).Should().BeTrue();
                lease.ShouldBeEquivalentTo(winner);
            }
        }

        [Theory]
        [AutoSetup("InActiveLease")]
        public void TryObtainLock_InactiveLeaseExistsBySameOwner_LockObtained(
            ILeaseService service,
            ISystemContext ctx,
            ElasticLease inactiveLease,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                // write inactive lease
                service.TryObtainLock(inactiveLease, ctx, out inactiveLease).Should().BeTrue();

                var activeLease = new ElasticLease(
                    inactiveLease.ResourceId,
                    DateTime.UtcNow + TimeSpan.FromHours(1),
                    inactiveLease.Owner,
                    inactiveLease.Version);

                ElasticLease winner;
                service.TryObtainLock(activeLease, ctx, out winner).Should().BeTrue();
                activeLease.ShouldBeEquivalentTo(winner);
            }
        }

        [Theory]
        [AutoSetup("ActiveLease")]
        public void TryObtainLock_ActiveLeaseExistsByDifferentOwner_LockRefused(
            ILeaseService service,
            ISystemContext ctx,
            ElasticLease owner1ActiveLease,
            TestIndexUtils contactIndex,
            IFixture fixture)
        {
            using (contactIndex)
            {
                ElasticLease winner;
                service.TryObtainLock(owner1ActiveLease, ctx, out winner).Should().BeTrue();

                var owner2ActiveLease = new ElasticLease(
                    winner.ResourceId,
                    DateTime.UtcNow + TimeSpan.FromHours(1),
                    fixture.Create<ElasticLeaseOwner>(),
                    winner.Version);

                service.TryObtainLock(owner2ActiveLease, ctx, out winner).Should().BeFalse();
            }
        }

        [Theory]
        [AutoSetup("InActiveLease")]
        public void TryObtainLock_InactiveLeaseExistsByDifferentOwner_LockObtained(
            ILeaseService service,
            ISystemContext ctx,
            ElasticLease owner1InactiveLease,
            TestIndexUtils contactIndex,
            IFixture fixture)
        {
            using (contactIndex)
            {
                service.TryObtainLock(owner1InactiveLease, ctx, out owner1InactiveLease).Should().BeTrue();

                var owner2ActiveLease = new ElasticLease(
                    owner1InactiveLease.ResourceId,
                    DateTime.UtcNow + TimeSpan.FromHours(2),
                    fixture.Create<ElasticLeaseOwner>(),
                    owner1InactiveLease.Version);

                ElasticLease winner;
                service.TryObtainLock(owner2ActiveLease, ctx, out winner).Should().BeTrue();
                owner2ActiveLease.ShouldBeEquivalentTo(winner);
            }
        }

        [Theory]
        [AutoSetup("ActiveLease")]
        public void TryObtainLock_ActiveLeaseExistsBySameOwner_LockObtained(
            ILeaseService service,
            ISystemContext ctx,
            ElasticLease activeLease,
            TestIndexUtils contactIndex,
            IFixture fixture
            )
        {
            using (contactIndex)
            {
                service.TryObtainLock(activeLease, ctx, out activeLease);

                var activeLease2 = new ElasticLease(
                    activeLease.ResourceId,
                    DateTime.UtcNow + TimeSpan.FromHours(2),
                    activeLease.Owner,
                    activeLease.Version);

                ElasticLease winner;
                service.TryObtainLock(activeLease2, ctx, out winner);
                activeLease2.ShouldBeEquivalentTo(winner);
            }
        }
    }
}