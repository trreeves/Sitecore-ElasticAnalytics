namespace ElasticAnalytics.Service.Tests.Integration.Services
{
    using System;

    using Castle.Windsor;

    using ElasticAnalytics.Configuration.Windsor.Common;
    using ElasticAnalytics.Configuration.Windsor.Repository;
    using ElasticAnalytics.Configuration.Windsor.Service;
    using ElasticAnalytics.Model.Contact;
    using ElasticAnalytics.Model.Locking;
    using ElasticAnalytics.Repository.Elasticsearch;
    using ElasticAnalytics.Repository.Elasticsearch.Contacts.PersistenceModel;
    using ElasticAnalytics.Repository.Elasticsearch.PersistenceModel;
    using ElasticAnalytics.Repository.Types;
    using ElasticAnalytics.Repository.Types.Repositories;
    using ElasticAnalytics.Service.Types;
    using ElasticAnalytics.Service.Types.Services;
    using ElasticAnalytics.Tests.Common;
    using ElasticAnalytics.Tests.Common.Customizations;
    using ElasticAnalytics.Tests.Common.EsUtils;
    using ElasticAnalytics.Tests.Common.Installers;
    using ElasticAnalytics.Tests.Common.Utils;
    using ElasticAnalytics.Tests.Customizations.Assertions;
    using ElasticAnalytics.Tests.Customizations.Customizations;
    using ElasticAnalytics.Utils.DateTime;

    using FluentAssertions;

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
                    const string systemKey = "ElasticAnalyticsContactServiceFixture";
                    var instanceKey = f.Create<string>();

                    // Setup Windsor as in production...
                    var container = new WindsorContainer()
                        .Install(
                            new ContainerFeaturesDefaultInstaller(),
                            new TypeMapperDefaultInstaller(),

                            new RepositoryMappingDefaultInstaller(),
                            new RepositoryInProcessInstaller(),
                            new ContactsRepositoryInProcessInstaller(),

                            new ServiceInProcessInstaller(),

                            new TestSettingsInstaller(systemKey, instanceKey),
                            new TimeTravelInstaller());

                    // Setup AutoFixture...
                    f
                        // Objects resolved by Windsor first, then (Auto)Moq, then AutoFixture
                        .Customize(new AutoMoqCustomization())
                        .Customize(new WindsorAdapterCustomization(container))
                        .Customize(new ElasticContactCustomization()) // create meaningful contacts
                        .Customize(new SystemContextCustomization())
                        .Customize(new TestIndicesUtilCustomization(container.Resolve<EsRequestConfiguration<ElasticContact, EsContact>>()));
                };
        }

        public static Action<IFixture> UnknownContact()
        {
            return (f) => f.Customize(new ElasticContactCustomization(IdentificationLevel.None));
        }

        #region LoadAndLock

        [Theory]
        [AutoSetup]
        public void TryLoadAndLockById_ContactExistsAndNotLockedWithNoSuccessors_ReturnsContact(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner us,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(contact, us, true, ctx)
                    .Should().Be(true);

                var res = contactService.TryLoadAndLock(contact.Id, us, TimeSpan.FromMinutes(1), ctx);

                res.Status.Should().Be(LockAttemptStatus.Success);
                res.LockedObject.ShouldBeEquivalentToContact(contact, l => l.Expires > DateTime.UtcNow && l.IsOwnedBy(us) && l.Version == 3);
            }
        }

        [Theory]
        [AutoSetup]
        public void TryLoadAndLockByIdentity_ContactExistsAndNotLockedWithNoSuccessors_ReturnsContact(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner us,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(contact, us, true, ctx)
                    .Should().Be(true);

                var res = contactService.TryLoadAndLock(contact.Identification.Identity, us, TimeSpan.FromMinutes(1), ctx);

                res.Status.Should().Be(LockAttemptStatus.Success);
                res.LockedObject.ShouldBeEquivalentToContact(contact, l => l.Expires > DateTime.UtcNow && l.IsOwnedBy(us) && l.Version == 3);
            }
        }

        [Theory]
        [AutoSetup]
        public void TryLoadAndLockById_ContactExistsWithSuccessor_ContactReturned(
            TestIndexUtils contactIndex,
            IElasticAnalyticsContactService contactService,
            ElasticContact firstContact,
            ElasticContact secondContact,
            ElasticLeaseOwner us,
            ISystemContext ctx)
        {
            using (contactIndex)
            {
                firstContact.Successor = secondContact.Id;
                contactService.Save(firstContact, us, true, ctx);
                contactService.Save(secondContact, us, true, ctx);

                var res = contactService.TryLoadAndLock(firstContact.Id, us, TimeSpan.FromMinutes(1), ctx);

                res.Status.Should().Be(LockAttemptStatus.Success);
                res.LockedObject.ShouldBeEquivalentToContact(secondContact, l => l.Expires > DateTime.UtcNow && l.IsOwnedBy(us) && l.Version == 3);
            }
        }

        [Theory]
        [AutoSetup]
        public void TryLoadAndLockByIdentity_ContactExistsWithSuccessor_ContactReturned(
            TestIndexUtils contactIndex,
            IElasticAnalyticsContactService contactService,
            ElasticContact firstContact,
            ElasticContact secondContact,
            ElasticLeaseOwner us,
            ISystemContext ctx)
        {
            using (contactIndex)
            {
                firstContact.Successor = secondContact.Id;
                contactService.Save(firstContact, us, true, ctx);
                contactService.Save(secondContact, us, true, ctx);

                var res = contactService.TryLoadAndLock(firstContact.Identification.Identity, us, TimeSpan.FromMinutes(1), ctx);

                res.Status.Should().Be(LockAttemptStatus.Success);
                res.LockedObject.ShouldBeEquivalentToContact(secondContact, l => l.Expires > DateTime.UtcNow && l.IsOwnedBy(us) && l.Version == 3);
            }
        }

        [Theory]
        [AutoSetup]
        public void TryLoadAndLockById_ContactExistsAlreadyLockedBySameOwner_ReturnsContact(
            IElasticAnalyticsContactService contactService,
            ElasticLeaseOwner us,
            ElasticContact eContact,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(eContact, us, false, ctx).Should().BeTrue();

                var res = contactService.TryLoadAndLock(eContact.Id, us, TimeSpan.FromMinutes(1), ctx);

                eContact.Lease.IncrementVersion();
                res.LockedObject.ShouldBeEquivalentToContact(eContact);
                res.Status.Should().Be(LockAttemptStatus.Success);
            }
        }

        [Theory]
        [AutoSetup]
        public void TryLoadAndLockByIdentity_ContactExistsAlreadyLockedBySameOwner_ReturnsContact(
            IElasticAnalyticsContactService contactService,
            ElasticLeaseOwner us,
            ElasticContact eContact,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(eContact, us, false, ctx).Should().BeTrue();

                var res = contactService.TryLoadAndLock(eContact.Identification.Identity, us, TimeSpan.FromMinutes(1), ctx);

                eContact.Lease.IncrementVersion();
                res.LockedObject.ShouldBeEquivalentToContact(eContact);
                res.Status.Should().Be(LockAttemptStatus.Success);
            }
        }

        [Theory]
        [AutoSetup]
        public void TryLoadAndLockById_ContactExistsAlreadyLockedByDifferentOwner_NotLoaded(
            IElasticAnalyticsContactService contactService,
            ElasticLeaseOwner us,
            ElasticLeaseOwner them,
            ElasticContact eContact,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(eContact, them, false, ctx).Should().BeTrue();

                var res = contactService.TryLoadAndLock(eContact.Id, us, TimeSpan.FromMinutes(1), ctx);
                res.LockedObject.Id.Should().Be(eContact.Id);
                res.Status.Should().Be(LockAttemptStatus.AlreadyLocked);
            }
        }

        [Theory]
        [AutoSetup]
        public void TryLoadAndLockByIdentity_ContactExistsAlreadyLockedByDifferentOwner_NotLoaded(
            IElasticAnalyticsContactService contactService,
            ElasticLeaseOwner us,
            ElasticLeaseOwner them,
            ElasticContact eContact,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(eContact, them, false, ctx).Should().BeTrue();

                var res = contactService.TryLoadAndLock(eContact.Identification.Identity, us, TimeSpan.FromMinutes(1), ctx);
                res.LockedObject.Id.Should().Be(eContact.Id);
                res.Status.Should().Be(LockAttemptStatus.AlreadyLocked);
            }
        }

        [Theory]
        [AutoSetup]
        public void TryLoadAndLockById_ContactDoesntExist_NullReturned(
            IElasticAnalyticsContactService contactService,
            ElasticLeaseOwner us,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                var res = contactService.TryLoadAndLock(Guid.NewGuid(), us, TimeSpan.FromMinutes(1), ctx);
                res.Status.Should().Be(LockAttemptStatus.NotFound);
                res.LockedObject.Should().BeNull();
            }
        }

        [Theory]
        [AutoSetup]
        public void TryLoadAndLockByIdentity_ContactDoesntExist_NullReturned(
            IElasticAnalyticsContactService contactService,
            ElasticLeaseOwner us,
            string identity,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                var res = contactService.TryLoadAndLock(identity, us, TimeSpan.FromMinutes(1), ctx);
                res.Status.Should().Be(LockAttemptStatus.NotFound);
                res.LockedObject.Should().BeNull();
            }
        }

        [Theory]
        [AutoSetup]
        public void TryLoadAndLockById_ContactObsolete_SuccessorReturned(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticContact successor,
            ElasticLeaseOwner owner,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(contact, owner, true, ctx)
                    .Should().Be(true);

                contactService.Obsolete(contact.Id, successor.Id, owner, ctx);
                contactService.Save(successor, owner, true, ctx);

                var res = contactService.TryLoadAndLock(contact.Id, owner, TimeSpan.FromMinutes(1), ctx);

                res.Status.Should().Be(LockAttemptStatus.Success);
                res.LockedObject.ShouldBeEquivalentToContact(successor, l => l.Expires > DateTime.UtcNow && l.IsOwnedBy(owner) && l.Version == 3);
            }
        }

        [Theory]
        [AutoSetup]
        public void TryLoadAndLockByIdentity_ContactObsolete_NullReturned(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticContact successor,
            ElasticLeaseOwner owner,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(contact, owner, true, ctx)
                    .Should().Be(true);

                contactService.Obsolete(contact.Id, successor.Id, owner, ctx);
                contactService.Save(successor, owner, true, ctx);

                var res = contactService.TryLoadAndLock(contact.Identification.Identity, owner, TimeSpan.FromMinutes(1), ctx);
                res.Status.Should().Be(LockAttemptStatus.NotFound);
                res.LockedObject.Should().BeNull();
            }
        }

        #endregion

        #region LoadForReadOnly

        [Theory]
        [AutoSetup]
        public void LoadForReadOnlyById_ContactExistsNotLocked_ContactReturned(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner them,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(contact, them, true, ctx)
                    .Should().Be(true);

                contactService.LoadForReadOnly(contact.Id, ctx)
                    .ShouldBeEquivalentToContact(contact, l => l == null);
            }
        }

        [Theory]
        [AutoSetup]
        public void LoadForReadOnlyById_ContactExistsLockedByDifferentOwner_ContactReturned(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner them,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(contact, them, false, ctx)
                    .Should().Be(true);

                contactService.LoadForReadOnly(contact.Id, ctx)
                    .ShouldBeEquivalentToContact(contact, l => l == null);
            }
        }

        [Theory]
        [AutoSetup]
        public void LoadForReadOnlyById_ContactObsolete_SuccessorReturned(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticContact successor,
            ElasticLeaseOwner owner,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(contact, owner, true, ctx)
                    .Should().Be(true);

                contactService.Obsolete(contact.Id, successor.Id, owner, ctx);
                contactService.Save(successor, owner, true, ctx);

                contactService.LoadForReadOnly(contact.Id, ctx)
                    .ShouldBeEquivalentToContact(successor, l => l == null);
            }
        }

        [Theory]
        [AutoSetup]
        public void LoadForReadOnlyByIdentity_ContactExistsNotLocked_ContactReturned(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner them,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(contact, them, true, ctx)
                    .Should().Be(true);

                contact.Lease = null;
                contactService.LoadForReadOnly(contact.Identification.Identity, ctx)
                    .ShouldBeEquivalentToContact(contact);
            }
        }

        [Theory]
        [AutoSetup]
        public void LoadForReadOnlyByIdentity_ContactExistsLockedByDifferentOwner_ContactReturned(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner them,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(contact, them, false, ctx)
                    .Should().Be(true);

                contact.Lease = null;
                contactService.LoadForReadOnly(contact.Identification.Identity, ctx)
                    .ShouldBeEquivalentToContact(contact);
            }
        }

        [Theory]
        [AutoSetup]
        public void LoadForReadOnlyByIdentity_ContactObsolete_NullReturned(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticContact successor,
            ElasticLeaseOwner owner,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(contact, owner, true, ctx)
                    .Should().Be(true);

                contactService.Obsolete(contact.Id, successor.Id, owner, ctx);
                contactService.Save(successor, owner, true, ctx);

                contactService.LoadForReadOnly(contact.Identification.Identity, ctx)
                    .Should().BeNull();
            }
        }

        #endregion

        #region SaveContact

        [Theory]
        [AutoSetup]
        public void SaveContact_ContactDoesntExistShouldBeReleased_ContactCreatedLockFree(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner them,
            ElasticLeaseOwner us,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(contact, them, true, ctx)
                    .Should().Be(true);

                var res = contactService.TryLoadAndLock(contact.Id, us, TimeSpan.FromMinutes(1), ctx);

                res.Status.Should().Be(LockAttemptStatus.Success);
                res.LockedObject.ShouldBeEquivalentToContact(contact, l => l.Expires > DateTime.UtcNow && l.IsOwnedBy(us) && l.Version == 3);

                contactService.LoadForReadOnly(contact.Identification.Identity, ctx)
                    .ShouldBeEquivalentToContact(contact);
            }
        }

        [Theory]
        [AutoSetup]
        public void SaveContact_ContactDoesntExistShouldNotBeReleased_ContactCreatedWithLock(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner us,
            ElasticLeaseOwner them,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(contact, them, false, ctx)
                    .Should().Be(true);

                var res = contactService.TryLoadAndLock(contact.Id, us, TimeSpan.FromMinutes(1), ctx);

                res.Status.Should().Be(LockAttemptStatus.AlreadyLocked);
                res.LockedObject.ShouldBeEquivalentToContact(contact);

                contactService.LoadForReadOnly(contact.Identification.Identity, ctx)
                    .ShouldBeEquivalentToContact(contact, l => l == null);
            }
        }

        [Theory]
        [AutoSetup("UnknownContact")]
        public void SaveContact_ContactExistsNotLockedWithNoIdentity_ContactUpdated(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner us,
            ElasticLeaseOwner them,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contact.Identification.IdentityLevel.Should().Be(IdentificationLevel.None);

                contactService.Save(contact, them, true, ctx)
                    .Should().Be(true);

                // contact..VisitCount = 999; // make a change

                contactService.Save(contact, us, true, ctx)
                    .Should().Be(true);

                // Assert
                var res = contactService.TryLoadAndLock(contact.Id, us, TimeSpan.FromMinutes(1), ctx);
                res.Status.Should().Be(LockAttemptStatus.Success);
                res.LockedObject.ShouldBeEquivalentToContact(contact, l => l.Expires > DateTime.UtcNow && l.IsOwnedBy(us) && l.Version == 5);
            }
        }

        [Theory]
        [AutoSetup]
        public void SaveContact_ContactLockedbyDifferentOwner_ContactNotAdded(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner us,
            ElasticLeaseOwner them,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(contact, them, false, ctx)
                    .Should().Be(true);

                contactService.Save(contact, us, false, ctx)
                    .Should().Be(false);
            }
        }
        
        [Theory]
        [AutoSetup]
        public void SaveContact_ContactExistsWithIdentityNotLocked_ContactUpdated(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner us,
            ElasticLeaseOwner them,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contact.Identification.IdentityLevel.Should().Be(IdentificationLevel.Known);

                contactService.Save(contact, them, true, ctx)
                    .Should().Be(true);

                //contact.SystemInfo.VisitCount = 999; // make a change

                contactService.Save(contact, us, false, ctx)
                    .Should().Be(true);

                // Assert
                var res = contactService.TryLoadAndLock(contact.Id, us, TimeSpan.FromMinutes(1), ctx);
                res.Status.Should().Be(LockAttemptStatus.Success);
                res.LockedObject.ShouldBeEquivalentToContact(contact, l => l.Expires > DateTime.UtcNow && l.IsOwnedBy(us) && l.Version == 4);

                contactService.LoadForReadOnly(contact.Identification.Identity, ctx)
                    .ShouldBeEquivalentToContact(contact, l => l == null);
            }
        }

        [Theory]
        [AutoSetup]
        public void SaveContact_ContactExistsWithIdentityAndIdentityInUse_ThrowsException(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact1,
            ElasticContact contact2,
            ElasticLeaseOwner owner,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(contact1, owner, true, ctx).Should().Be(true);

                // set the identity to be the same
                contact2.Identification.SetIdentity(
                    contact1.Identification.Identity,
                    contact1.Identification.IdentityLevel);

                AssertionExtensions.ShouldThrow<InvalidOperationException>(
                    () => contactService.Save(contact2, owner, true, ctx));
            }
        }

        [Theory(Skip = "requires unit test")]
        [AutoSetup]
        public void SaveContact_ContactExistsWithNoIdentityAndContactSaveFailed_NewIdentityRemoved(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner owner,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            // ? how would a save contact fail, but first an identity save succeeds? (i.e. ES isn't down)
        }

        [Theory(Skip = "requires unit test")]
        [AutoSetup]
        public void SaveContact_ContactExistsWithIdentityAndContactSaveFailed_ExistingIdentityRestored(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner owner,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            // ? how would a save contact fail, but first an identity save succeeds? (i.e. ES isn't down)
        }

        #endregion

        #region Extend

        [Theory]
        [AutoSetup]
        public void TryExtendLock_ContactExistsLockedBySameOwner_LeaseExtended(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner us,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(contact, us, false, ctx)
                    .Should().Be(true);

                var res = contactService.TryExtendLock(contact, TimeSpan.FromHours(1), ctx);

                res.Should().BeTrue();
                contact.Lease.IsOwnedBy(us);
            }
        }

        [Theory]
        [AutoSetup]
        public void TryExtendLock_ContactExistsLockedByDifferentOwner_NotExtended(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner them,
            ElasticLeaseOwner us,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                // lock by a different owner
                contactService.Save(contact, them, false, ctx)
                    .Should().Be(true);

                // extend it
                contact.Lease = new ElasticLease(contact.Id, DateTime.UtcNow, us); // specify the lease we want
                var res = contactService.TryExtendLock(contact, TimeSpan.FromHours(1), ctx);

                res.Should().BeFalse();
                contact.Lease.IsOwnedBy(us); // currently Andes does not update the lease reference in this scenario
            }
        }

        #endregion

        #region DeleteContact

        [Theory]
        [AutoSetup]
        public void DeleteContact_ContactExistsWithNoLease_ContactDeleted(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner us,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService
                    .Save(contact, us, true, ctx)
                    .Should().BeTrue();

                contactService.Delete(contact.Id, ctx);

                contactService
                    .LoadForReadOnly(contact.Id, ctx)
                    .Should()
                    .BeNull();
            }
        }

        [Theory]
        [AutoSetup]
        public void DeleteContact_ContactExistsWithExpiredLeaseByDifferentOwner_ContactDeleted(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner them,
            ISystemContext ctx,
            IDateTimeController dateTime,
            TestIndexUtils contactIndex) 
        {
            using (contactIndex)
            {
                // arrange contact with expired lease from another owner...
                var timeTraveller = (DateTimeTimeTraveller)dateTime;

                using (timeTraveller.NewJourney(-24))
                {
                    contactService.Save(contact, them, false, ctx).Should().BeTrue();
                }

                // deletes don't have an owner, at this level at least.
                contactService.Delete(contact.Id, ctx);

                contactService
                    .LoadForReadOnly(contact.Id, ctx)
                    .Should()
                    .BeNull();
            }
        }

        [Theory]
        [AutoSetup]
        public void DeleteContact_ContactExistsWithActiveLeaseBySameOwner_ContactDeleted(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner us,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(contact, us, false, ctx).Should().BeTrue();

                // deletes seem to be brutal - they don't care about locks
                contactService.Delete(contact.Id, ctx);

                contactService
                    .LoadForReadOnly(contact.Id, ctx)
                    .Should()
                    .BeNull();
            }
        }

        [Theory]
        [AutoSetup]
        public void DeleteContact_ContactExistsWithActiveLeaseByDifferentOwner_DeleteStillHonoured(
            IElasticAnalyticsContactService contactService,
            ILeaseService leaseService,
            ElasticContact contact,
            ElasticLeaseOwner us,
            ElasticLeaseOwner them,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(contact, them, false, ctx).Should().BeTrue();

                // deletes seem to be brutal
                contactService.Delete(contact.Id, ctx);

                contactService
                    .LoadForReadOnly(contact.Id, ctx)
                    .Should()
                    .BeNull();

                // lease is deleted so if we wanted, we can just go and create another contact with the
                // same id straight away...
                contact.Lease = null;
                contactService.Save(contact, us, false, ctx).Should().BeTrue();
            }
        }

        [Theory]
        [AutoSetup]
        public void DeleteContact_ContactDoesntExist_ReturnsSuccessfully(
            IElasticAnalyticsContactService contactService,
            ILeaseService leaseService,
            ElasticContact contact,
            ElasticLeaseOwner us,
            ElasticLeaseOwner them,
            ISystemContext ctx,
            TestIndexUtils contactIndex) 
        {
            using (contactIndex)
            {
                AssertionExtensions.ShouldNotThrow(() => 
                    contactService.Delete(contact.Id, ctx));
            }
        }

        #endregion DeleteContact

        #region ObsoleteContact

        [Theory]
        [AutoSetup]
        public void ObsoleteContact_ContactExistsNotLocked_ContactObsoleted(
            IElasticAnalyticsContactService contactService,
            IRepository<ElasticContact> contactRepo,
            ElasticContact contact,
            Guid successor,
            ElasticLeaseOwner us,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(contact, us, true, ctx).Should().BeTrue();

                contactService.Obsolete(contact.Id, successor, us, ctx).Should().BeTrue();

                var obsoletedContact = contactRepo.Get(contact.Id, ctx);

                obsoletedContact.Id.Should().Be(contact.Id);
                obsoletedContact.Successor.Should().Be(successor);
                obsoletedContact.Should().BeObsolete();
            }
        }

        [Theory]
        [AutoSetup]
        public void ObsoleteContact_ContactExistsWithSuccessorNotLocked_ContactObsoleted(
            IElasticAnalyticsContactService contactService,
            IRepository<ElasticContact> contactRepo,
            ElasticContact contact,
            Guid successor,
            ElasticLeaseOwner us,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            // andes doesn't care if a contact has successors or not apart from when loading a contact.
            using (contactIndex)
            {
                contact.Successor = Guid.NewGuid();
                contactService.Save(contact, us, true, ctx).Should().BeTrue();

                contactService.Obsolete(contact.Id, successor, us, ctx).Should().BeTrue(); 
                var obsoletedContact = contactRepo.Get(contact.Id, ctx);

                obsoletedContact.Id.Should().Be(contact.Id);
                obsoletedContact.Successor.Should().Be(successor);
                obsoletedContact.Should().BeObsolete();
            }
        }

        [Theory]
        [AutoSetup]
        public void ObsoleteContact_ContactDoesntExist_NothingHappens(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            Guid successor,
            ElasticLeaseOwner owner,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                AssertionExtensions.ShouldNotThrow(() => contactService.Obsolete(contact.Id, successor, owner, ctx));
            }
        }

        [Theory]
        [AutoSetup]
        public void ObsoleteContact_ContactExistsAndLockedBySameOwner_ContactObsoleted(
            IElasticAnalyticsContactService contactService,
            IRepository<ElasticContact> contactRepo,
            ElasticContact contact,
            Guid successor,
            ElasticLeaseOwner us,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                contactService.Save(contact, us, false, ctx).Should().BeTrue();

                contactService.Obsolete(contact.Id, successor, us, ctx).Should().BeTrue();

                var obsoletedContact = contactRepo.Get(contact.Id, ctx);

                obsoletedContact.Id.Should().Be(contact.Id);
                obsoletedContact.Successor.Should().Be(successor);
                obsoletedContact.Should().BeObsolete();
            }
        }

         [Theory]
         [AutoSetup]
         public void ObsoleteContact_ContactExistsAndLockedByDifferentOwner_ReturnsFalse(
            IElasticAnalyticsContactService contactService,
            IRepository<ElasticContact> contactRepo,
            ElasticContact contact,
            Guid successor,
            ElasticLeaseOwner us,
            ElasticLeaseOwner them,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
         {
             using (contactIndex)
             {
                 // contact exists and currently locked.
                 contactService
                     .Save(contact, them, false, ctx)
                     .Should()
                     .Be(true);

                 contactService.Obsolete(contact.Id, successor, us, ctx).Should().BeFalse();
             }
         }

        [Theory]
        [AutoSetup]
        public void ObsoleteContact_ContactExistsAndExpiredLockedByDifferentOwner_ContactObsoleted(
            IElasticAnalyticsContactService contactService,
            IRepository<ElasticContact> contactRepo,
            ElasticContact contact,
            Guid successor,
            ElasticLeaseOwner us,
            ElasticLeaseOwner them,
            ISystemContext ctx,
            IDateTimeController dateTime,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                // arrange contact with expired lease from another owner...
                var timeTraveller = (DateTimeTimeTraveller)dateTime;

                using (timeTraveller.NewJourney(-24))
                {
                    contactService.Save(contact, them, false, ctx).Should().BeTrue();
                }

                contactService.Obsolete(contact.Id, successor, us, ctx).Should().BeTrue();

                var obsoletedContact = contactRepo.Get(contact.Id, ctx);

                obsoletedContact.Id.Should().Be(contact.Id);
                obsoletedContact.Successor.Should().Be(successor);
                obsoletedContact.Should().BeObsolete();
            }
        }

        #endregion

        #region ReleaseContact

        [Theory]
        [AutoSetup]
        public void ReleaseContact_ContactDoesntExist_NothingHappens(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner us,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                AssertionExtensions.ShouldNotThrow(() => 
                    contactService.Release(contact.Id, us, ctx));
            }
        }

        [Theory]
        [AutoSetup]
        public void ReleaseContact_NoLockExistsForContact_ContactReleased(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner us,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                // contact exists, no lock
                contactService.Save(contact, us, true, ctx).Should().BeTrue();

                // release
                contactService.Release(contact.Id, us, ctx);

                // should be able to lock it again
                contactService.TryLoadAndLock(contact.Id, us, TimeSpan.FromHours(1), ctx)
                    .Status.Should()
                    .Be(LockAttemptStatus.Success);
            }
        }

        [Theory]
        [AutoSetup]
        public void ReleaseContact_ExpiredLockExistsForSameOwner_NothingHappens(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner us,
            ISystemContext ctx,
            IDateTimeController dateTime,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                // contact exists, lock expired
                var timeTraveller = (DateTimeTimeTraveller)dateTime;
                using (timeTraveller.NewJourney(-24))
                {
                    contactService.Save(contact, us, false, ctx).Should().BeTrue();
                }

                // release
                contactService.Release(contact.Id, us, ctx);

                // should be able to lock it again
                contactService.TryLoadAndLock(contact.Id, us, TimeSpan.FromHours(1), ctx)
                    .Status.Should()
                    .Be(LockAttemptStatus.Success);
            }
        }

        [Theory]
        [AutoSetup]
        public void ReleaseContact_ExpiredLockExistsForDifferentOwner_NothingHappens(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner us,
            ElasticLeaseOwner them,
            ISystemContext ctx,
            IDateTimeController dateTime,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                // contact exists, lock expired
                var timeTraveller = (DateTimeTimeTraveller)dateTime;
                using (timeTraveller.NewJourney(-24))
                {
                    contactService.Save(contact, them, false, ctx).Should().BeTrue();
                }

                // release
                contactService.Release(contact.Id, us, ctx);

                // should be able to lock it again
                contactService.TryLoadAndLock(contact.Id, us, TimeSpan.FromHours(1), ctx)
                    .Status.Should()
                    .Be(LockAttemptStatus.Success);
            }
        }

        [Theory]
        [AutoSetup]
        public void ReleaseContact_CurrentLockBySameOwner_LockReleased(
            IElasticAnalyticsContactService contactService,
            ElasticContact contact,
            ElasticLeaseOwner us,
            ISystemContext ctx,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                // contact exists, current lock by us
                contactService.Save(contact, us, false, ctx).Should().BeTrue();

                // release
                contactService.Release(contact.Id, us, ctx);

                // should be able to lock it again
                contactService.TryLoadAndLock(contact.Id, us, TimeSpan.FromHours(1), ctx)
                    .Status.Should()
                    .Be(LockAttemptStatus.Success);
            }
        }

        [Theory]
        [AutoSetup]
        public void ReleaseContact_CurrentLockByDifferentOwner_ReturnsFalse(
            IElasticAnalyticsContactService contactService,
            IRepository<ElasticContact> contactRepo,
            ElasticContact contact,
            ElasticLeaseOwner us,
            ElasticLeaseOwner them,
            ISystemContext ctx,
            IDateTimeController dateTime,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                // contact exists and locked
                contactService.Save(contact, them, false, ctx).Should().BeTrue();

                // release
                contactService.Release(contact.Id, us, ctx).Should().BeFalse();

                // shouldn't be able to lock it again -- ??
                contactService.TryLoadAndLock(contact.Id, us, TimeSpan.FromHours(1), ctx)
                    .Status.Should()
                    .Be(LockAttemptStatus.AlreadyLocked);
            }
        }

        #endregion
    }
}
