namespace ElasticAnalytics.ScAdapter.Adapter
{
    using System;

    using ElasticAnalytics.Model.Contact;
    using ElasticAnalytics.Model.Locking;

    using Sitecore.Analytics.DataAccess;
    using Sitecore.Analytics.Model;
    using Sitecore.Analytics.Model.Entities;
    using Sitecore.Data;

    using LockAttemptStatus = Sitecore.Analytics.DataAccess.LockAttemptStatus;

    public partial class ElasticAnalyticsDataAdapterProvider
    {
        public override Sitecore.Analytics.DataAccess.LockAttemptResult<IContact> TryLoadContact(
            ID id, 
            IContactFactory factory, 
            LeaseOwner leaseOwner, 
            TimeSpan leaseDuration)
        {
            var res = this.contactService.TryLoadAndLock(
                id.Guid,
                this.modelMapper.Map<LeaseOwner, ElasticLeaseOwner>(leaseOwner), 
                leaseDuration,
                this.ctx);

            IContact scContact = null;
            if (res.LockedObject != null)
            {
                scContact = factory.Create(new ID(res.LockedObject.Id));
                this.modelMapper.Map(res.LockedObject, scContact);
            }

            return new Sitecore.Analytics.DataAccess.LockAttemptResult<IContact>(
                (LockAttemptStatus)(int)res.Status,
                scContact,
                this.modelMapper.Map<ElasticLeaseOwner, LeaseOwner>(res.LeaseOwner));
        }

        public override Sitecore.Analytics.DataAccess.LockAttemptResult<IContact> TryLoadContact(
            string identifier,
            IContactFactory factory,
            LeaseOwner leaseOwner,
            TimeSpan leaseDuration)
        {
            var res = this.contactService.TryLoadAndLock(
                identifier,
                this.modelMapper.Map<LeaseOwner, ElasticLeaseOwner>(leaseOwner),
                leaseDuration,
                this.ctx);

            IContact scContact = null;
            if (res.LockedObject != null)
            {
                scContact = factory.Create(new ID(res.LockedObject.Id));
                this.modelMapper.Map(res.LockedObject, scContact);
            }

            return new Sitecore.Analytics.DataAccess.LockAttemptResult<IContact>(
                (LockAttemptStatus)(int)res.Status,
                scContact,
                this.modelMapper.Map<ElasticLeaseOwner, LeaseOwner>(res.LeaseOwner));
        }

        public override bool TryExtendContactLockLease(IContact contact, TimeSpan leaseDuration)
        {
            if (contact.Lease == null)
            {
                throw new InvalidOperationException("Cannot extend lease on a contact record that does not have a valid lease.");
            }

            return this.contactService.TryExtendLock(
                this.modelMapper.Map<IContact, ElasticContact>(contact),
                leaseDuration, 
                this.ctx);
        }

        public override IContact LoadContactReadOnly(string identifier, IContactFactory factory)
        {
            var res = this.contactService.LoadForReadOnly(identifier, this.ctx);
            var scContact = factory.Create(new ID(res.Id));
            return this.modelMapper.Map(res, scContact);
        }

        public override IContact LoadContactReadOnly(ID id, IContactFactory factory)
        {
            var res = this.contactService.LoadForReadOnly(id.Guid, this.ctx);
            var scContact = factory.Create(new ID(res.Id));
            return this.modelMapper.Map(res, scContact);
        }

        public override bool ObsoleteContact(ID contact, LeaseOwner leaseOwner, ID successor)
        {
           return this.contactService.Obsolete(
                contact.Guid,
                successor.Guid,
                this.modelMapper.Map<LeaseOwner, ElasticLeaseOwner>(leaseOwner),
                this.ctx);
        }

        public override void DeleteContact(ID id)
        {
            this.contactService.Delete(id.Guid, this.ctx);
        }

        public override bool ReleaseContact(ID id, LeaseOwner leaseOwner)
        {
            return this.contactService.Release(
                id.Guid,
                this.modelMapper.Map<LeaseOwner, ElasticLeaseOwner>(leaseOwner),
                this.ctx);
        }

        public override bool SaveContact(IContact contact, ContactSaveOptions saveOptions)
        {
            var res = this.contactService.Save(
                this.modelMapper.Map<IContact, ElasticContact>(contact),
                this.modelMapper.Map<LeaseOwner, ElasticLeaseOwner>(saveOptions.LeaseOwner),
                saveOptions.Release,
                this.ctx);

            if (res && saveOptions.Release)
            {
                contact.Lease = null;
            }

            return res;
        }
    }
}
