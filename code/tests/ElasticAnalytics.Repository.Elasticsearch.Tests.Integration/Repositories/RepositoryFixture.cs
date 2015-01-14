namespace ElasticAnalytics.Repository.Elasticsearch.Tests.Integration.Repositories
{
    using System;

    using Castle.Windsor;

    using ElasticAnalytics.Configuration.Windsor.Common;
    using ElasticAnalytics.Configuration.Windsor.Repository;
    using ElasticAnalytics.Model.Contact;
    using ElasticAnalytics.Repository.Elasticsearch.PersistenceModel;
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

    public class RepositoryFixture
    {
        public static Action<IFixture> AutoSetup()
        {
            return (f) =>
                {
                    const string systemKey = "EsContactRepositoryFixture";
                    var instanceKey = f.Create<string>();

                    // its a full integration test, so install everything as we do in production.
                    var container = new WindsorContainer()
                        .Install(
                            new ContainerFeaturesDefaultInstaller(),
                            new TypeMapperDefaultInstaller(),
                            new RepositoryMappingDefaultInstaller(),
                            new RepositoryInProcessInstaller(),
                            new TestSettingsInstaller(systemKey, instanceKey));

                    f.Customize(new WindsorAdapterCustomization(container)) // use the production Windsor installers
                        .Customize(new ElasticContactCustomization()) // create meaningful contact objects
                        .Customize(new SystemContextCustomization()) // To create a unique context for each test
                        .Customize(
                            new TestIndicesUtilCustomization(
                                container.Resolve<EsRequestConfiguration<ElasticContact, EsContact>>()));
                };
        }

        [Theory]
        [AutoSetup]
        public void GetContact_ContactExists_ContactReturned(
            TestIndexUtils contactIndex,
            IRepository<ElasticContact> repo,
            ElasticContact contact,
            ISystemContext ctx)
        {
            using (contactIndex)
            {
                repo.Save(contact, ctx);

                var res = repo.Get(contact.Id, ctx);
                res.Should().NotBeNull();
                res.Id.Should().Be(contact.Id);
            }
        }

        [Theory]
        [AutoSetup]
        public void GetContact_ContactDoesntExist_ContactNotReturned(
            TestIndexUtils contactIndex,
            IRepository<ElasticContact> repo,
            ElasticContact contact,
            ISystemContext ctx)
        {
            using (contactIndex)
            {
                // a little hacky - this is an easy way of ensuring the test index exists first.
                repo.Save(contact, ctx);

                var res = repo.Get(new Guid(), ctx);
                res.Should().BeNull();
            }
        }

        [Theory]
        [AutoSetup]
        public void DeleteContact_ContactExistsWithNoSuccessors_ContactDeleted(
            TestIndexUtils contactIndex,
            IRepository<ElasticContact> repo,
            ElasticContact contact,
            ISystemContext ctx)
        {
            using (contactIndex)
            {
                repo.Save(contact, ctx);

                repo.Delete(contact.Id, ctx);

                repo.Get(contact.Id, ctx).Should().BeNull();
            }
        }
    }
}
