namespace ElasticAnalytics.Service.Tests.Unit.Services
{
    using System;

    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    using ElasticAnalytics.Configuration.Windsor.Service;
    using ElasticAnalytics.Model.Contact;
    using ElasticAnalytics.Model.Locking;
    using ElasticAnalytics.Repository.Types.Repositories;
    using ElasticAnalytics.Service.Types;
    using ElasticAnalytics.Service.Types.Services;
    using ElasticAnalytics.Tests.Common;
    using ElasticAnalytics.Tests.Common.Customizations;
    using ElasticAnalytics.Tests.Customizations.Customizations;

    using FluentAssertions;

    using Moq;

    using Ploeh.AutoFixture;
    using Ploeh.AutoFixture.AutoMoq;

    using Treevs.Essentials.AutoFixture.Xunit.AutoSetup;

    using Xunit.Extensions;

    public class ContactServiceFixture
    {
        public static Action<IFixture> AutoSetup()
        {
            return (f) =>
                {
                    // Setup Windsor as in production...
                    var container = new WindsorContainer()
                        .Install(new ServiceInProcessInstaller());

                    // Setup AutoFixture...
                    f
                        // Objects resolved by Windsor first, then (Auto)Moq, then AutoFixture
                        .Customize(new AutoMoqCustomization())
                        .Customize(new WindsorAdapterCustomization(container))
                        // create meaningful contacts
                        .Customize(new ElasticContactCustomization(IdentificationLevel.Known))
                        .Customize(new SystemContextCustomization());

                    var contact = f.Freeze<ElasticContact>();

                    // override Repos with mocks...
                    container.Register(
                        Component.For<IRepository<ElasticContact>>()
                            .Instance(new Mock<IRepository<ElasticContact>>().Object)
                            .Named("mockedContactRepo")
                            .IsDefault()
                            .LifestyleSingleton(),
                        Component.For<IRepository<ElasticContactIdentityMap>>()
                            .Instance(new Mock<IRepository<ElasticContactIdentityMap>>().Object)
                            .Named("mockedContactIdentityRepo")
                            .IsDefault()
                            .LifestyleSingleton(),
                        Component.For<IConcurrencyControlRepository<ElasticLease>>()
                            .Instance(new Mock<IConcurrencyControlRepository<ElasticLease>>().Object)
                            .Named("ContactLeaserRepo")
                            .IsDefault()
                            .LifestyleSingleton());
                };
        }

        #region Setup - Contact

        public static Action<IFixture> ContactExists()
        {
            return (f) =>
                {
                    var contact = f.Create<ElasticContact>();
                    Mock.Get(f.Create<IRepository<ElasticContact>>())
                        .Setup(r => r.Get(contact.Id, It.IsAny<ISystemContext>()))
                        .Returns(contact);
                };
        }

        public static Action<IFixture> LatestContactExists()
        {
            return
                (f) =>
                    {
                        var contact = f.Create<ElasticContact>();
                        contact.Successor = null;
                        Mock.Get(f.Create<IRepository<ElasticContact>>())
                            .Setup(r => r.Get(contact.Id, It.IsAny<ISystemContext>()))
                            .Returns(contact);
                    };
        }

        public static Action<IFixture> ContactDoesntExist()
        {
            return (f) =>
                    Mock.Get(f.Create<IRepository<ElasticContact>>())
                        .Setup(r => r.Get(It.IsAny<Guid>(), It.IsAny<ISystemContext>()))
                        .Returns<ElasticContact>(null);
        }

        #endregion

        #region Setup - Locking

        public static Action<IFixture> ContactNotLocked()
        {
            return (f) =>
                {
                    var contact = f.Create<ElasticContact>();
                    var leaseRepo = Mock.Get(f.Create<IConcurrencyControlRepository<ElasticLease>>());
                    leaseRepo.Setup(r => r.Get(contact.Id, It.IsAny<ISystemContext>()))
                        .Returns((ElasticLease) null);
                    leaseRepo.Setup(r => r.Create(It.IsAny<ElasticLease>(), It.IsAny<ISystemContext>())).Returns(true);
                };
        }

        public static Action<IFixture> ContactLockedByCurrentOwner()
        {
            return (f) =>
            {
                var contact = f.Create<ElasticContact>();
                var owner = f.Freeze<ElasticLeaseOwner>();
                var leaseRepo = Mock.Get(f.Create<IConcurrencyControlRepository<ElasticLease>>());
                leaseRepo.Setup(r => r.Get(contact.Id, It.IsAny<ISystemContext>()))
                    .Returns(new ElasticLease(contact.Id, DateTime.UtcNow + TimeSpan.FromHours(1), owner));
                leaseRepo.Setup(r => r.SaveOptimistically(It.IsAny<ElasticLease>(), It.IsAny<ISystemContext>())).Returns(true);
            };
        }

