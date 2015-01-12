namespace ElasticAnalytics.Repository.Elasticsearch.Tests.Integration.Repositories
{
    using System;

    using Castle.Windsor;

    using ElasticAnalytics.Configuration.Windsor.Common;
    using ElasticAnalytics.Configuration.Windsor.Repository;
    using ElasticAnalytics.Model.Locking;
    using ElasticAnalytics.Repository.Types;
    using ElasticAnalytics.Repository.Types.Repositories;
    using ElasticAnalytics.Service.Types;
    using ElasticAnalytics.Tests.Common;
    using ElasticAnalytics.Tests.Common.Customizations;
    using ElasticAnalytics.Tests.Common.EsUtils;
    using ElasticAnalytics.Tests.Common.Installers;
    using ElasticAnalytics.Tests.Customizations.Customizations;

    using FluentAssertions;

    using Ploeh.AutoFixture;

    using Treevs.Essentials.AutoFixture.Xunit.AutoSetup;

    using Xunit.Extensions;

    public class ConcurrencyControlRepositoryFixture
    {
        public static Action<IFixture> AutoSetup()
        {
            return (f) =>
                {
                    // its a full integration test, so install everything as we do in production.
                    var container = new WindsorContainer().Install(
                            new ContainerFeaturesDefaultInstaller(),
                            new TypeMapperDefaultInstaller(),
                            new RepositoryMappingDefaultInstaller(),
                            new RepositoryInProcessInstaller(),
                            new TestSettingsInstaller("EsLeaseRepositoryFixture", f.Create<string>()));

                    f.Customize(new WindsorAdapterCustomization(container)) // use the production Windsor installers
                        .Customize(new ElasticContactCustomization()) // create meaningful contact objects
                        .Customize(new IRequestContextCustomization("EsLeaseRepositoryFixture", f.Create<string>()))
                        // To create a unique context for each test
                        .Customize(new TestIndicesUtilCustomization(container.Resolve<IEsRequestConfiguration>("ContactStorage")));
                        // To create and delete and index for each test

                    f.Freeze<ISystemContext>();
                    // f.Freeze<ILeaseRepository>();
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
        public void Create_NoLeaseExists_LeaseCreated(
            IConcurrencyControlRepository<ElasticLease> repo,
            ElasticLease lease,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                var actual = repo.Create(lease, ctx);
                actual.Should().BeTrue();
            }
        }

        [Theory]
        [AutoSetup("ActiveLease")]
        public void Create_LeaseAlreadyExists_FalseReturned(
            IConcurrencyControlRepository<ElasticLease> repo,
            ElasticLease lease,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                repo.Create(lease, ctx);
                var actual = repo.Create(lease, ctx);
                actual.Should().BeFalse();
            }
        }

        [Theory]
        [AutoSetup]
        public void Get_NoLeaseExists_NoLeaseIsReturned(
            IConcurrencyControlRepository<ElasticLease> repo,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                var actual = repo.Get(new Guid(), ctx);
                actual.Should().BeNull();
            }
        }

        [Theory]
        [AutoSetup("ActiveLease")]
        public void Get_LeaseAlreadyExists_LeaseIsReturned(
            IConcurrencyControlRepository<ElasticLease> repo,
            ElasticLease lease,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                repo.Create(lease, ctx);
                var actual = repo.Get(lease.ResourceId, ctx);
                actual.Should().NotBeNull();
                actual.ShouldBeEquivalentTo(lease);
            }
        }

        [Theory]
        [AutoSetup]
        public void Delete_NoLeaseExists_TrueIsReturned(
            IConcurrencyControlRepository<ElasticLease> repo,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                repo.Delete(new Guid(), ctx);
            }
        }

        [Theory]
        [AutoSetup("ActiveLease")]
        public void Delete_LeaseExists_LeaseDeleted(
            IConcurrencyControlRepository<ElasticLease> repo,
            ElasticLease lease,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                repo.Create(lease, ctx);
                repo.Delete(lease.ResourceId, ctx);
                repo.Get(lease.ResourceId, ctx).Should().BeNull();
            }
        }

        [Theory]
        [AutoSetup("ActiveLease")]
        public void Extend_NoLeaseExists_LeaseCreatedAndSuccessful(
            IConcurrencyControlRepository<ElasticLease> repo,
            ElasticLease lease,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                var actual = repo.SaveOptimistically(lease, ctx);
                actual.Should().BeTrue();
            }
        }

        [Theory]
        [AutoSetup("InActiveLease")]
        public void Extend_InactiveLeaseExistsButLeaseGrabbedByDifferentOwner_LeaseNotExtended(
            IConcurrencyControlRepository<ElasticLease> repo,
            ElasticLease lease,
            ISystemContext ctx,
            TestIndexUtils contactIndex,
            IFixture fixture)
        {
            using (contactIndex)
            {
                // expired lease exists with owner1.
                repo.Create(lease, ctx);

                // simulate owner2 taking the lease...
                var changedLease = new ElasticLease(
                    lease.ResourceId,
                    DateTime.UtcNow + TimeSpan.FromHours(5),
                    fixture.Create<ElasticLeaseOwner>(),
                    lease.Version);
                repo.SaveOptimistically(changedLease, ctx);

                // now owner 1 tries to extend what it thinks is it's lease...
                var actual = repo.SaveOptimistically(lease, ctx);
                actual.Should().BeFalse();
            }
        }

        [Theory]
        [AutoSetup("InActiveLease")]
        public void Extend_InactiveLeaseExistsThenLeaseGrabbedBySameOwner_LeaseNotExtended(
            IConcurrencyControlRepository<ElasticLease> repo,
            ElasticLease lease,
            ISystemContext ctx,
            TestIndexUtils contactIndex,
            IFixture fixture)
        {
            using (contactIndex)
            {
                // expired lease exists with owner1.
                repo.Create(lease, ctx);

                // simulate owner1 (in a different thread/process I guess) taking the lease...
                var changedLease = new ElasticLease(
                    lease.ResourceId,
                    DateTime.UtcNow + TimeSpan.FromHours(5),
                    lease.Owner,
                    lease.Version);
                repo.SaveOptimistically(changedLease, ctx); // version will get incremented

                // now owner 1 tries to extend what it thinks is it's lease, this should eventually be allowed...
                var actual = repo.SaveOptimistically(lease, ctx);
                actual.Should().BeFalse();
            }
        }

        [Theory]
        [AutoSetup("ActiveLease")]
        public void Extend_LeaseExistsVersionNotChanged_LeaseExtended(
            IConcurrencyControlRepository<ElasticLease> repo,
            ElasticLease lease,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                repo.Create(lease, ctx);
                var extendedLease = new ElasticLease(
                    lease.ResourceId,
                    lease.Expires + TimeSpan.FromHours(2),
                    lease.Owner,
                    lease.Version);
                var actual = repo.SaveOptimistically(extendedLease, ctx);
                actual.Should().BeTrue();
            }
        }
    }
}
