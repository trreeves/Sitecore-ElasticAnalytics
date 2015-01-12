namespace ElasticAnalytics.Repository.Elasticsearch.Tests.Unit.Repositories
{
    using System;

    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    using ElasticAnalytics.Configuration.Windsor.Common;
    using ElasticAnalytics.Configuration.Windsor.Repository;
    using ElasticAnalytics.Model.Contact;
    using ElasticAnalytics.Repository.Elasticsearch.Client;
    using ElasticAnalytics.Repository.Elasticsearch.Extensions;
    using ElasticAnalytics.Repository.Elasticsearch.PersistenceModel;
    using ElasticAnalytics.Repository.Elasticsearch.QueryContext;
    using ElasticAnalytics.Repository.Types.Repositories;
    using ElasticAnalytics.Service.Types;
    using ElasticAnalytics.Tests.Common;

    using FluentAssertions;

    using Moq;

    using Nest;

    using Ploeh.AutoFixture;
    using Ploeh.AutoFixture.AutoMoq;

    using Treevs.Essentials.AutoFixture.Xunit.AutoSetup;

    using Xunit.Extensions;

    public class RepositoryFixture
    {
        private static readonly Guid TestContactId = new Guid();

        public static Action<IFixture> AutoSetup()
        {
            return (f) =>
                {
                    // Setup Windsor...
                    var container = new WindsorContainer();
                    container.Install(
                        new ContainerFeaturesDefaultInstaller(),
                        new TypeMapperDefaultInstaller(),
                        new RepositoryMappingDefaultInstaller(),
                        new RepositoryInProcessInstaller());

                    // mock out ES client 
                    container.Register(
                        Component.For<IEsClient>()
                            .Instance(new Mock<IEsClient>().Object)
                            .LifestyleSingleton()
                            .IsDefault());

                    // setup AutoFixture...
                    f
                        // Objects resolved by Windsor first, then (Auto)Moq, then AutoFixture
                        .Customize(new WindsorAdapterCustomization(container))
                        .Customize(new AutoMoqCustomization());


                    // Common Tests data setup...
                    var contact = f.Freeze<EsContact>();
                    contact._id = TestContactId.ToEsSafeId();

                    f.Freeze<ISystemContext>();
                };
        }

        public static Action<IFixture> ContactExists()
        {
            return (f) =>
                {
                    var contact = f.Freeze<EsContact>();
                    string id = contact._id.ToString();

                    var ctx = f.Create<ISystemContext>();

                    // put in a customization at some point...
                    var response =
                        Mock.Of<IGetResponse<EsContact>>(r => 
                            r.Found == true && 
                            r.IsValid == true && 
                            r.Id == id && 
                            r.Source == contact);

                    Mock.Get(f.Freeze<IEsClient>())
                        .Setup(c => c.Get<EsContact>(It.IsAny<IQueryContext>(), TestContactId.ToString(), ctx))
                        .Returns(response);
            };
        }

        public static Action<IFixture> ContactDoesntExist()
        {
            return (f) =>
            {
                var response =
                    Mock.Of<IGetResponse<EsContact>>(r =>
                            r.Found == true &&
                            r.IsValid == true);

                var ctx = f.Create<ISystemContext>();

                Mock.Get(f.Freeze<IEsClient>())
                    .Setup(c => c.Get<EsContact>(It.IsAny<IQueryContext>(), TestContactId.ToString(), ctx))
                    .Returns(response);
            };
        }

        public static Action<IFixture> ContactSuccessfullyDeletes()
        {
            return (f) =>
                {
                    var response = Mock.Of<IDeleteResponse>(r => r.Found == true && r.IsValid == true);
                    var ctx = f.Create<ISystemContext>();

                    Mock.Get(f.Freeze<IEsClient>())
                        .Setup(c => c.Delete<EsContact>(It.IsAny<IQueryContext>(), TestContactId.ToString(), ctx, -1))
                        .Returns(response);
                };
        }

        [Theory]
        [AutoSetup("ContactExists")]
        public void GetContact_ContactExists_ContactReturned(
            IRepository<ElasticContact> repo,
            ISystemContext ctx)
        {
            var res = repo.Get(TestContactId, ctx);
            res.Should().NotBeNull();
            res.Id.Should().Be(TestContactId);
        }

        [Theory]
        [AutoSetup("ContactDoesntExist")]
        public void GetContact_ContactDoesntExist_NullReturned(
            IRepository<ElasticContact> repo,
            ISystemContext ctx)
        {
            repo.Get(new Guid(), ctx).Should().BeNull();
        }

        [Theory]
        [AutoSetup("ContactSuccessfullyDeletes")]
        public void DeleteContact_ContactExists_ContactDeleted(
            IRepository<ElasticContact> repo,
            ISystemContext ctx)
        {
            repo.Delete(TestContactId, ctx);
        }

        // GetLatestContact with no successor

        // GetLatestContact with successor

        // GetLatestContact with successor that doesn't exist

        // savecontact when contact doesn't exist

        // savecontact when contact already exists
    }
}