        public static Action<IFixture> ContactLockedByDifferentOwner()
        {
            return (f) =>
            {
                var contact = f.Create<ElasticContact>();
                var owner = f.Create<ElasticLeaseOwner>();
                Mock.Get(f.Create<IConcurrencyControlRepository<ElasticLease>>())
                    .Setup(r => r.Get(contact.Id, It.IsAny<ISystemContext>()))
                    .Returns(new ElasticLease(contact.Id, DateTime.UtcNow + TimeSpan.FromHours(1), owner));
            };
        }

        #endregion

        #region Setup - Delete Contact

        public static Action<IFixture> DeleteContactSuccessfully()
        {
            return
                (f) =>
                {
                    var contact = f.Create<ElasticContact>();

                    Mock.Get(f.Create<IRepository<ElasticContact>>())
                        .Setup(r => r.Delete(contact.Id, It.IsAny<ISystemContext>()));
                };
        }

        #endregion

        #region LoadContact

        [Theory]
        [AutoSetup("LatestContactExists", "ContactNotLocked")]
        public void TryLoadContact_ContactExists_ReturnsContact(
            IElasticAnalyticsContactService contactService,
            ElasticLeaseOwner us,
            ElasticContact eContact,
            ISystemContext ctx)
        {
            var res = contactService.TryLoadAndLock(eContact.Id, us, TimeSpan.FromMinutes(1), ctx);
            res.Should().NotBeNull();
            res.LockedObject.Id.Should().Be(eContact.Id);
            res.Status.Should().Be(LockAttemptStatus.Success);
        }

        [Theory]
        [AutoSetup("LatestContactExists", "ContactLockedByCurrentOwner")]
        public void TryLoadContact_ContactExistsAlreadyLockedBySameOwner_ReturnsLocked(
            IElasticAnalyticsContactService contactService,
            ElasticLeaseOwner us,
            ElasticContact eContact,
            ISystemContext ctx)
        {
            var res = contactService.TryLoadAndLock(eContact.Id, us, TimeSpan.FromMinutes(1), ctx);
            res.Should().NotBeNull();
            res.LockedObject.Id.Should().Be(eContact.Id);
            res.Status.Should().Be(LockAttemptStatus.Success);
        }

        [Theory]
        [AutoSetup("LatestContactExists", "ContactLockedByDifferentOwner")]
        public void TryLoadContact_ContactExistsAlreadyLockedByDifferentOwner_NotLocked(
            IElasticAnalyticsContactService contactService,
            ElasticLeaseOwner us,
            ElasticContact eContact,
            ISystemContext ctx)
        {
            var res = contactService.TryLoadAndLock(eContact.Id, us, TimeSpan.FromMinutes(1), ctx);
            res.Should().NotBeNull();
            res.LockedObject.Id.Should().Be(eContact.Id);
            res.Status.Should().Be(LockAttemptStatus.AlreadyLocked);
        }

        [Theory]
        [AutoSetup("ContactDoesntExist")]
        public void TryLoadContact_ContactDoesntExist_ReturnsNull(
            ElasticLeaseOwner us,
            IElasticAnalyticsContactService contactService,
            ISystemContext ctx)
        {
            var res = contactService.TryLoadAndLock(Guid.NewGuid(), us, TimeSpan.FromMinutes(1), ctx);
            res.LockedObject.Should().BeNull();
            res.Status.Should().Be(LockAttemptStatus.NotFound);
        }

        [Theory]
        [AutoSetup("LatestContactExists")]
        public void LoadContactReadOnly_ContactExists_ReturnsContact(
            IElasticAnalyticsContactService contactService,
            ElasticContact eContact,
            ISystemContext ctx)
        {
            var res = contactService.LoadForReadOnly(eContact.Id, ctx);
            res.Should().NotBeNull();
            res.Id.Should().Be(eContact.Id);
        }

        [Theory]
        [AutoSetup("LatestContactExists", "ContactLockedByDifferentOwner")]
        public void LoadContactReadOnly_ContactExistsAndLockedByDifferentOwner_ReturnsContact(
            IElasticAnalyticsContactService contactService,
            ElasticContact eContact,
            ISystemContext ctx)
        {
            var res = contactService.LoadForReadOnly(eContact.Id, ctx);
            res.Should().NotBeNull();
            res.Id.Should().Be(eContact.Id);
        }

        #endregion

        #region DeleteContact

        [Theory]
        [AutoSetup("ContactDoesntExist")]
        public void DeleteContact_ContactExistsAndNotLocked_DeletesContact(
            IElasticAnalyticsContactService contactService,
            ElasticContact eContact,
            ISystemContext ctx)
        {
            contactService.Delete(eContact.Id, ctx);
        }

        #endregion
    }
}
