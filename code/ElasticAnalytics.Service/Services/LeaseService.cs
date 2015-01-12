namespace ElasticAnalytics.Service.Services
{
    using System;
    using ElasticAnalytics.Model.Locking;
    using ElasticAnalytics.Repository.Types.Repositories;
    using ElasticAnalytics.Service.Types;
    using ElasticAnalytics.Utils.DateTime;

    public class LeaseService : ILeaseService
    {
        private readonly IDateTimeController dateTime;

        private readonly IConcurrencyControlRepository<ElasticLease> leaseRepository;

        public LeaseService(
            IDateTimeController dateTime,
            IConcurrencyControlRepository<ElasticLease> leaseRepository)
        {
            if (leaseRepository == null)
            {
                throw new ArgumentNullException("leaseRepository");
            }

            if (dateTime == null)
            {
                throw new ArgumentNullException("dateTime");
            }

            this.leaseRepository = leaseRepository;
            this.dateTime = dateTime;
        }

        public virtual ElasticLease Get(Guid resourceId, ISystemContext ctx)
        {
            return this.leaseRepository.Get(resourceId, ctx);
        }

        public virtual bool TryObtainLock(
            ElasticLease challenger, 
            ISystemContext ctx, 
            out ElasticLease winner)
        {
            // NOTE: The approach here is to fail immediately if someone else already has the lock.  Else
            // we try and obtain the lock.  If in the very unlikely/unlucky scenario that a lock is obtained 
            // between the first check and then the grab, we try again (recurse).  The likely hood of many
            // recursions there for is very very unlikely, so there is currently no guard against that.

            var currentLease = this.leaseRepository.Get(challenger.ResourceId, ctx);

            // there is no current lease
            if (currentLease == null)
            {
                var esChallenger = challenger;
                if (this.leaseRepository.Create(esChallenger, ctx)) // 'create only' ES query so will fail if we were beaten to it
                {
                    winner = challenger;
                    return true;
                }
                else
                {
                    // someone just beat us to it, so have another go...
                    return this.TryObtainLock(challenger, ctx, out winner);
                }
            }

            // lease is already held, and not by the challenger...
            if (!currentLease.HasExpired(this.dateTime.UtcNow) && !currentLease.IsOwnedBy(challenger.Owner))
            {
                winner = currentLease;
                return false;    
            }

            // lease has either expired, or is held by the current challenger owner and needs extending...
            if (!currentLease.IsOwnedBy(challenger.Owner) ||
                (currentLease.IsOwnedBy(challenger.Owner) && currentLease.Expires >= challenger.Expires))
            {
                if (!this.leaseRepository.SaveOptimistically(challenger, ctx)) // ES (upsert) using optimistic concurrency, so will fail if we were beaten to it
                {
                    // someone just beat us to it, so have another go...
                    return this.TryObtainLock(challenger, ctx, out winner);
                }
            }

            winner = challenger;
            return true;
        }

        public virtual bool Release(Guid resourceId, ElasticLeaseOwner leaseOwner, ISystemContext ctx)
        {
            var lease = this.Get(resourceId, ctx);

            if (lease == null) // nothing to do here
                return true;

            if (lease.HasExpired(this.dateTime.UtcNow) ||
                lease.IsOwnedBy(leaseOwner))
            {
                return this.leaseRepository.DeleteOptimistically(resourceId, lease.Version, ctx);
            }

            return false;
        }

        public virtual bool Extend(ElasticLease lease, TimeSpan leaseDuration, ISystemContext ctx, out ElasticLease currentLease)
        {
            var newLease = new ElasticLease(lease.ResourceId, this.dateTime.UtcNow + leaseDuration, lease.Owner);
            return this.TryObtainLock(newLease, ctx, out currentLease);
        }

        public virtual void Delete(Guid resourceId, ISystemContext ctx)
        {
            // brutal delete
            this.leaseRepository.Delete(resourceId, ctx);
        }
    }
}