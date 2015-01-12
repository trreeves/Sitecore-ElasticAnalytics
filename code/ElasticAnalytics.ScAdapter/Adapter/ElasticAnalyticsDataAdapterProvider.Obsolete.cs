namespace ElasticAnalytics.ScAdapter.Adapter
{
    using System;

    using ElasticAnalytics.Model.Contact;
    using ElasticAnalytics.Model.Locking;

    using Sitecore.Analytics.DataAccess;
    using Sitecore.Analytics.Model;
    using Sitecore.Analytics.Model.Entities;

    /// <summary>
    /// Go on, delete them.  I dare you.
    /// </summary>
    /// <remarks>It is the worst possible choice in this scenario to mark them as obsolete, AND then replace the
    /// implementation with exceptions.  This is a really really bad idea. Really bad.</remarks>
    public partial class ElasticAnalyticsDataAdapterProvider : DataAdapterProvider
    {
        [Obsolete]
        public override bool SaveContact(IContact contact, LeaseOwner leaseOwner, bool release)
        {
            var res = this.contactService.Save(
                this.modelMapper.Map<IContact, ElasticContact>(contact),
                this.modelMapper.Map<LeaseOwner, ElasticLeaseOwner>(leaseOwner),
                release,
                this.ctx);

            if (res && release)
            {
                contact.Lease = null;
            }

            return res;
        }

        [Obsolete]
        public override IContact LoadContactReadOnly(string identifier)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public override IContact LoadContact(string userName)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public override void DeleteContact(Guid contactId)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public override Sitecore.Analytics.DataAccess.LockAttemptResult<IContact> TryLoadContact(Guid contactId, LeaseOwner leaseOwner, TimeSpan leaseDuration)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public override IContact LoadContactReadOnly(Guid contactId)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public override bool ReleaseContact(Guid id, LeaseOwner leaseOwner)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public override bool SaveContact(ContactData contact, LeaseOwner leaseOwner, bool release)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public override LookupBase Lookups
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
