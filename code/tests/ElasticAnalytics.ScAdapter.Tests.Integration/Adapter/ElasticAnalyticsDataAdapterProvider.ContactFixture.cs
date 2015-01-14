namespace ElasticAnalytics.ScAdapter.Tests.Integration.Adapter
{
    using System;

    using Castle.MicroKernel.Registration;

    using ElasticAnalytics.Container.Windsor;
    using ElasticAnalytics.Repository.Types;
    using ElasticAnalytics.Repository.Types.Repositories;
    using ElasticAnalytics.ScAdapter.Adapter;
    using ElasticAnalytics.ScAdapter.Tests.Customizations;
    using ElasticAnalytics.Tests.Common;
    using ElasticAnalytics.Tests.Common.Customizations;
    using ElasticAnalytics.Tests.Common.EsUtils;
    using ElasticAnalytics.Tests.Common.Installers;

    using FluentAssertions;

    using Ploeh.AutoFixture;
    using Ploeh.AutoFixture.AutoMoq;

    using Sitecore.Analytics.DataAccess;
    using Sitecore.Analytics.Model;
    using Sitecore.Analytics.Model.Entities;

    using Treevs.Essentials.AutoFixture.Xunit.AutoSetup;

    using Xunit.Extensions;

    public class ElasticAnalyticsDataAdapterProviderFixture
    {
        public static Action<IFixture> AutoSetup()
        {
            return (f) =>
            {
                const string systemKey = "ElasticAnalyticsDataAdapterFixture";
                var instanceKey = f.Create<string>();

                // Configure ElasticAnalytics as in production...
                var container = new ElasticAnalyticsWindsorContainer();
                container.Initialize("default", "inprocess"); // with only the simple 'in process' execution model for now

                // With a few test tweaks...
                container.Container.Install(
                    new TestSettingsInstaller(systemKey, instanceKey), // don't rely on config files to provide config
                    new TimeTravelInstaller()); // allow us to travel in time if we want

                // Setup AutoFixture...
                f.Register<DataAdapterProvider>(() => new ElasticAnalyticsDataAdapterProvider(container)); // as it will be called by SC.
                f
                    // Objects resolved by Windsor first, then (Auto)Moq, then AutoFixture
                    .Customize(new AutoMoqCustomization())
                    .Customize(new WindsorAdapterCustomization(container.Container))
                    .Customize(new ContactFactoryCustomization())
                    .Customize(new IContactCustomization(f.Create<IContactFactory>())) // create meaningful contacts
                    .Customize(
                        new TestIndicesUtilCustomization(
                            container.Container.Resolve<IRequestConfiguration>(ElasticAnalyticsWindsorSettings.Configuration.ContactStorageSettingsKey)));
            };
        }

        // TODO: Add more tests to cover edge cases etc...

        #region SaveContact

        [Theory]
        [AutoSetup]
        public void SaveContact_ContactDoesntExist_ContactSave(
            DataAdapterProvider provider,
            IContactFactory contactFactory,
            IContact contact,
            LeaseOwner leaseOwner,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                provider.SaveContact(contact, new ContactSaveOptions(true, leaseOwner)).Should().BeTrue();

                var actual = provider.LoadContactReadOnly(contact.Id, contactFactory);

                AssertContactsEqual(actual, contact);
            }
        }

        #endregion

        #region LoadContactReadOnly

        [Theory]
        [AutoSetup]
        public void LoadContactReadOnlyById_ContactExists_ContactReturned(
            DataAdapterProvider provider,
            IContactFactory contactFactory,
            IContact contact,
            LeaseOwner leaseOwner,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                provider.SaveContact(contact, new ContactSaveOptions(true, leaseOwner)).Should().BeTrue();

                var actual = provider.LoadContactReadOnly(contact.Id, contactFactory);

                AssertContactsEqual(actual, contact);
            }
        }

        [Theory]
        [AutoSetup]
        public void LoadContactReadOnlyByIdentifier_ContactExists_ContactReturned(
            DataAdapterProvider provider,
            IContactFactory contactFactory,
            IContact contact,
            LeaseOwner leaseOwner,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                provider.SaveContact(contact, new ContactSaveOptions(true, leaseOwner)).Should().BeTrue();

                var actual = provider.LoadContactReadOnly(contact.Identifiers.Identifier, contactFactory);

                AssertContactsEqual(actual, contact);
            }
        }

        #endregion

        #region TryLoadContact

        [Theory]
        [AutoSetup]
        public void TryLoadContactByIdentifier_ContactExistsAndNotLocked_ContactReturned(
            DataAdapterProvider provider,
            IContactFactory contactFactory,
            IContact contact,
            LeaseOwner leaseOwner,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                provider.SaveContact(contact, new ContactSaveOptions(true, leaseOwner)).Should().BeTrue();

                var actual = provider.TryLoadContact(contact.Identifiers.Identifier, contactFactory, leaseOwner, TimeSpan.FromMinutes(1));

                actual.Status.Should().Be(LockAttemptStatus.Success);
                AssertContactsEqual(actual.Object, contact, l => l.Owner == leaseOwner && l.ExpirationTime > DateTime.UtcNow);
            }
        }

        [Theory]
        [AutoSetup]
        public void TryLoadContactById_ContactExistsAndNotLocked_ContactReturned(
            DataAdapterProvider provider,
            IContactFactory contactFactory,
            IContact contact,
            LeaseOwner leaseOwner,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                provider.SaveContact(contact, new ContactSaveOptions(true, leaseOwner)).Should().BeTrue();

                var actual = provider.TryLoadContact(contact.Id, contactFactory, leaseOwner, TimeSpan.FromMinutes(1));

                actual.Status.Should().Be(LockAttemptStatus.Success);
                AssertContactsEqual(actual.Object, contact, l => l.Owner == leaseOwner && l.ExpirationTime > DateTime.UtcNow);
            }
        }

        [Theory]
        [AutoSetup]
        public void TryLoadContactById_ContactDoesntExist_NullReturned(
            DataAdapterProvider provider,
            IContactFactory contactFactory,
            IContact contact,
            LeaseOwner leaseOwner,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                var actual = provider.TryLoadContact(contact.Id, contactFactory, leaseOwner, TimeSpan.FromMinutes(1));

                actual.Status.Should().Be(LockAttemptStatus.NotFound);
                actual.Object.Should().BeNull();
            }
        }

        #endregion

        #region DeleteContact

        [Theory]
        [AutoSetup]
        public void DeleteContact_ContactExists_ContactRemoved(
            DataAdapterProvider provider,
            IContactFactory contactFactory,
            IContact contact,
            LeaseOwner leaseOwner,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                provider.SaveContact(contact, new ContactSaveOptions(true, leaseOwner)).Should().BeTrue();

                provider.DeleteContact(contact.Id);

                var actual = provider.TryLoadContact(contact.Id, contactFactory, leaseOwner, TimeSpan.FromMinutes(1));
                actual.Status.Should().Be(LockAttemptStatus.NotFound);
                actual.Object.Should().BeNull();
            }
        }

        #endregion

        #region ReleaseContact

        [Theory]
        [AutoSetup]
        public void ReleaseContact_ContactExistsAndLocked_ContactReleased(
            DataAdapterProvider provider,
            IContactFactory contactFactory,
            IContact contact,
            LeaseOwner them,
            LeaseOwner us,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                // lock contact for 'them'
                provider.SaveContact(contact, new ContactSaveOptions(false, them)).Should().BeTrue();

                provider.ReleaseContact(contact.Id, them);

                var actual = provider.TryLoadContact(contact.Id, contactFactory, us, TimeSpan.FromMinutes(1));
                actual.Status.Should().Be(LockAttemptStatus.Success);
                AssertContactsEqual(actual.Object, contact, l => l.Owner == us && l.ExpirationTime > DateTime.UtcNow);
            }
        }

        #endregion

        #region ObsoleteContact

        [Theory]
        [AutoSetup]
        public void ObsoleteContact_ContactExistsAndNotLocked_ContactObsoleted(
            DataAdapterProvider provider,
            IContactFactory contactFactory,
            IContact contact,
            IContact successor,
            LeaseOwner leaseOwner,
            LeaseOwner us,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                // lock contact for 'them'
                provider.SaveContact(contact, new ContactSaveOptions(true, leaseOwner)).Should().BeTrue();

                provider.ObsoleteContact(contact.Id, leaseOwner, successor.Id);
                provider.SaveContact(successor, new ContactSaveOptions(true, leaseOwner)).Should().BeTrue();

                var actual = provider.LoadContactReadOnly(contact.Id, contactFactory);
                AssertContactsEqual(actual, successor);
            }
        }

        #endregion

        #region TryExtendContactLockLease

        [Theory]
        [AutoSetup]
        public void TryExtendLock_ContactExistsLockedBySameOwner_LeaseExtended(
            DataAdapterProvider provider,
            IContact contact,
            LeaseOwner leaseOwner,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                provider.SaveContact(contact, new ContactSaveOptions(false, leaseOwner, TimeSpan.FromMinutes(1)))
                    .Should().Be(true);

                contact.Lease.Owner = leaseOwner;
                var res = provider.TryExtendContactLockLease(contact, TimeSpan.FromHours(1));

                res.Should().BeTrue();
                contact.Lease.Owner.Identifier.Should().Be(leaseOwner.Identifier);
            }
        }

        [Theory]
        [AutoSetup]
        public void TryExtendLock_ContactExistsLockedByDifferentOwner_NotExtended(
            DataAdapterProvider provider,
            IContact contact,
            LeaseOwner them,
            LeaseOwner us,
            TestIndexUtils contactIndex)
        {
            using (contactIndex)
            {
                // lock by a different owner
                provider.SaveContact(contact, new ContactSaveOptions(false, them, TimeSpan.FromMinutes(1)))
                    .Should().Be(true);

                // extend it
                contact.Lease.Owner = us;
                var res = provider.TryExtendContactLockLease(contact, TimeSpan.FromHours(1));

                res.Should().BeFalse();
                // In Andes, owner is not updated to reflect the persistence layer, so we do the same here.
                contact.Lease.Owner.Identifier.Should().Be(us.Identifier);
            }
        }

        #endregion

        #region Assertions

        private static void AssertContactsEqual(
            IContact actual, 
            IContact expected,
            Func<LeaseData, bool> leaseAssertion = null)
        {
            actual.ShouldBeEquivalentTo(expected, op => op
                   .Excluding(c => c.Lease)
                   .Excluding(c => c.Identifiers)
                   .Excluding(c => c.Tags)
                   .Excluding(c => c.System)
                   .Excluding(c => c.Extensions)
                   .Excluding(c => c.Facets));

            actual.Identifiers.ShouldBeEquivalentTo(expected.Identifiers, op => op.Excluding(i => i.Members));
            actual.Tags.ShouldBeEquivalentTo(expected.Tags, op => op.Excluding(i => i.Members));
            actual.System.ShouldBeEquivalentTo(expected.System, op => op.Excluding(i => i.Members));
            actual.Extensions.ShouldBeEquivalentTo(expected.Extensions, op => op.Excluding(i => i.Members));
            actual.Facets.ShouldBeEquivalentTo(expected.Facets);

            if (leaseAssertion != null)
            {
                leaseAssertion(actual.Lease).Should().BeTrue();
            }
            else
            {
                actual.Lease.ShouldBeEquivalentTo(expected.Lease, opt => opt.Excluding(l => l.ExpirationTime));
            }
        }

        #endregion
    }
}
