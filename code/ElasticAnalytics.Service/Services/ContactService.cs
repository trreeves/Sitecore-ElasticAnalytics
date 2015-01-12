namespace ElasticAnalytics.Service.Services
{
    using System;

    using ElasticAnalytics.Model.Contact;
    using ElasticAnalytics.Model.Locking;
    using ElasticAnalytics.Repository.Types.Repositories;
    using ElasticAnalytics.Service.Types;
    using ElasticAnalytics.Service.Types.Services;
    using ElasticAnalytics.Utils.DateTime;

    using ElasticAnalyticsSettings = ElasticAnalytics.Utils.ElasticAnalyticsSettings;

    public class ContactService : IElasticAnalyticsContactService
    {
        protected readonly IDateTimeController dateTime;

        protected readonly IRepository<ElasticContact> contactRepo;

        protected readonly IContactIdentityMapService contactIdentityService;

        protected readonly ILeaseService contactLeaseService;

        public ContactService(
            IRepository<ElasticContact> contactRepo,
            IContactIdentityMapService contactIdentityService,
            ILeaseService contactLeaseService,
            IDateTimeController dateTime)
        {
            if (contactRepo == null)
            {
                throw new ArgumentNullException("contactRepo");
            }

            if (contactIdentityService == null)
            {
                throw new ArgumentNullException("contactIdentityService");
            }

            if (contactLeaseService == null)
            {
                throw new ArgumentNullException("contactLeaseService");
            }

            if (dateTime == null)
            {
                throw new ArgumentNullException("dateTime");
            }

            this.contactRepo = contactRepo;
            this.contactIdentityService = contactIdentityService;
            this.contactLeaseService = contactLeaseService;
            this.dateTime = dateTime;
        }

        public virtual LockAttemptResult<ElasticContact> TryLoadAndLock(
            Guid id,            
            ElasticLeaseOwner targetLeaseOwner,
            TimeSpan leaseDuration,
            ISystemContext ctx)
        {
            if (targetLeaseOwner == null)
            {
                throw new ArgumentNullException("targetLeaseOwner", "A lease owner must be specified.");
            }

            var contact = this.GetLatest(id, ctx);

            if (contact == null)
            {
                return new LockAttemptResult<ElasticContact>(LockAttemptStatus.NotFound, null, null);
            }

            if (this.TryLockContact(
                    contact,
                    targetLeaseOwner,
                    leaseDuration,
                    ctx) == LockAttemptStatus.Success)
            {
                return new LockAttemptResult<ElasticContact>(
                    LockAttemptStatus.Success,
                    contact,
                    targetLeaseOwner);
            }
            else
            {
                return new LockAttemptResult<ElasticContact>(
                    LockAttemptStatus.AlreadyLocked,
                    contact,
                    contact.Lease.Owner);
            }

            // LockAttemptResult.DatabaseUnavailable - I'm letting any exceptions bubble up for now
        }

        public virtual LockAttemptResult<ElasticContact> TryLoadAndLock(
            string identity,
            ElasticLeaseOwner targetLeaseOwner,
            TimeSpan leaseDuration,
            ISystemContext ctx)
        {
            var contactIdentityMap = this.contactIdentityService.Get(identity, ctx);
            if (contactIdentityMap != null)
            {
                return this.TryLoadAndLock(contactIdentityMap.ContactId, targetLeaseOwner, leaseDuration, ctx);    
            }
            
            return new LockAttemptResult<ElasticContact>(LockAttemptStatus.NotFound, null, null);
        }

        public virtual bool TryExtendLock(
            ElasticContact contact, 
            TimeSpan leaseDuration,
            ISystemContext ctx)
        {
            if (contact.Lease == null)
            {
                throw new InvalidOperationException("Cannot extend lease on a contact record that does not have a valid lease.");
            }

            ElasticLease currentLease;
            if (this.contactLeaseService.Extend(contact.Lease, leaseDuration, ctx, out currentLease))
            {
                contact.Lease = currentLease;
                return true;
            }

            return false;
        }

        public virtual bool Obsolete(
            Guid id,
            Guid successor,
            ElasticLeaseOwner leaseOwner,
            ISystemContext ctx)
        {
            // get lock for the whole operation
            ElasticLease lease;
            var saveLockStatus = this.TryLockContact(id, leaseOwner, TimeSpan.FromMinutes(10), ctx, out lease);
            if (saveLockStatus != LockAttemptStatus.NotFound && saveLockStatus != LockAttemptStatus.Success)
            {
                // the mongo implementation throws an exception, but this seems to be the wrong thing to do.
                //throw new InvalidOperationException("A contact is locked by another operation.");

                return false;
            }

            try
            {
                var contact = this.contactRepo.Get(id, ctx);

                if (contact != null)
                {
                    // TODO: can do all this async
                    if (contact.Identification.Identity != null)
                    {
                        this.contactIdentityService.Delete(contact.Identification.Identity, ctx);
                    }

                    var obsoleteContact = new ElasticContact(id, successor);
                    this.contactRepo.Save(obsoleteContact, ctx);
                }

                return true;
            }
            finally
            {
                this.contactLeaseService.Release(id, leaseOwner, ctx);
            }
        }

        public virtual void Delete(Guid contactId, ISystemContext ctx)
        {
            var contact = this.contactRepo.Get(contactId, ctx);

            if (contact != null)
            {
                // TODO: can do all this async
                if (contact.Identification.Identity != null)
                {
                    this.contactIdentityService.Delete(contact.Identification.Identity, ctx);
                }

                this.contactRepo.Delete(contactId, ctx);                 
            }

            this.contactLeaseService.Delete(contactId, ctx);
        }

        public virtual bool Release(
            Guid id, 
            ElasticLeaseOwner leaseOwner, 
            ISystemContext ctx)
        {
            return this.contactLeaseService.Release(id, leaseOwner, ctx);
        }

        public virtual bool Save(
            ElasticContact contact, 
            ElasticLeaseOwner leaseOwner, 
            bool release, 
            ISystemContext ctx)
        {
            // obtain a contact lock for the whole operation.
            var saveLockStatus = this.TryLockContact(contact, leaseOwner, TimeSpan.FromMinutes(10), ctx);
            if (saveLockStatus != LockAttemptStatus.NotFound && saveLockStatus != LockAttemptStatus.Success)
            {
                return false;
            }
            
            try
            {
                // TODO: can we do a partial get here?
                var existingContact = this.contactRepo.Get(contact.Id, ctx);

                if (!this.contactIdentityService.MapIdentity(
                        contact.Id,
                        contact.Identification,
                        existingContact == null ? null : existingContact.Identification,
                        ctx))
                {
                    throw new InvalidOperationException("Another contact with the same identifier already exists.");
                }

                try
                {
                    this.contactRepo.Save(contact, ctx);
                    return true;
                }
                catch (Exception)
                {
                    this.contactIdentityService.Delete(contact.Identification.Identity, ctx);
                    release = true; // if the save failed, should we try and force a release as well?
                    throw; // or return false?
                }
            }
            finally
            {
                if (release)
                {
                    this.contactLeaseService.Release(contact.Id, leaseOwner, ctx);
                    contact.Lease = null;
                }
            }
        }

        public virtual ElasticContact LoadForReadOnly(string identity, ISystemContext ctx)
        {
            var contact = this.GetLatest(identity, ctx);

            if (contact != null && contact.Identification.IdentityLevel == IdentificationLevel.Known)
            {
                return contact;
            }

            return null;
        }

        public virtual ElasticContact LoadForReadOnly(Guid id, ISystemContext ctx)
        {
            var contact = this.GetLatest(id, ctx);

            if (contact != null && 
                contact.Identification != null && 
                contact.Identification.IdentityLevel == IdentificationLevel.Known)
            {
                return contact;
            }

            return null;
        }

        public virtual ElasticContact GetLatest(Guid id, ISystemContext ctx)
        {
            var contact = this.contactRepo.Get(id, ctx);

            while (contact != null && contact.Successor != null)
            {
                contact = this.contactRepo.Get(contact.Successor.Value, ctx);
            }

            return contact;
        }

        public virtual ElasticContact GetLatest(string identity, ISystemContext ctx)
        {
            var contactIdentityMap = this.contactIdentityService.Get(identity, ctx);

            if (contactIdentityMap == null)
            {
                return null;
            }

            return this.GetLatest(contactIdentityMap.ContactId, ctx);
        }

        protected virtual LockAttemptStatus TryLockContact(
            ElasticContact contact,
            ElasticLeaseOwner targetLeaseOwner,
            TimeSpan leaseDuration,
            ISystemContext ctx)
        {
            ElasticLease newLease;
            var res = this.TryLockContact(contact.Id, targetLeaseOwner, leaseDuration, ctx, out newLease);
            contact.Lease = newLease;
            return res;
        }

        protected virtual LockAttemptStatus TryLockContact(
            Guid id, 
            ElasticLeaseOwner targetLeaseOwner, 
            TimeSpan leaseDuration, 
            ISystemContext ctx,
            out ElasticLease newLease)
        {
            // TODO: this is a bit crap
            var leaseExpiration = (this.dateTime.UtcNow + ElasticAnalyticsSettings.MaxAcceptedClockDeviation + leaseDuration);

            var challengerLease = new ElasticLease(id, leaseExpiration, targetLeaseOwner);

            return this.TryLockContact(id, challengerLease, ctx, out newLease);
        }

        protected virtual LockAttemptStatus TryLockContact(
            Guid id,
            ElasticLease challengerLease,
            ISystemContext ctx,
            out ElasticLease newLease)
        {
            var status = this.contactLeaseService.TryObtainLock(challengerLease, ctx, out newLease) ?
                                           LockAttemptStatus.Success :
                                           LockAttemptStatus.AlreadyLocked;
            return status;
        }
    }
}
